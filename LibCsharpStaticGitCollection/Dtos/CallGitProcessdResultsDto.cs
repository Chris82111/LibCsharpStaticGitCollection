namespace Chris82111.LibCsharpStaticGitCollection.Dtos
{
    public class CallGitProcessdResultsDto
    {
        public string StandardOutput { get; set; } = string.Empty;
        public string StandardError { get; set; } = string.Empty;
        public int ExitCode { get; set; } = -1;
    }
}
