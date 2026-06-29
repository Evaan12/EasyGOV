using Application.Interfaces;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Tesseract;

namespace Infrastructure.Services
{
    public class OcrService : IOcrService
    {
        private readonly string _tessDataPath;
        private readonly ILogger<OcrService> _logger;
        private static readonly SemaphoreSlim _tessSemaphore = new SemaphoreSlim(1, 1);

        public OcrService(ILogger<OcrService> logger)
        {
            _logger = logger;
            _tessDataPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "tessdata");

            if (!Directory.Exists(_tessDataPath))
            {
                Directory.CreateDirectory(_tessDataPath);
                _logger.LogWarning("tessdata directory did not exist. Created at: {Path}", _tessDataPath);
            }
        }

        public async Task<Dictionary<string, string>> ExtractTextAndFieldsAsync(Stream imageStream, string language = "eng+nep", CancellationToken cancellationToken = default)
        {
            var extractedFields = new Dictionary<string, string>();
            _logger.LogInformation("====== OCR PIPELINE STARTED ======");
            _logger.LogInformation("STEP 1: Starting OCR extraction for language(s): {Language}", language);

            try
            {
                var engPath = Path.Combine(_tessDataPath, "eng.traineddata");
                var nepPath = Path.Combine(_tessDataPath, "nep.traineddata");

                _logger.LogInformation("STEP 2: Checking tessdata paths. eng: {EngExists}, nep: {NepExists}", File.Exists(engPath), File.Exists(nepPath));

                if (!File.Exists(engPath) || (!File.Exists(nepPath) && language.Contains("nep")))
                {
                    extractedFields["Error"] = "OCR language models (tessdata) are missing from the server. Please ensure eng.traineddata and nep.traineddata are present.";
                    _logger.LogError("Missing required tessdata files.");
                    return extractedFields;
                }

                if (imageStream.CanSeek)
                {
                    imageStream.Position = 0;
                }

                _logger.LogInformation("STEP 3: Copying input stream to memory.");
                using var ms = new MemoryStream();
                await imageStream.CopyToAsync(ms, cancellationToken);
                var rawBytes = ms.ToArray();
                _logger.LogInformation("Input stream copied. Size: {Bytes} bytes.", rawBytes.Length);

                _logger.LogInformation("STEP 4: Decoding image using SkiaSharp to normalize format.");
                using var skBitmap = SKBitmap.Decode(rawBytes);
                if (skBitmap == null)
                {
                    extractedFields["Error"] = "Failed to load image for OCR processing. Format might be unsupported or corrupted.";
                    _logger.LogError("SkiaSharp failed to decode the raw bytes.");
                    return extractedFields;
                }
                _logger.LogInformation("SkiaSharp successfully decoded image. Width: {Width}, Height: {Height}", skBitmap.Width, skBitmap.Height);

                _logger.LogInformation("STEP 5: Encoding normalized image to JPEG to avoid Leptonica PNG issues.");
                using var normalizedImage = SKImage.FromBitmap(skBitmap);
                using var encodedData = normalizedImage.Encode(SKEncodedImageFormat.Jpeg, 95);
                var normalizedBytes = encodedData.ToArray();
                _logger.LogInformation("Normalized JPEG created. Size: {Bytes} bytes.", normalizedBytes.Length);

                string tempFilePath = Path.Combine(Path.GetTempPath(), $"ocr_{Guid.NewGuid():N}.jpg");
                await File.WriteAllBytesAsync(tempFilePath, normalizedBytes, cancellationToken);
                _logger.LogInformation("Saved normalized image to temporary file: {TempFilePath}", tempFilePath);

                string fullText = string.Empty;

                _logger.LogInformation("STEP 6: Attempting OCR via subprocess for process isolation.");
                bool subprocessSucceeded = false;

                try
                {
                    fullText = await RunTesseractSubprocessAsync(tempFilePath, language, cancellationToken);
                    if (fullText != null)
                    {
                        subprocessSucceeded = true;
                        _logger.LogInformation("STEP 6a: Subprocess OCR succeeded. Extracted {Length} characters.", fullText.Length);
                    }
                }
                catch (Exception subEx)
                {
                    _logger.LogWarning(subEx, "Subprocess OCR failed or executable was missing.");
                }

                if (!subprocessSucceeded)
                {
                    _logger.LogWarning("STEP 7: Subprocess Tesseract was not found. Bypassing in-process engine fallback to prevent a hard native CPU crash.");
                    extractedFields["Error"] = "Tesseract OCR engine is not fully initialized. To prevent native CPU instruction crashes, please download the Windows installer from UB Mannheim (https://github.com/UB-Mannheim/tesseract/wiki) and install it. Ensure it's in 'C:\\Program Files\\Tesseract-OCR' or added to your system PATH environment variables, then restart the application.";
                    return extractedFields;
                }

                try
                {
                    if (File.Exists(tempFilePath))
                    {
                        File.Delete(tempFilePath);
                        _logger.LogInformation("Temporary image file deleted.");
                    }
                }
                catch { }

                extractedFields["RawText"] = fullText ?? string.Empty;

                _logger.LogInformation("STEP 8: Beginning Regex pattern matching on extracted text.");
                bool isCitizenshipFront = fullText!.Contains("नेपाली नागरिकताको प्रमाणपत्र") || fullText.Contains("ना. प्र. नं.");
                bool isCitizenshipBack = fullText.Contains("Citizenship Certificate") || fullText.Contains("Sex: Male") || fullText.Contains("Sex: Female");
                bool isNidForm = fullText.Contains("राष्ट्रिय परिचयपत्र सम्बन्धी आवेदन रूजु फाराम/भर्पाई") || fullText.Contains("First NID Card Request");
                bool isBirthCert = fullText.Contains("जन्म दर्ता प्रमाणपत्र") || fullText.Contains("Birth Registration Certificate");

                bool isNidCard = fullText.Contains("NATIONAL IDENTITY CARD") && !isNidForm;

                if (isNidCard)
                {
                    _logger.LogWarning("Detected actual NID card which is unsupported.");
                    extractedFields["Error"] = "Actual National ID cards are not supported because vital information cannot be reliably extracted. Please upload the National ID Card Request Form (Paper).";
                    return extractedFields;
                }

                if (!isCitizenshipFront && !isCitizenshipBack && !isNidForm && !isBirthCert)
                {
                    _logger.LogWarning("Document type could not be identified based on OCR text.");
                    extractedFields["Error"] = "Document type could not be strictly identified. Please ensure the document is a valid Citizenship, Birth Certificate, or NID Paper Form, and the image is clear.";
                    return extractedFields;
                }

                if (isCitizenshipFront)
                {
                    _logger.LogInformation("Identified document as Citizenship Front.");
                    var match = Regex.Match(fullText, @"ना\.?\s*प्र\.?\s*नं\.?\s*[:\-]?\s*([०-९0-9\-]+)");
                    if (match.Success) extractedFields["DocumentNumber"] = match.Groups[1].Value.Trim();

                    var nameMatch = Regex.Match(fullText, @"नाम थर[:\s]*([^\n]+)");
                    if (nameMatch.Success) extractedFields["FullName"] = nameMatch.Groups[1].Value.Trim();
                }
                else if (isCitizenshipBack)
                {
                    _logger.LogInformation("Identified document as Citizenship Back.");
                    var ctzNoMatch = Regex.Match(fullText, @"Certificate\s*No\.?\s*[:\-]?\s*([0-9\-]+)", RegexOptions.IgnoreCase);
                    if (ctzNoMatch.Success) extractedFields["DocumentNumber"] = ctzNoMatch.Groups[1].Value.Trim();

                    var fullNameMatch = Regex.Match(fullText, @"Full\s*Name\s*[:\-]?\s*([A-Za-z\s]+)", RegexOptions.IgnoreCase);
                    if (fullNameMatch.Success && !fullNameMatch.Groups[1].Value.Contains("Sex", StringComparison.OrdinalIgnoreCase))
                    {
                        var name = fullNameMatch.Groups[1].Value.Split(new[] { "Sex", "Date" }, StringSplitOptions.None)[0].Trim();
                        extractedFields["FullName"] = name;
                    }

                    var distMatch = Regex.Match(fullText, @"District\s*[:\-]?\s*([A-Za-z]+)", RegexOptions.IgnoreCase);
                    if (distMatch.Success) 
                    {
                        extractedFields["District"] = distMatch.Groups[1].Value.Trim();
                    }
                }
                else if (isBirthCert)
                {
                    _logger.LogInformation("Identified document as Birth Certificate.");
                    var birthRegMatch = Regex.Match(fullText, @"Registration\s*No\.?\)--\s*[:\-]?\s*([०-९0-9\-]+)", RegexOptions.IgnoreCase);
                    if (birthRegMatch.Success) extractedFields["DocumentNumber"] = birthRegMatch.Groups[1].Value.Trim();

                    var birthNameMatch = Regex.Match(fullText, @"Full\s*Name\s*[:\-]?\s*([A-Za-z\s]+)", RegexOptions.IgnoreCase);
                    if (birthNameMatch.Success)
                    {
                        extractedFields["FullName"] = birthNameMatch.Groups[1].Value.Trim();
                    }
                    else
                    {
                        var nepaliNameMatch = Regex.Match(fullText, @"पूरा\s*नाम\s*[:\-]?\s*([^\n]+)");
                        if (nepaliNameMatch.Success) extractedFields["FullName"] = nepaliNameMatch.Groups[1].Value.Trim();
                    }
                }
                else if (isNidForm)
                {
                    _logger.LogInformation("Identified document as NID Form.");
                    var ninMatch = Regex.Match(fullText, @"राष्ट्रिय\s*परिचय\s*नम्बर\s*[:\-]?\s*([०-९0-9\-]+)");
                    if (ninMatch.Success)
                    {
                        extractedFields["DocumentNumber"] = ninMatch.Groups[1].Value.Trim();
                    }
                    else
                    {
                        var englishNinMatch = Regex.Match(fullText, @"[0-9]{3}\-[0-9]{3}\-[0-9]{3}\-[0-9]");
                        if (englishNinMatch.Success) extractedFields["DocumentNumber"] = englishNinMatch.Value;
                    }

                    var firstNameMatch = Regex.Match(fullText, @"First\s*Name\s*[:\-]?\s*([A-Za-z]+)", RegexOptions.IgnoreCase);
                    var lastNameMatch = Regex.Match(fullText, @"Last\s*Name\s*[:\-]?\s*([A-Za-z]+)", RegexOptions.IgnoreCase);

                    if (firstNameMatch.Success && lastNameMatch.Success)
                    {
                        extractedFields["FullName"] = $"{firstNameMatch.Groups[1].Value.Trim()} {lastNameMatch.Groups[1].Value.Trim()}";
                    }
                    else
                    {
                        var fullNameMatch = Regex.Match(fullText, @"Full\s*Name\s*[:\-]?\s*([A-Za-z\s]+)", RegexOptions.IgnoreCase);
                        if (fullNameMatch.Success) extractedFields["FullName"] = fullNameMatch.Groups[1].Value.Trim();
                    }
                }

                _logger.LogInformation("STEP 9: Processing completed successfully.");
                _logger.LogInformation("====== OCR PIPELINE FINISHED ======");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FATAL EXCEPTION in OCR processing pipeline.");
                extractedFields["Error"] = $"OCR processing failed. The system caught a critical failure during extraction. Detailed trace: {ex.Message}";
            }

            return extractedFields;
        }

