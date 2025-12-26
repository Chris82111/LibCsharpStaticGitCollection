namespace Chris82111.LibCsharpStaticGitCollection.Dtos
{
    /// <summary>
    /// Data transfer object for passing on information about the system processes used.
    /// </summary>
    public class CallGitProcessdResultsDto
    {
        /// <summary>
        /// Standard output of the application.
        /// </summary>
        public string StandardOutput { get; set; } = string.Empty;

        /// <summary>
        /// Standard error of the application.
        /// </summary>
        public string StandardError { get; set; } = string.Empty;

        /// <summary>
        /// Gets the value that was specified by the associated process when it was terminated.
        /// </summary>
        public int ExitCode { get; set; } = -1;
    }
}
