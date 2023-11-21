using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TestRunner.UI.DTOs;
using TestRunner.UI.Services;
using TestRunner.UI.Services.Interfaces;
using TestRunner.UI.ViewModels;
using TestRunner.Utilities;

namespace TestRunner.UI.Controllers
{
    public class ReportController : Controller
    {
        public IReadResults ResultsReader { get; }
        public IAZClient AZClient { get; }

        public ReportController(IReadResults resultsReader, IAZClient aZClient)
        {
            ResultsReader = resultsReader;
            AZClient = aZClient;
        }

        [HttpGet, Route("Report/release/{releaseId}")]
        public IActionResult ReleaseReport(string releaseId)
        {
            ReleaseReportVM viewModel = null;
            string appName = null;

            var releaseInfo = AZClient.GetRelease(releaseId);

            var artifact = releaseInfo.Artifacts.Where(x => x.Alias.Equals("BC") || x.Alias.Equals("BA") || x.Alias.Equals("BM")).FirstOrDefault();
            string buildId = Regex.Match(artifact.definitionReference.BuildUri.Id, "\\d+$").Value;
            var buildInfo = AZClient.GetBuild(buildId);

            if (artifact.Alias.ToLower() == "bc")
            {
                appName = "BrightChoices";
            }
            else
            {
                appName = "Benefit Access";
            }

            TimeSpan buildTime = buildInfo.finishTime.Subtract(buildInfo.startTime);
            TimeSpan queueTime = buildInfo.startTime.Subtract(buildInfo.queueTime);
            var seleniumRunEnv = releaseInfo.Environments.Where(x => x.Name.ToLower().Contains("_serun")).FirstOrDefault();
            TimeSpan releaseDeployTime = seleniumRunEnv.CreatedOn.Subtract(releaseInfo.CreatedOn);

            viewModel = new ReleaseReportVM()
            {
                ApplicationName = appName,
                RunDate = releaseInfo.CreatedOn.ToLocalTime().ToString("MMM d"),
                RunTime = releaseInfo.CreatedOn.ToLocalTime().ToString("h:mm tt"),
                BuildTime = $"{buildTime.Hours} Hour {buildTime.Minutes} Min {buildTime.Seconds} Sec",
                BuildQueuedTime = $"{queueTime.Hours} Hour {queueTime.Minutes} Min {queueTime.Seconds} Sec",
                ReleaseDeployTime = $"{releaseDeployTime.Hours} Hour {releaseDeployTime.Minutes} Min {releaseDeployTime.Seconds} Second",
                BuildUrl = $"https://liazon.visualstudio.com/BrightChoices/_build/results?buildId={buildInfo.Id}",
                ReleaseUrl = $"https://liazon.visualstudio.com/BrightChoices/_releaseProgress?_a=release-pipeline-progress&releaseId={releaseInfo.Id}"
            };

            ViewBag.AppType = RunnerConfiguration.ApplicationType; // We need to find a better way then this for navigation bar
            return View(viewModel);
        }

