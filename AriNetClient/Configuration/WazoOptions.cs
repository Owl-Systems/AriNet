namespace AriNetClient.Configuration
{
    public class WazoOptions
    {
        public const string SectionName = "Wazo";

        public string BaseUrl { get; set; }
        public string ApiVersion { get; set; } = "1.1";
        public string Username { get; set; }
        public string Password { get; set; }
        public bool VerifySsl { get; set; } = true;
        public int TimeoutSeconds { get; set; } = 30;
        public int RetryCount { get; set; } = 3;

        // AMI Settings
        public string AmiHost { get; set; }
        public int AmiPort { get; set; } = 5038;
        public string AmiUsername { get; set; }
        public string AmiPassword { get; set; }
    }
}
