namespace TestRunner.Utilities.DTOs
{
    public class BranchesDto
    {
        public string BranchName { get; set; }
        public string LastCommitId { get; set; }
        public string LastCommitDate { get; set; }
        public string LastCommitter { get; set; }
        public string CommitterEmail { get; set; }
    }
}
