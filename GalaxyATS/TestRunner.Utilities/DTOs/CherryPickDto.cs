namespace TestRunner.Utilities.DTOs
{
    public class CherryPickDto
    {
        public string PullRequestTitle { get; set; }
        public string Contributor { get; set; }
        public string BaseBranchCommitDate { get; set; }
        public string BaseBranchCommitId { get; set; }
        public string IsCherryPicked { get; set; }
        public string CherryPickedBy { get; set; }
        public string TargetBranchCommitDate { get; set; }
        public string TargetBranchCommitId { get; set; }
        public string JiraId { get; set; }
        public string JiraStatus { get; set; }
    }
}
