namespace DiagnosKit.Core.Settings
{
    public class DiagnosKitESSettings
    {
        public string Url { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string IndexFormat { get; set; } = string.Empty;
        public string? IndexPrefix { get; set; }
    }
}
