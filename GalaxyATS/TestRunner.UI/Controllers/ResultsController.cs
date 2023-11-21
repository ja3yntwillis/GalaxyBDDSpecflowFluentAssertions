using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TestRunner.UI.DTOs;
using TestRunner.UI.Services.Interfaces;
using TestRunner.UI.ViewModels;
using TestRunner.Utilities;
using TestRunner.Utilities.DTOs;

namespace TestRunner.UI.Controllers
{
    public class ResultsController : Controller
    {
        public IReadResults ResultsReader { get; }

        public ResultsController(IReadResults resultsReader)
        {
            ResultsReader = resultsReader;
        }

        public static List<RunDto> runList = new List<RunDto>();

        [HttpGet, Route("RunResult")]
        public IActionResult RunList(string url, DateTime startDate, DateTime endDate)
        {
            var start = startDate.ToUniversalTime().ToString("yyyyMMddHHmmssfffffff");
            var end = endDate.ToUniversalTime().AddDays(1).AddTicks(-1).ToString("yyyyMMddHHmmssfffffff");

            ViewBag.AppType = RunnerConfiguration.ApplicationType; // We need to find a better way then this for navigation bar
            if (RunnerConfiguration.ApplicationType == "dashboard" && url == null)
            {
                var urlList = ResultsReader.GetBaseUrls();
                var environmentListVM = new EnvironmentListVM()
                {
                    Environments = urlList.OrderBy(x => x.RowKey).ToList(),
                };

                return View("EnvironmentList", environmentListVM);
            }
            else if (url != null)
            {
                var invalidUrl = Regex.IsMatch(url, @"[^\da-zA-Z\.\/:\-]", RegexOptions.None);
                if (invalidUrl)
                {
                    url = null;
                }
            }
            
            runList = ResultsReader.GetRuns(url, start, end);
            var runListVM = new RunListVM()
            {
                Runs = runList,
            };

            runListVM.Runs.ForEach(x => x.StartTime = x.StartTime.ToLocalTime());

            return View(runListVM);
        }

        [HttpGet, Route("DailyReport")]
        public IActionResult DailyReport(string url, DateTime startDate, DateTime endDate)
        {
            ViewBag.AppType = RunnerConfiguration.ApplicationType;
            List<string> labelsOfDailyRuns = new List<string>
            {
                "ScheduledAPIEntStg",
                "ScheduledUIEntStg",
                "SalesDemo",
                "ScheduledUISbx",
                "ScheduledAPISbx",
                "ScheduledAPIDev",
                "ScheduledUIDev",
                "SchedulediOSDev",
                "SchedulediOSStage",
                "ScheduledEntDevAndroid",
                "ScheduledEntStageAndroid",
                "SchedulediOSSbx",
                "SchedulediOSDemo",
                "ScheduledUIDemo",
                "ScheduledAPIDemo",
                "ScheduledRetiree",
                "ScheduledAdminSanityEus",
                "ScheduledEUSAPI",
                "ScheduledBatch",
                "ScheduledBatchLongRunning",
                "ScheduledDB"
            };
            runList = ResultsReader.GetDailyRuns(labels: labelsOfDailyRuns);
            var dailyReportVM = new RunListVM()
            {
                Runs = runList,
            };

            return View(dailyReportVM);
        }

        /// <summary>
        /// Returns a view of Run results by Label and time period or number of hours. Dates are defaulted to blank. The hours is defaulted to 0.
        /// </summary>
        /// <param name="label">Run label</param>
        /// <param name="startDate">Date from which to fetch run records</param>
        /// <param name="endDate">Date up to which to fetch run records</param>
        /// <param name="hours">Number of hours from current time up to which to fetch run records, defaulted to zero</param>
        /// <returns>List of runs for the specified run label and time period</returns>
        [HttpGet, Route("RunResult/trigger")]
        public IActionResult LabelRuns(string label, string startDate, string endDate, int hours = 0)
        {
            ViewBag.AppType = RunnerConfiguration.ApplicationType;
            if (RunnerConfiguration.ApplicationType == "dashboard" && label == null)
            {
                var urlList = ResultsReader.GetBaseUrls();
                var environmentListVM = new EnvironmentListVM()
                {
                    Environments = urlList.OrderBy(x => x.RowKey).ToList(),
                };

                return View("EnvironmentList", environmentListVM);
            }
            
            var runList = new List<RunDto>();
            if (hours > 0)
            {
                runList = ResultsReader.GetLabelRuns(label, hours);
            }
            else
            {
                runList = ResultsReader.GetLabelRuns(label, startDate, endDate);
            }
            var runListVM = new RunListVM()
            {
                Runs = runList,
            };

            runListVM.Runs.ForEach(x => x.StartTime = x.StartTime.ToLocalTime());
            return View(runListVM);
        }

