namespace TestRunner.UI.DTOs
{
    public class DevelopHealthReportDto
    {
        public string RunIdBC { get; set; }
        public string RunIdBA { get; set; }
        public string ReleaseIdBC { get; set; }
        public string ReleaseIdBA { get; set; }
        public string PullRequestId { get; set; }
        public string BuildId { get; set; }
        public string RunResultURLBC { get; set; }
        public string RunResultURLBA { get; set; }
        public string BuildUrl { get; set; }
        public string ReleaseUrlBC { get; set; }
        public string ReleaseUrlBA { get; set; }
        public string CommitUrl { get; set; }
        public string PullRequestUrl { get; set; }
        public string PassedTestPercentBC { get; set; }
        public string PassedTestPercentBA { get; set; }
        public string Committer { get; set; }
    }
}
