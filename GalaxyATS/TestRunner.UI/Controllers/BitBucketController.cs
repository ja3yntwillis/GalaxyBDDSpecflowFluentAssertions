using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Framework.Galaxy.Dtos;
using Framework.Galaxy;
using TestRunner.UI.ViewModels;
using TestRunner.Utilities.DTOs;

namespace TestRunner.UI.Controllers
{
    public class BitBucketController : Controller
    {
        private string bitbucketUser = "nirav483";
        private string repoSlug = "benefitsaccounts_automation";

        [HttpGet, Route("/BitBucketReports")]
        public IActionResult BitBucketReports()
        {
            var viewModel = new BitBucketReportsVM();
            return View(viewModel);
        }

        [HttpPost, Route("/GetBitBucketReport")]
        public IActionResult GetBitBucketReport(string reportType, string bitbucketUsername, string bitbucketPassword, string reportStartDate, string reportEndDate, 
                                        string branchCutoffDate, string targetBranch, string cherryPickReportType, string commitCutoffDate)
        {
            string bitbucketRepository = $"{bitbucketUser}/repos/{repoSlug}";
            BitBucketApiClient bitBucketApiClient = new BitBucketApiClient();
            List<CommitsDto> commitsDtos = new List<CommitsDto>();
            List<BranchesDto> branchesDtos = new List<BranchesDto>();
            List<CherryPickDto> cherryPickDtos = new List<CherryPickDto>();
            var baseBranch = reportType.Equals("PR") || reportType.Equals("BR") ? "develop" : targetBranch.Equals("trunk") ? "develop" : "trunk";
            
            if (reportType.Equals("PR"))
            {
                BitBucketCommitsResponseDto bitBucketCommitsDto = bitBucketApiClient.GetCommitsForSpecificTimePeriod(bitbucketRepository, bitbucketUsername, bitbucketPassword, 
                                        DateTime.Parse(reportStartDate, CultureInfo.InvariantCulture), DateTime.Parse(reportEndDate, CultureInfo.InvariantCulture));
                foreach (var bitBucketCommit in bitBucketCommitsDto.values)
                {
                    var commit = new CommitsDto();
                    commit.Contributor = bitBucketCommit.author.displayName == null || bitBucketCommit.author.displayName.Length == 0 ? bitBucketCommit.author.name : bitBucketCommit.author.displayName;
                    commit.CommitId = bitBucketCommit.displayId;
                    commit.PullRequestTitle = bitBucketCommit.message.Split(":")[0];
                    commit.CommitDate = bitBucketCommit.committerTimestamp;
                    commitsDtos.Add(commit);
                }
            }
            else if (reportType.Equals("BR"))
            {
                BitBucketBranchesResponseDto bitBucketBranchesDto = bitBucketApiClient.GetIdleBranchesForSpecificDate(bitbucketRepository, bitbucketUsername, bitbucketPassword, 
                                        DateTime.Parse(branchCutoffDate, CultureInfo.InvariantCulture));
                foreach (var bitBucketBranch in bitBucketBranchesDto.values)
                {
                    var branch = new BranchesDto();
                    branch.BranchName = bitBucketBranch.displayId;
                    branch.LastCommitId = bitBucketBranch.latestCommit;
                    branch.LastCommitDate = bitBucketBranch.lastCommitDate;
                    branch.LastCommitter = bitBucketBranch.author.displayName == null || bitBucketBranch.author.displayName.Length == 0 ? bitBucketBranch.author.name : bitBucketBranch.author.displayName;
                    branch.CommitterEmail = bitBucketBranch.author.emailAddress;

                    branchesDtos.Add(branch);
                }
            }
            else
            {
                JiraApiClient jiraApiClient = new JiraApiClient();
                BitBucketCommitsResponseDto baseBranchCommitsDto = bitBucketApiClient.GetCommitsForSpecificTimePeriod(bitbucketRepository, bitbucketUsername, bitbucketPassword, 
                                        commitCutoffDate == null ? DateTime.MinValue : DateTime.Parse(commitCutoffDate, CultureInfo.InvariantCulture), DateTime.Now, 
                                        baseBranch, "yyyy-MM-dd hh:mm:ss");
                BitBucketCommitsResponseDto targetBranchCommitsDto = bitBucketApiClient.GetCommitsForSpecificTimePeriod(bitbucketRepository, bitbucketUsername, bitbucketPassword, 
                                        commitCutoffDate == null ? DateTime.MinValue : DateTime.Parse(commitCutoffDate, CultureInfo.InvariantCulture), DateTime.Now, 
                                        targetBranch, "yyyy-MM-dd hh:mm:ss");

                //Creating a List of JIRA IDs associated with Base branch commit
                List<String> baseCommitsJiraIds = new List<string>();
                foreach (var baseBranchCommit in baseBranchCommitsDto.values)
                {
                    if (baseBranchCommit.properties != null)
                    {
                        List<String> relatedJiraIds = baseBranchCommit.properties.jiraKey;
                        foreach (var jiraId in relatedJiraIds)
                        {
                            if (jiraId.Contains("ATF"))
                            {
                                baseCommitsJiraIds.Add($"BUG-{jiraId.Substring(jiraId.LastIndexOf("-") + 1)}");
                            }
                            else
                            {
                                baseCommitsJiraIds.Add(jiraId);
                            }
                        }
                    }
                }

                //Collecting JIRA ids and their status for further processing
                Dictionary<string, string> baseBranchCommitsJiraData = jiraApiClient.GetBulkJiraIssueStatus(bitbucketUsername, bitbucketPassword, baseCommitsJiraIds);

                foreach (var baseBranchCommit in baseBranchCommitsDto.values)
                {
                    var cherryPick = new CherryPickDto();
                    cherryPick.PullRequestTitle = baseBranchCommit.message.Split(":")[0];
                    cherryPick.Contributor = baseBranchCommit.author.displayName == null || baseBranchCommit.author.displayName.Length == 0 ? baseBranchCommit.author.name : baseBranchCommit.author.displayName;
                    cherryPick.BaseBranchCommitDate = baseBranchCommit.committerTimestamp;
                    cherryPick.BaseBranchCommitId = baseBranchCommit.displayId;
                    cherryPick.IsCherryPicked = "No";
                    try
                    {
                        if (baseBranchCommit.properties != null)
                        {
                            List<string> jiraIDs = baseBranchCommit.properties.jiraKey;
                            for (int i = 0; i < jiraIDs.Count; i++)
                            {
                                try
                                {
                                    if (jiraIDs[i].Contains("ATF"))
                                    {
                                        jiraIDs[i] = $"BUG-{jiraIDs[i].Substring(jiraIDs[i].LastIndexOf("-") + 1)}";
                                    }

                                    if (i == 0)
                                    {
                                        cherryPick.JiraId = $"{jiraIDs[i]}";
                                        cherryPick.JiraStatus = baseBranchCommitsJiraData[jiraIDs[i]];
                                    }
                                    else
                                    {
                                        cherryPick.JiraId = $"{cherryPick.JiraId}, {jiraIDs[i]}";
                                        cherryPick.JiraStatus = $"{cherryPick.JiraStatus}, {baseBranchCommitsJiraData[jiraIDs[i]]}";
                                    }
                                }
                                catch (Exception)
                                {
                                    if (!jiraIDs[i].StartsWith("TC-"))
                                    {
                                        cherryPick.JiraStatus = $"{cherryPick.JiraStatus}, {jiraApiClient.GetJiraIssueStatus(bitbucketUsername, bitbucketPassword, jiraIDs[i])}";
                                    }
                                    else
                                    {
                                        Console.WriteLine($"JIRA data not present for ID : {jiraIDs[i]}");
                                    }
                                }
                            }
                            //This will remove any unwanted characters from start and end
                            cherryPick.JiraStatus = cherryPick.JiraStatus.Trim(',');
                        }                        
                    }
                    catch (Exception)
                    {
                    }
                    
                    bool isCherryPicked = false;
                    bool isReverted = false;
                    foreach (var targetBranchCommit in targetBranchCommitsDto.values)
                    {
                        string commitTitle = targetBranchCommit.message.Split(":")[0];
                        if (cherryPick.PullRequestTitle.Equals(commitTitle))
                        {
                            isCherryPicked = true;
                            if (cherryPickReportType.Equals("all"))
                            {
                                cherryPick.IsCherryPicked = "Yes";
                                cherryPick.CherryPickedBy = targetBranchCommit.committer.displayName == null || targetBranchCommit.committer.displayName.Length == 0 ? targetBranchCommit.committer.name : targetBranchCommit.committer.displayName;
                                cherryPick.TargetBranchCommitDate = targetBranchCommit.committerTimestamp;
                                cherryPick.TargetBranchCommitId = targetBranchCommit.displayId;
                            }
                            break;
                        }
                        else if (cherryPick.PullRequestTitle.Equals(commitTitle) && !baseBranchCommit.message.Split(":")[0].Equals(commitTitle))
                        {
                            isReverted = true;
                            cherryPick.IsCherryPicked = "No";
                            cherryPick.CherryPickedBy = targetBranchCommit.committer.displayName == null || targetBranchCommit.committer.displayName.Length == 0 ? targetBranchCommit.committer.name : targetBranchCommit.committer.displayName;
                            cherryPick.TargetBranchCommitDate = targetBranchCommit.committerTimestamp;
                            cherryPick.TargetBranchCommitId = targetBranchCommit.displayId;
                            break;
                        }
                    }

                    if (cherryPickReportType.Equals("all"))
                    {
                        cherryPickDtos.Add(cherryPick);
                    }
                    else
                    {
                        if (isReverted || !isCherryPicked)
                        {
                            cherryPickDtos.Add(cherryPick);
                        }
                    }
                }
            }
            
            var bitBucketReportsVM = new BitBucketReportsVM()
            {
                Commits = commitsDtos,
                Branches = branchesDtos,
                CherryPicks = cherryPickDtos,
                IsCommitsReportAvailable = reportType.Equals("PR") ? true : false,
                IsBranchesReportAvailable = reportType.Equals("BR") ? true : false,
                IsCherryPicksReportAvailable = reportType.Equals("CP") ? true : false,
                ReportStartDate = reportStartDate,
                ReportEndDate = reportEndDate,
                ReportCutoffDate = branchCutoffDate != null ? branchCutoffDate : commitCutoffDate,
                CherryPickType = cherryPickReportType,
                CherryPickBaseBranch = $"{char.ToUpper(baseBranch[0])}{baseBranch[1..]}",
                CherryPickTargetBranch = reportType.Equals("CP") ? $"{char.ToUpper(targetBranch[0])}{targetBranch[1..]}" : "Develop"
            };
            return View("BitBucketReports", bitBucketReportsVM);
        }
    }
}