        /// <summary>
        /// Returns RunID for a failing Run or Passed for a run where all tests pass
        /// </summary>
        /// <param name="label">Run label</param>
        /// <param name="hours">Number of hours</param>
        /// <returns></returns>
        [HttpGet, Route("RunResult/FailedLabelRun")]
        public string GetFailedLabelRun(string label, int hours = 1)
        {
            Console.WriteLine("***Inside Results Controller - RunResult/FailedLabelRun Call***");
            string result = "Passed";
           
            //Get list of runs based on label and hour
            var runList = ResultsReader.GetLabelRuns(label, hours);

            Console.WriteLine("***After GetLabelRuns call, writing from Results Controller - RunResult/FailedLabelRun Call***");

            //Check if runList is null
            if (runList == null)
            {
                Console.WriteLine("***RunList is null, writing from Results Controller - inside runList null check***");
                return "Null RunList";
            }
            
            //Check latest runId for failing tests
            var runId = runList.LastOrDefault().RunId;

            Console.WriteLine($"***Before IsFailedRun call, Latest RunId is - {runId} - RunResult/FailedLabelRun Call***");

            if (ResultsReader.IsFailedRun(runId))
            {
                Console.WriteLine($"***After IsFailedRun call, failed RunId is - {runId} - RunResult/FailedLabelRun Call***");
                result = runId;
            }

            Console.WriteLine($"***Before retrning from Results Controller result is - {result} - RunResult/FailedLabelRun Call***");

            return  result;
        }

        /// <summary>
        /// Gets the number of passing tests during a duration for a specific labeled run
        /// </summary>
        /// <param name="label">Run label</param>
        /// <param name="hours">Number of hours</param>
        /// <returns></returns>
        [HttpGet, Route("RunResult/PassingCount")]
        public int GetPassingRunCount(string label, int hours)
        {
            int count = 0;
            var testList = ResultsReader.GetLabelTests(label, hours);

            //check if testList is null
            if (testList == null)
                return count;

            //Check test status
            count = testList.FindAll(t => t.Status.Equals("Passed")).Count;
            return count;
        }

