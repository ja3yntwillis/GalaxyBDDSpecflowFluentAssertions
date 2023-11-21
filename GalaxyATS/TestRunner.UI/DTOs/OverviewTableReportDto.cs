namespace TestRunner.UI.DTOs
{
    public class OverviewTableReportDto
    {
        public string RunId { get; set; }
        public string ReleaseId { get; set; }
        public string PullRequestId { get; set; }
        public string BuildId { get; set; }
        public string RunDate { get; set; }
        public string RunTime { get; set; }
        public string RunResultURL { get; set; }
        public string BuildUrl { get; set; }
        public string ReleaseUrl { get; set; }
        public string CommitUrl { get; set; }
        public string PullRequestUrl { get; set; }
        public int TotalTestCount { get; set; }
        public int PassedTestCount { get; set; }
        public int FailedTestCount { get; set; }
        public int BlockedTestCount { get; set; }
        public int QueuedTestCount { get; set; }
        public int StartedTestCount { get; set; }
        public int BuildHours { get; set; }
        public int BuildMinutes { get; set; }
        public int BuildSeconds { get; set; }
        public int DeployHours { get; set; }
        public int DeployMinutes { get; set; }
        public int DeploySeconds { get; set; }
        public int ExecutionHours { get; set; }
        public int ExecutionMinutes { get; set; }
        public int ExecutionSeconds { get; set; }
        public string PassedTestPercent { get; set; }
        public string Committer { get; set; }
    }
}
