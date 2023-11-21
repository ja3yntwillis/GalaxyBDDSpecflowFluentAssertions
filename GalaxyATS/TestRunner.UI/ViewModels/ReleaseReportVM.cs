namespace TestRunner.UI.ViewModels
{
    public class ReleaseReportVM
    {
        public string ApplicationName { get; set; }
        public string RunDate { get; set; }
        public string RunTime { get; set; }
        public string BuildTime { get; set; }
        public string BuildQueuedTime { get; set; }
        public string ReleaseDeployTime { get; set; }
        public string BuildUrl { get; set; }
        public string ReleaseUrl { get; set; }
    }
}
