namespace Infrastructure.Telephony.Options
{
    public class TingTingOptions
    {
        public const string SectionName = "TingTing";
        public string BaseUrl { get; set; } = "https://app.tingting.io/api/v1/";
        public string ApiToken { get; set; } = string.Empty;
    }
}