        [HttpGet, Route("Report/run/{runId}")]
        public IActionResult RunReport(string runId)
        {
            RunReportVM viewModel = null;
            string appName = null;

            var runDto = ResultsReader.GetRun(runId);
            var executedTests = ResultsReader.GetTests(runId);

            var passedTests = executedTests.Where(x => x.Status == "Passed").Count();
            var blockedTests = executedTests.Where(x => x.Status == "Blocked").Count();
            var startedTests = executedTests.Where(x => x.Status == "Started").Count();
            var failedTests = executedTests.Where(x => x.Status == "Failed").Count();
            var queuedTests = executedTests.Where(x => x.Status == "Queued").Count();

            string releaseId = Regex.Match(runDto.Labels, "rid_(\\d+).*").Groups[1].ToString();
            var releaseInfo = AZClient.GetRelease(releaseId);

            var artifact = releaseInfo.Artifacts.Where(x => x.Alias.Equals("BC") || x.Alias.Equals("BA") || x.Alias.Equals("BM")).FirstOrDefault();
            string buildId = Regex.Match(artifact.definitionReference.BuildUri.Id, "\\d+$").Value;
            var buildInfo = AZClient.GetBuild(buildId);

            var commitInfo = AZClient.GetCommit(buildId);

            if (artifact.Alias.ToLower() == "bc")
            {
                appName = "BrightChoices";
            }
            else
            {
                appName = "Benefit Access";
            }

            TimeSpan buildTime = buildInfo.finishTime.Subtract(buildInfo.startTime);
            TimeSpan queueTime = buildInfo.startTime.Subtract(buildInfo.queueTime);

            AZReleaseEnvironmentDto seleniumRunEnv;
            TimeSpan releaseDeployTime;
            TimeSpan seleniumRunTime;

            seleniumRunEnv = releaseInfo.Environments.Where(x => x.Name.ToLower().Contains("poctest") && x.Status.ToLower() == "succeeded").FirstOrDefault();
            if (seleniumRunEnv != null)
            {
                var deploymentStep = seleniumRunEnv.deploySteps.Where(x => x.Status.ToLower() == "succeeded").FirstOrDefault();
                var deployEnvironmentPhase = deploymentStep.releaseDeployPhases.Where(x => x.Name.ToLower().Contains("web servers")).FirstOrDefault();
                var deployTestsPhase = deploymentStep.releaseDeployPhases.Where(x => x.Name.ToLower().Contains("_serun")).FirstOrDefault();

                releaseDeployTime = deployTestsPhase.startedOn.Subtract(deployEnvironmentPhase.startedOn);
                seleniumRunTime = seleniumRunEnv.ModifiedOn.Subtract(deployTestsPhase.startedOn);
            }
            else
            {
                seleniumRunEnv = releaseInfo.Environments.Where(x => x.Name.ToLower().Contains("_serun")).FirstOrDefault();
                releaseDeployTime = seleniumRunEnv.CreatedOn.Subtract(releaseInfo.CreatedOn);
                seleniumRunTime = seleniumRunEnv.ModifiedOn.Subtract(seleniumRunEnv.CreatedOn);
            }

            viewModel = new RunReportVM()
            {
                ApplicationName = appName,
                RunDate = releaseInfo.CreatedOn.ToLocalTime().ToString("MMM d"),
                RunTime = releaseInfo.CreatedOn.ToLocalTime().ToString("h:mm tt"),
                TotalTestCount = runDto.TestsCount,
                PassedTestCount = passedTests,
                FailedTestCount = failedTests,
                BlockedTestCount = blockedTests,
                QueuedTestCount = queuedTests,
                StartedTestCount = startedTests,
                RunResult = $"Total - {runDto.TestsCount} / Passed - {passedTests} / Failed - {failedTests} / Started - {startedTests} / Blocked - {blockedTests}",
                BuildHours = buildTime.Hours,
                BuildMinutes = buildTime.Minutes,
                BuildSeconds = buildTime.Seconds,
                BuildTime = $"{buildTime.Hours} Hour {buildTime.Minutes} Min {buildTime.Seconds} Sec",
                BuildQueuedTime = $"{queueTime.Hours} Hour {queueTime.Minutes} Min {queueTime.Seconds} Sec",
                DeployHours = releaseDeployTime.Hours,
                DeployMinutes = releaseDeployTime.Minutes,
                DeploySeconds = releaseDeployTime.Seconds,
                ReleaseDeployTime = $"{releaseDeployTime.Hours} Hour {releaseDeployTime.Minutes} Min {releaseDeployTime.Seconds} Second",
                ExecutionHours = seleniumRunTime.Hours,
                ExecutionMinutes = seleniumRunTime.Minutes,
                ExecutionSeconds = seleniumRunTime.Seconds,
                TestExecutionTime = $"{seleniumRunTime.Hours} Hour {seleniumRunTime.Minutes} Min {seleniumRunTime.Seconds} Second",
                RunResultURL = $"https://automation-dashboard.participantportal.com/RunResult/{runId}",
                BuildUrl = $"https://liazon.visualstudio.com/BrightChoices/_build/results?buildId={buildInfo.Id}",
                ReleaseUrl = $"https://liazon.visualstudio.com/BrightChoices/_releaseProgress?_a=release-pipeline-progress&releaseId={releaseInfo.Id}",
                CommitUrl = $"https://liazon.visualstudio.com/_git/BrightChoices/commit/{commitInfo.commitId}",
                PullRequestUrl = $"https://liazon.visualstudio.com/_git/BrightChoices/pullrequest/{commitInfo.pullRequestId}?_a=overview"
            };

            ViewBag.AppType = RunnerConfiguration.ApplicationType; // We need to find a better way then this for navigation bar
            return View(viewModel);
        }

