namespace TestRunner.UI.DTOs
{
    public class AZCommitDto
    {
        public string commitId { get; set; }
        public string comment { get; set; }
        public string pullRequestId { get; set; }
        public CommitterDto committer { get; set; }
    }

    public class CommitterDto
    {
        public string name { get; set; }
    }
}
