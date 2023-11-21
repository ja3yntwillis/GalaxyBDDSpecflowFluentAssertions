using System.Collections.Generic;

namespace Framework.Galaxy.Dtos
{
    /// <summary>
    /// DTO for the information received about commits from BitBucket API
    /// </summary>
    public class BitBucketBranchesResponseDto
    {
        public int size { get; set; }
        public int limit { get; set; }
        public bool isLastPage { get; set; }
        public List<Branch> values { get; set; }
        public int start { get; set; }
        public int nextPageStart { get; set; }
    }

    /// <summary>
    /// Class that contains the details of the branches
    /// </summary>
    public class Branch
    {
        public string id { get; set; }
        public string displayId { get; set; }
        public string type { get; set; }
        public string latestCommit { get; set; }
        public string latestChangeset { get; set; }
        public bool isDefault { get; set; }
        public string lastCommitDate { get; set; }
        public Author author { get; set; }
    }
}