        [HttpGet, Route("RunResult/{runId}")]
        public IActionResult RunResult(string runId)
        {
            RunResultVM viewModel = null;
            var latestModifiedKey = ResultsReader.GetLastModified(runId);
            TimeSpan execution;

            if (latestModifiedKey == "summary")
            {
                viewModel = ResultsReader.GetRunSummary(runId);
                execution = viewModel.EndTime.Subtract(viewModel.StartTime);
                viewModel.ExecutionTime = new TimeSpan(execution.Ticks / TimeSpan.TicksPerSecond * TimeSpan.TicksPerSecond);
                viewModel.NonPassedTestCount = viewModel.QueuedCount + viewModel.StartedCount + viewModel.BlockedCount;
                viewModel.RunDate = viewModel.StartTime.ToString("M/d/yyyy");
                viewModel.RunEnvironment = GetEnvironment(viewModel.BaseUrl);
                viewModel.StartTime = viewModel.StartTime.ToLocalTime();
                viewModel.EndTime = viewModel.EndTime.ToLocalTime();
            }
            else
            {
                var runDto = ResultsReader.GetRun(runId);
                var allTestsNames = runDto.Tests.ToList();

                var executedTests = ResultsReader.GetTests(runId);
                var executedTestNames = executedTests.Select(x => x.Name).ToList();

                var queuedTests = allTestsNames.Except(executedTestNames).OrderBy(x => x).ToList();
                var passedTests = executedTests.Where(x => x.Status == "Passed").OrderBy(x => x.Name).ToList();
                var blockedTests = executedTests.Where(x => x.Status == "Blocked").OrderBy(x => x.Name).ToList();
                var startedTests = executedTests.Where(x => x.Status == "Started").ToList();
                var failedTestDtos = executedTests.Where(x => x.Status == "Failed").OrderBy(x => x.Name).ToList();

                var (failedTests, failedTestsByMessage) = OrganizeFailedTests(runId, failedTestDtos);

                var nonPassedTests = new List<string>();
                nonPassedTests.AddRange(failedTests.Select(x => $"{x.Name}"));
                nonPassedTests.AddRange(queuedTests);
                nonPassedTests.AddRange(blockedTests.Select(x => $"{x.Name}"));
                nonPassedTests.AddRange(startedTests.Select(x => $"{x.Name}"));

                var nonFinishedTestCount = queuedTests.Count + blockedTests.Count + startedTests.Count;

                bool isReleaseId = Regex.IsMatch(runDto.Labels, "rid_\\d+");
                string runReport = $"/Report/run/{runId}";

                var executionTime = runDto.EndTime.Subtract(runDto.StartTime);

                viewModel = new RunResultVM()
                {
                    Application = runDto.Application,
                    SuiteType = runDto.SuiteType,
                    BaseUrl = runDto.BaseUrl,
                    Browser = runDto.Browser,
                    RunId = runId,
                    RunEnvironment = GetEnvironment(runDto.BaseUrl),
                    RunDate = runDto.StartTime.ToString("M/d/yyyy"),
                    Threads = runDto.MaxThreads,
                    TestCount = runDto.Tests.Count,
                    StartTime = runDto.StartTime.ToLocalTime(),
                    EndTime = runDto.EndTime.ToLocalTime(),
                    ExecutionTime = new TimeSpan(executionTime.Ticks / TimeSpan.TicksPerSecond * TimeSpan.TicksPerSecond),
                    IsReleaseId = isReleaseId,
                    RunReportUrl = runReport,
                    PassedTests = passedTests,
                    PassedCount = passedTests.Count,
                    FailedTests = failedTests,
                    FailedCount = failedTests.Count,
                    FailedTestsByMessage = failedTestsByMessage,
                    BlockedTests = blockedTests,
                    BlockedCount = blockedTests.Count,
                    StartedTests = startedTests,
                    StartedCount = startedTests.Count,
                    QueuedTests = queuedTests,
                    QueuedCount = queuedTests.Count,
                    NonPassedTests = nonPassedTests,
                    NonPassedTestCount = nonFinishedTestCount,
                    DashboardUrl = runDto.ServerDashboardId != null ? string.Format("https://automation-dashboard.participantportal.com/RunResult/{0}", runDto.ServerDashboardId) : null,
                    Labels = runDto.Labels,
                };

                ResultsReader.SetRunSummary(runId, viewModel);
            }

            ViewBag.AppType = RunnerConfiguration.ApplicationType; // We need to find a better way then this for navigation bar
            return View(viewModel);
        }

        [HttpGet, Route("RunResultJson/{runId}")]
        public IActionResult RunResultJson(string runId)
        {
            var runDto = ResultsReader.GetRun(runId);
            var allTestsNames = runDto.Tests.ToList();

            var executedTests = ResultsReader.GetTests(runId);
            var executedTestNames = executedTests.Select(x => x.Name).ToList();

            var queuedTests = allTestsNames.Except(executedTestNames).OrderBy(x => x).ToList();
            var passedTests = executedTests.Where(x => x.Status == "Passed").OrderBy(x => x.Name).ToList();
            var blockedTests = executedTests.Where(x => x.Status == "Blocked").Select(x => x.Name).OrderBy(x => x).ToList();
            var startedTests = executedTests.Where(x => x.Status == "Started").Select(x => x.Name).ToList();
            var failedTestDtos = executedTests.Where(x => x.Status == "Failed").OrderBy(x => x.Name).ToList();

            var (failedTests, failedTestsByMessage) = OrganizeFailedTests(runId, failedTestDtos);

            var nonPassedTests = new List<string>();
            nonPassedTests.AddRange(failedTests.Select(x => $"{x.Name}"));
            nonPassedTests.AddRange(queuedTests);
            nonPassedTests.AddRange(blockedTests);
            nonPassedTests.AddRange(startedTests);

            return Json(new
            {
                PassedTests = passedTests.Select(x => new
                {
                    x.TestId,
                    x.Name,
                }),
                FailedTestsByMessage = failedTestsByMessage.ToDictionary(x => x.Key, x => x.Value.Select(y => new
                {
                    y.TestId,
                    y.Name,
                    y.ErrorType,
                    y.Url,
                })),
                FailedTests = failedTests.Select(x => new
                {
                    x.TestId,
                    x.Name,
                    x.ErrorType,
                    x.Message,
                }),
                BlockedTests = blockedTests,
                StartedTests = startedTests,
                QueuedTests = queuedTests,
                EndTime = runDto.EndTime.ToUniversalTime().ToString("o"),
                NonPassedTests = nonPassedTests,
            });
        }