        [HttpGet, Route("Report/Table/{url}")]
        public IActionResult TableReport(string url)
        {
            OverviewTableReportDto model = null;
            var viewModel = new OverviewTableReportVM();
            viewModel.RunReports = new List<OverviewTableReportDto>();
            var runId = "";
            var runList = ResultsReader.GetRuns(url, string.Empty, string.Empty).Where(x => x.Labels.Contains("rid")).TakeLast(20).ToList();

            foreach (var run in runList)
            {
                runId = run.RunId;

                var runDto = ResultsReader.GetRun(runId);
                var executedTests = ResultsReader.GetTests(runId);

                var passedTests = executedTests.Where(x => x.Status == "Passed").Count();
                var blockedTests = executedTests.Where(x => x.Status == "Blocked").Count();
                var startedTests = executedTests.Where(x => x.Status == "Started").Count();
                var failedTests = executedTests.Where(x => x.Status == "Failed").Count();
                var queuedTests = executedTests.Where(x => x.Status == "Queued").Count();

                var passedTestPercent = (double)passedTests / executedTests.Count();

                string releaseId = Regex.Match(runDto.Labels, "rid_(\\d+).*").Groups[1].ToString();
                var releaseInfo = AZClient.GetRelease(releaseId);

                var artifact = releaseInfo.Artifacts.Where(x => x.Alias.Equals("BC") || x.Alias.Equals("BA") || x.Alias.Equals("BM")).FirstOrDefault();
                string buildId = Regex.Match(artifact.definitionReference.BuildUri.Id, "\\d+$").Value;
                var buildInfo = AZClient.GetBuild(buildId);

                var commitInfo = AZClient.GetCommit(buildId);

                model = new OverviewTableReportDto()
                {
                    RunId = runId,
                    RunDate = releaseInfo.CreatedOn.ToLocalTime().ToString("MM/dd/yyyy"),
                    RunTime = releaseInfo.CreatedOn.ToLocalTime().ToString("h:mm tt"),
                    TotalTestCount = runDto.TestsCount,
                    PassedTestCount = passedTests,
                    FailedTestCount = failedTests,
                    BlockedTestCount = blockedTests,
                    QueuedTestCount = queuedTests,
                    StartedTestCount = startedTests,
                    RunResultURL = $"https://automation-dashboard.participantportal.com/RunResult/{runId}",
                    BuildId = buildId,
                    BuildUrl = $"https://liazon.visualstudio.com/BrightChoices/_build/results?buildId={buildInfo.Id}",
                    ReleaseUrl = $"https://liazon.visualstudio.com/BrightChoices/_releaseProgress?_a=release-pipeline-progress&releaseId={releaseInfo.Id}",
                    CommitUrl = $"https://liazon.visualstudio.com/_git/BrightChoices/commit/{commitInfo.commitId}",
                    PullRequestId = commitInfo.pullRequestId,
                    PullRequestUrl = $"https://liazon.visualstudio.com/_git/BrightChoices/pullrequest/{commitInfo.pullRequestId}?_a=overview",
                    PassedTestPercent = passedTestPercent.ToString("P", CultureInfo.InvariantCulture),
                    ReleaseId = releaseId,
                    Committer = commitInfo.committer.name,
                };

                viewModel.RunReports.Add(model);
            }

            ViewBag.AppType = RunnerConfiguration.ApplicationType; // We need to find a better way then this for navigation bar
            return View(viewModel);
        }

