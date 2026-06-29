namespace Infrastructure.Options
{
    public class LLMStudioSettings
    {
        public const string SectionName = "LLMStudioSettings";

        public string BaseUrl { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }
}