        [HttpGet, Route("RunResult/{runId}/submit")]
        public IActionResult PushRunResult(string runId)
        {
            RunnerConfiguration.LogApiURL = "";
            //var logger = new RestClientResultsLogger();
            //TODO uncomment and delete line above to switch to new dashboard
            var logger = new DashboardRESTResultsLogger();
            logger.SendFileSystemResultsToServer(runId, RunnerConfiguration.FileStoragePath);
            return Redirect("/RunResult/" + runId);
        }

        [HttpGet, Route("RunResult/{runId}/TestResult/{testId}")]
        public IActionResult TestResult(string runId, string testId)
        {
            var testDto = ResultsReader.GetTest(runId, testId);

            var viewModel = new TestResultVM()
            {
                Attempts = testDto.Attempts,
                EndTime = testDto.EndTime.ToLocalTime(),
                Assembly = testDto.Assembly,
                Fixture = testDto.Fixture,
                Method = testDto.Method,
                TestDescription = testDto.TestDescription,
                RunId = runId,
                StartTime = testDto.StartTime.ToLocalTime(),
                Status = testDto.Status,
                TestId = testId,
            };

            var testDataDto = ResultsReader.GetTestData(runId, testId);

            if (testDataDto?.Any() != false)
            {
                viewModel.DataTree = testDataDto;
            }

            if (!string.Equals(testDto.Status, "passed", StringComparison.OrdinalIgnoreCase))
            {
                viewModel.ErrorType = testDto.ErrorType;
                viewModel.Message = testDto.Message;
                viewModel.PageTitle = testDto.PageTitle;
                viewModel.ScreenshotBase64 = testDto.ScreenshotBase64;
                viewModel.Url = testDto.Url;
                var traceStr = testDto.Trace;

                if (testDto.ErrorType == null)
                {
                    var errorDto = ResultsReader.GetTestError(runId, testId);
                    viewModel.ErrorType = errorDto.ErrorType;
                    viewModel.Message = errorDto.Message;
                    viewModel.Url = errorDto.Url;
                    viewModel.ScreenshotBase64 = errorDto.ScreenshotBase64;
                    traceStr = errorDto.Trace;
                }

                // ToDo (Niravkumar/Matt/Saurabh): Check for the regular expression logic.
                //Regex reg = new Regex(@"(at )LZAuto\.(?:Framework.(?:PageObject|Roles|PageObjects|Actions)|Tests|Utility).(.*?in )(?:.*?)SeleniumAutomation((?:.*?)\d+)");
                //var mat = reg.Matches(traceStr);
                List<string> newTrace = new List<string>();
                //foreach (Match match in mat)
                //{
                //    newTrace.Add(string.Format("{0}{1}{2}", match.Groups[1], match.Groups[2], match.Groups[3]));
                //}
                //if (newTrace.Count == 0)
                //{
                //    reg = new Regex(@"at LZAuto\..*?\)");
                //    mat = reg.Matches(traceStr);
                //    foreach (Match match in mat)
                //    {
                        newTrace.Add(traceStr);
                //    }
                //}

                viewModel.Trace = newTrace;
            }

            ViewBag.AppType = RunnerConfiguration.ApplicationType; // We need to find a better way then this for navigation bar

            /***************************Get the last ten runs for the current test****************************/
            List<TestDto> testList = new List<TestDto>();
            testList = ResultsReader.GetLastNTests(testDto.Name).OrderByDescending(t => t.EndTime).ToList();
            testList.RemoveAll(t => t.TestId.Equals(testDto.TestId));
            testList.ForEach(t => t.Url = $"/RunResult/{t.PartitionKey}/TestResult/{t.RowKey}");
            viewModel.ResultList = testList;

            return View(viewModel);
        }