        [HttpGet, Route("Report/developHealth")]
        public IActionResult DevelopHealthReport()
        {
            DevelopHealthReportDto model = null;
            var viewModel = new DevelopHealthReportVM();
            viewModel.RunReports = new List<DevelopHealthReportDto>();
            var runId = "";
            var runListBC = ResultsReader.GetRuns("http://bctest1.grpeapp.com", string.Empty, string.Empty).Where(x => x.Labels.Contains("rid")).TakeLast(10).ToList();
            var runListBA = ResultsReader.GetRuns("http://batest1-adm.grpeapp.com", string.Empty, string.Empty).Where(x => x.Labels.Contains("rid")).TakeLast(10).ToList();

            foreach (var run in runListBC)
            {
                runId = run.RunId;

                var runDto = ResultsReader.GetRun(runId);
                var executedTests = ResultsReader.GetTests(runId);

                var passedTests = executedTests.Where(x => x.Status == "Passed").Count();

                var passedTestPercent = (double)passedTests / executedTests.Count();

                string releaseId = Regex.Match(runDto.Labels, "rid_(\\d+).*").Groups[1].ToString();
                var releaseInfo = AZClient.GetRelease(releaseId);

                var artifact = releaseInfo.Artifacts.Where(x => x.Alias.Equals("BC") || x.Alias.Equals("BA") || x.Alias.Equals("BM")).FirstOrDefault();
                string buildId = Regex.Match(artifact.definitionReference.BuildUri.Id, "\\d+$").Value;

                var commitInfo = AZClient.GetCommit(buildId);

                model = new DevelopHealthReportDto()
                {
                    RunIdBC = runId,
                    RunResultURLBC = $"https://automation-dashboard.participantportal.com/RunResult/{runId}",
                    BuildId = buildId,
                    BuildUrl = $"https://liazon.visualstudio.com/BrightChoices/_build/results?buildId={buildId}",
                    ReleaseUrlBC = $"https://liazon.visualstudio.com/BrightChoices/_releaseProgress?_a=release-pipeline-progress&releaseId={releaseInfo.Id}",
                    CommitUrl = $"https://liazon.visualstudio.com/_git/BrightChoices/commit/{commitInfo.commitId}",
                    PullRequestId = commitInfo.pullRequestId,
                    PullRequestUrl = $"https://liazon.visualstudio.com/_git/BrightChoices/pullrequest/{commitInfo.pullRequestId}?_a=overview",
                    PassedTestPercentBC = passedTestPercent.ToString("P", CultureInfo.InvariantCulture),
                    ReleaseIdBC = releaseId,
                    Committer = commitInfo.committer.name,
                };

                viewModel.RunReports.Add(model);
            }

            foreach (var run in runListBA)
            {
                runId = run.RunId;

                var runDto = ResultsReader.GetRun(runId);
                var executedTests = ResultsReader.GetTests(runId);

                var passedTests = executedTests.Where(x => x.Status == "Passed").Count();

                var passedTestPercent = (double)passedTests / executedTests.Count();

                string releaseId = Regex.Match(runDto.Labels, "rid_(\\d+).*").Groups[1].ToString();
                var releaseInfo = AZClient.GetRelease(releaseId);

                var artifact = releaseInfo.Artifacts.Where(x => x.Alias.Equals("BC") || x.Alias.Equals("BA") || x.Alias.Equals("BM")).FirstOrDefault();
                string buildId = Regex.Match(artifact.definitionReference.BuildUri.Id, "\\d+$").Value;

                model = viewModel.RunReports.Where(x => x.BuildId == buildId).SingleOrDefault();
                if (model != null)
                {
                    model.RunIdBA = runId;
                    model.RunResultURLBA = $"https://automation-dashboard.participantportal.com/RunResult/{runId}";
                    model.ReleaseUrlBA = $"https://liazon.visualstudio.com/BrightChoices/_releaseProgress?_a=release-pipeline-progress&releaseId={releaseInfo.Id}";
                    model.PassedTestPercentBA = passedTestPercent.ToString("P", CultureInfo.InvariantCulture);
                    model.ReleaseIdBA = releaseId;
                }
                else
                {
                    var commitInfo = AZClient.GetCommit(buildId);

                    model = new DevelopHealthReportDto()
                    {
                        RunIdBA = runId,
                        RunResultURLBA = $"https://automation-dashboard.participantportal.com/RunResult/{runId}",
                        BuildId = buildId,
                        BuildUrl = $"https://liazon.visualstudio.com/BrightChoices/_build/results?buildId={buildId}",
                        ReleaseUrlBA = $"https://liazon.visualstudio.com/BrightChoices/_releaseProgress?_a=release-pipeline-progress&releaseId={releaseInfo.Id}",
                        CommitUrl = $"https://liazon.visualstudio.com/_git/BrightChoices/commit/{commitInfo.commitId}",
                        PullRequestId = commitInfo.pullRequestId,
                        PullRequestUrl = $"https://liazon.visualstudio.com/_git/BrightChoices/pullrequest/{commitInfo.pullRequestId}?_a=overview",
                        PassedTestPercentBA = passedTestPercent.ToString("P", CultureInfo.InvariantCulture),
                        ReleaseIdBA = releaseId,
                        Committer = commitInfo.committer.name,
                    };

                    viewModel.RunReports.Add(model);
                }
            }

            ViewBag.AppType = RunnerConfiguration.ApplicationType; // We need to find a better way then this for navigation bar
            return View(viewModel);
        }

