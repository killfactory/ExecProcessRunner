namespace Killfactory.Tools.Exec
{
    public class ProcessExecutionResult
    {
        public int ExitCode { get; set; }
        public string Output { get; set; }
        public string ErrorOutput { get; set; }
    }
}