        [HttpGet, Route("RunResult/{runId}/ExecutionTimeSummary")]
        public IActionResult ExecutionTimeSummary(string runId)
        {
            var runDto = ResultsReader.GetRun(runId);
            var executionTime = runDto.EndTime.Subtract(runDto.StartTime);
            var executedTests = ResultsReader.GetTests(runId);
            var TotalTestExecTimes = new List<TestExecTimeDto>();
            foreach (var test in executedTests)
            {
                var testExecTime = new TestExecTimeDto();
                testExecTime.TestId = test.TestId;
                testExecTime.Assembly = test.Assembly;
                testExecTime.Fixture = test.Fixture;
                testExecTime.Method = test.Method;
                testExecTime.Status = test.Status;
                testExecTime.StartTime = test.StartTime;
                testExecTime.EndTime = test.EndTime;
                TotalTestExecTimes.Add(testExecTime);
            }

            ExecutionTimeSummaryVM viewModel = new ExecutionTimeSummaryVM()
            {
                RunId = runId,
                RunDate = runDto.StartTime.ToString("M/d/yyyy"),
                ThreadCount = runDto.MaxThreads,
                Browser = runDto.Browser,
                TestCount = runDto.Tests.Count,
                StartTime = runDto.StartTime,
                EndTime = runDto.EndTime,
                ExecutionTime = new TimeSpan(executionTime.Ticks / TimeSpan.TicksPerSecond * TimeSpan.TicksPerSecond),
                TotalTests = TotalTestExecTimes
            };

            return View(viewModel);
        }

        private (List<FailedTestDto> failedTests, Dictionary<string, List<FailedTestDto>> failedTestsByMessage) OrganizeFailedTests(string runId, List<TestDto> failedTestDtos)
        {
            var failedTests = new List<FailedTestDto>();
            var failedTestsByMessage = new Dictionary<string, List<FailedTestDto>>();
            foreach (var testFile in failedTestDtos)
            {
                var testFailure = new FailedTestDto()
                {
                    Assembly = testFile.Assembly,
                    Fixture = testFile.Fixture,
                    Method = testFile.Method,
                    TestId = testFile.TestId,
                    ErrorType = testFile.ErrorType,
                    Message = testFile.Message,
                    Url = testFile.Url,
                    Status = testFile.Status,
                };
                if (testFile.ErrorType == null)
                {
                    var errorDto = ResultsReader.GetTestError(runId, testFile.TestId);
                    testFailure.ErrorType = errorDto.ErrorType;
                    testFailure.Message = errorDto.Message;
                    testFailure.Url = errorDto.Url;
                }
                testFailure.GeneralizedMessage = testFailure.Message;
                testFailure.GeneralizedMessage = Regex.Replace(testFailure.GeneralizedMessage, "http://localhost:.*/session/.*/", "http://localhost:*/session/*/", RegexOptions.Multiline);
                testFailure.GeneralizedMessage = Regex.Replace(testFailure.GeneralizedMessage, "TESTVEN.*\\)", "*)", RegexOptions.Multiline);
                testFailure.GeneralizedMessage = Regex.Replace(testFailure.GeneralizedMessage, "ZCGroup .*", "ZCGroup *", RegexOptions.Multiline);
                testFailure.GeneralizedMessage = Regex.Replace(testFailure.GeneralizedMessage, "'Test.*'", "'Test*", RegexOptions.Multiline);
                testFailure.GeneralizedMessage = Regex.Replace(testFailure.GeneralizedMessage, "\\(\\d*\\)", "(*, *)", RegexOptions.Multiline);
                testFailure.GeneralizedMessage = Regex.Replace(testFailure.GeneralizedMessage, "\\(Session info(.|[\r\n])*", "", RegexOptions.Multiline);

                if (failedTestsByMessage.TryGetValue(testFailure.GeneralizedMessage, out var failedTestDto))
                {
                    failedTestDto.Add(testFailure);
                }
                else
                {
                    failedTestsByMessage.Add(testFailure.GeneralizedMessage, new List<FailedTestDto> { testFailure });
                }

                failedTests.Add(testFailure);
            }

            return (failedTests, failedTestsByMessage);
        }

        public string GetEnvironment(string url){
            string environment = null;
            switch (url)
            {
                case "https://ent-dev.participantportal.com":
                case "https://dev-mobile-ig.participantportal.com/dev":
                    environment = "ENT-DEV";
                    break;
                case "https://ent-stg.participantportal.com":
                case "https://mobile-ig.participantportal.com/stg":
                    environment = "ENT-STG";
                    break;
                case "https://ent-uat.participantportal.com":
                case "https://dev-mobile-ig.participantportal.com/uat":
                    environment = "ENT-UAT";
                    break;
                case "https://eus-stg.viabenefitsaccounts.com":
                case "https://mobile-ig.participantportal.com/stu":
                    environment = "EUS-STG";
                    break;
                case "https://app-automate.browserstack.com":
                    environment = "MOBILE";
                    break;
            }
            return environment;
        }
    }
}