        [HttpGet, Route("Report/LoadTesting")]
        public IActionResult LoadTesting()
        {
            if (RunnerConfiguration.ApplicationType == "local")
            {
                return Redirect("/");
            }

            var azTableReader = new AZStorageRESTResultsReader();

            var model = azTableReader.GetLoadTests();

            ViewBag.AppType = RunnerConfiguration.ApplicationType; // We need to find a better way then this for navigation bar

            return View(model);
        }

        [HttpPost, Route("Report/LoadTestSubmit")]
        public IActionResult LoadTestSubmit(IFormFile loadTestFile, string releaseNumber)
        {
            var counter = 0;

            var headers = new List<string>();

            var resultSet = new List<Dictionary<string, string>>();

            if (loadTestFile != null)
            {
                try
                {
                    if (loadTestFile.FileName.EndsWith(".csv"))
                    {
                        var partitionKey = Path.GetFileNameWithoutExtension(loadTestFile.FileName);

                        using (var reader = new StreamReader(loadTestFile.OpenReadStream()))
                        {
                            headers = reader.ReadLine().Split(",").ToList<string>();

                            while (!reader.EndOfStream)
                            {
                                var tempDict = new Dictionary<string, string>();

                                var row = reader.ReadLine().Split(",");

                                for (var i = 0; i < row.Count(); i++)
                                {
                                    if (headers[i] == "TestName")
                                    {
                                        tempDict.Add("PartitionKey", row[i]);
                                    }
                                    else
                                    {
                                        tempDict.Add(headers[i], row[i]);
                                    }
                                }

                                tempDict.Add("RowKey", $"{releaseNumber}|{counter++}");

                                var resultsLogger = new AZStorageRESTResultsLogger();

                                var success = resultsLogger.SaveLoadTestRow(tempDict);

                                resultSet.Add(tempDict);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return Redirect("LoadTesting");
        }

        [HttpGet, Route("Report/LoadTesting/{testName}")]
        public IActionResult LoadTestingReport(string testName)
        {
            if (RunnerConfiguration.ApplicationType == "local")
            {
                return Redirect("/");
            }

            var azTableReader = new AZStorageRESTResultsReader();

            var azData = azTableReader.GetLoadTestData(testName);

            var releaseData = azData.Value.Select(x => x.RowKey.Remove(4));

            var releaseNumbers = releaseData.Distinct().ToList();

            var contents = new List<Content>();

            foreach (var release in releaseNumbers)
            {
                foreach (var value in azData.Value)
                {
                    if (release == value.RowKey.Remove(4))
                    {
                        var content = new Content();
                        content.Release = value.RowKey.Remove(4);
                        content.Area = value.Area;
                        content.Time = value.AvgTime;

                        contents.Add(content);
                    }
                }
            }

            var areaData = azData.Value.Select(x => x.Area);

            var areas = areaData.Distinct().ToList();

            var model = new LoadTestReportVM()
            {
                ReleaseNumbers = releaseNumbers,
                Areas = areas,
                Contents = contents,
            };

            ViewBag.AppType = RunnerConfiguration.ApplicationType; // We need to find a better way then this for navigation bar

            return View(model);
        }
    }
}