        private async Task<string?> RunTesseractSubprocessAsync(string imagePath, string language, CancellationToken cancellationToken)
        {
            string? tesseractExe = FindTesseractExecutable();
            if (tesseractExe == null)
            {
                _logger.LogWarning("Tesseract executable not found for subprocess approach.");
                return null;
            }

            _logger.LogInformation("Found tesseract executable at: {Path}", tesseractExe);

            string outputBasePath = Path.Combine(Path.GetTempPath(), $"ocr_out_{Guid.NewGuid():N}");

            var psi = new ProcessStartInfo
            {
                FileName = tesseractExe,
                Arguments = $"\"{imagePath}\" \"{outputBasePath}\" -l {language} --oem 1", // OEM 1 is Neural nets LSTM only
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetTempPath()
            };

            psi.Environment["TESSDATA_PREFIX"] = _tessDataPath;

            using var process = new Process();
            process.StartInfo = psi;

            try
            {
                _logger.LogInformation("Starting tesseract subprocess...");
                process.Start();

                var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
                var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(60));

                await process.WaitForExitAsync(timeoutCts.Token);

                var stderr = await stderrTask;
                if (!string.IsNullOrWhiteSpace(stderr))
                {
                    _logger.LogInformation("Tesseract stderr: {Stderr}", stderr.Length > 500 ? stderr[..500] : stderr);
                }

                if (process.ExitCode != 0)
                {
                    _logger.LogWarning("Tesseract subprocess exited with code {ExitCode}", process.ExitCode);
                    return null;
                }

                string outputFilePath = outputBasePath + ".txt";
                if (File.Exists(outputFilePath))
                {
                    var text = await File.ReadAllTextAsync(outputFilePath, cancellationToken);
                    try { File.Delete(outputFilePath); } catch { }
                    return text;
                }

                _logger.LogWarning("Tesseract output file not found at {Path}", outputFilePath);
                return null;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Tesseract subprocess timed out, killing process.");
                try { process.Kill(entireProcessTree: true); } catch { }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to run tesseract subprocess.");
                try { if (!process.HasExited) process.Kill(entireProcessTree: true); } catch { }
                return null;
            }
        }

        private string? FindTesseractExecutable()
        {
            var possiblePaths = new List<string>();
            string baseDir = AppContext.BaseDirectory;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                possiblePaths.Add(@"C:\Program Files\Tesseract-OCR\tesseract.exe");
                possiblePaths.Add(@"C:\Program Files (x86)\Tesseract-OCR\tesseract.exe");

                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (!string.IsNullOrEmpty(localAppData))
                {
                    possiblePaths.Add(Path.Combine(localAppData, "Tesseract-OCR", "tesseract.exe"));
                }

                var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
                foreach (var dir in pathEnv.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    possiblePaths.Add(Path.Combine(dir.Trim(), "tesseract.exe"));
                }

                possiblePaths.Add(Path.Combine(baseDir, "x64", "tesseract50.exe"));
                possiblePaths.Add(Path.Combine(baseDir, "x86", "tesseract50.exe"));
                possiblePaths.Add(Path.Combine(baseDir, "tesseract.exe"));
                possiblePaths.Add(Path.Combine(baseDir, "runtimes", "win-x64", "native", "tesseract50.exe"));
            }
            else
            {
                possiblePaths.Add("tesseract");
                possiblePaths.Add("/usr/bin/tesseract");
                possiblePaths.Add("/usr/local/bin/tesseract");
                possiblePaths.Add("/opt/homebrew/bin/tesseract");
            }

            foreach (var path in possiblePaths)
            {
                try
                {
                    if (Path.IsPathRooted(path))
                    {
                        if (File.Exists(path))
                        {
                            return path;
                        }
                    }
                    else if (CanRunCommand(path))
                    {
                        return path;
                    }
                }
                catch
                {
                }
            }

            return null;
        }

        private bool CanRunCommand(string command)
        {
            try
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = command,
                        Arguments = "--version",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit(1000);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<string> RunTesseractInProcessAsync(string tempFilePath, string language, CancellationToken cancellationToken)
        {
            _logger.LogWarning("In-process initialization was bypassed dynamically to prevent native CPU crashes.");
            return await Task.FromResult(string.Empty);
        }
    }
}