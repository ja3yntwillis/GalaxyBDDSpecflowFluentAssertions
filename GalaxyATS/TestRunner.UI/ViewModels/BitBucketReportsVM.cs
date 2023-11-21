using System.Collections.Generic;
using TestRunner.Utilities.DTOs;

namespace TestRunner.UI.ViewModels
{
    public class BitBucketReportsVM
    {
        public string BitbucketUsername { get; set; }
        public string BitbucketPassword { get; set; }
        public string ReportStartDate { get; set; }
        public string ReportEndDate { get; set; }
        public string ReportCutoffDate { get; set; }
        public string CherryPickType { get; set; }
        public List<CommitsDto> Commits = new List<CommitsDto>();
        public List<BranchesDto> Branches = new List<BranchesDto>();
        public List<CherryPickDto> CherryPicks = new List<CherryPickDto>();
        public bool IsCommitsReportAvailable { get; set; }
        public bool IsBranchesReportAvailable { get; set; }
        public bool IsCherryPicksReportAvailable { get; set; }
        public string CherryPickBaseBranch { get; set; }
        public string CherryPickTargetBranch { get; set; }
    }
}
