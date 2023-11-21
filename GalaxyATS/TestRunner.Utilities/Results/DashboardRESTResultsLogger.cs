using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TestRunner.Utilities.DTOs;
using System.Text.Json;
using System.Net.Http;
using Flurl.Http;

namespace TestRunner.Utilities
{
    public class DashboardRESTResultsLogger : ILogResults
    {

        public string _runId = "0";
        private RunDto runDto;
        [ThreadStatic]
        private string placeHolder = "";
        public List<AssertDTO> assertDto = new List<AssertDTO>();
        private List<string> failedTests = new List<string>();
        public string totalTestCount;


        public void buildDataJson(object data, int parentId)
        {
            throw new NotImplementedException();
        }

        public DataTreeDto buildDataTree(object data)
        {
            DataTreeDto dataDto = new DataTreeDto();
            return dataDto;
        }

        public void EndRun(RunDto runDto)
        {

            var uri = string.Format("api/results/run/{0}", runDto.RunId);
            Call<RunDto>(HttpMethod.Put, uri, runDto, ref placeHolder);
        }

        public TestDto EndTest(string runId, string testId, string status, int attempts, string testData, ErrorDto errorDto, ActionDto actionDto, DateTime endTime)
        {
            var uri = $"api/results/run/{_runId}/test/{testId}";
            var test = new TestDto()
            {
                TestId = testId,
                Attempts = attempts,
                EndTime = endTime,
                Status = status,
            };
            if (errorDto != null)
            {
                test.ErrorType = errorDto.ErrorType;
                test.Message = errorDto.Message;
                test.PageTitle = errorDto.PageTitle;
                test.ScreenshotBase64 = errorDto.ScreenshotBase64;
                test.Trace = errorDto.Trace;
                test.Url = errorDto.Url;
            }

            Call<TestDto>(HttpMethod.Put, uri, test, ref placeHolder);
            var failCount = assertDto.Where(c => c.IsPass == false).Count();
            if (status == "Passed" && failCount > 0)
            {
                var failedAssertCount = 1;
                foreach (var item in assertDto)
                {
                    if (!item.IsPass)
                    {
                        failedAssertCount++;
                    }
                }
            }
            assertDto.Clear();

            return test;
        }

        public string GetTestDescription(string methodName)
        {
            throw new NotImplementedException();
        }

        public List<string> FailedTests()
        {
            throw new NotImplementedException();
        }

        public string GetActionDocumentation(string actionMemberName)
        {
            throw new NotImplementedException();
        }

        public List<string> GetAllTestsFromRun(int runId, List<string> testStatusList)
        {
            throw new NotImplementedException();
        }

        public void LogAction(string message, string memberName, string filePath)
        {
            throw new NotImplementedException();
        }

        public void LogError(string errorType, string message, string pageTitle, string screenshotBase64, string trace, string url)
        {
            throw new NotImplementedException();
        }

        public void LogInfo(string infoMessage)
        {
            throw new NotImplementedException();
        }

        public void LogScreenshot(string screenshotBase64, string pageTitle, string url, string groupId)
        {
            throw new NotImplementedException();
        }

        public void RetryTest()
        {
            throw new NotImplementedException();
        }

        public void SetRunId(int runId)
        {
            throw new NotImplementedException();
        }

        public void SetTestId(int testId)
        {
            throw new NotImplementedException();
        }

        public RunDto StartRun(string url, string application, string attribute, string suiteType, string browser, int threads, string labels, List<string> tests, DateTime startTime, string dbName, string dbUsername)
        {
            runDto = new TestRunner.Utilities.DTOs.RunDto()
            {
                Application = application,
                Attribute = attribute,
                SuiteType = suiteType,
                BaseUrl = url,
                Browser = browser,
                UserName = Environment.UserName,
                StartTime = startTime,
                EndTime = DateTime.UtcNow,
                MaxThreads = threads,
                Labels = labels,
                Tests = tests,
                TestsCount = tests.Count,
                DatabaseName = dbName,
                DatabaseUsername = dbUsername,
            };
            var uri = "api/results/run";
            Call<RunDto>(HttpMethod.Post, uri, runDto, ref _runId);
            runDto.RunId = _runId;
            runDto.PartitionKey = _runId;
            runDto.RowKey = "run";
            failedTests.Clear();
            return runDto;
        }

        public string StartTest(string runId, string testId, string assembly, string fixture, string method, DateTime startTime, string labels)
        {
            var testDto = new TestDto()
            {
                TestId = testId,
                Assembly = assembly,
                Fixture = fixture,
                Method = method,
                Status = "Started",
                StartTime = startTime,
                Labels = labels
            };
            var uri = string.Format("api/results/run/{0}/test", _runId);
            Call<TestDto>(HttpMethod.Post, uri, testDto, ref testId);
            return testId;
        }

        public void SendFileSystemResultsToServer(string runId, string resultsDirectory)
        {
            var filePathFormat = string.Format("{0}{2}{1}", resultsDirectory, runId, Path.DirectorySeparatorChar);
            var runFileContent = System.IO.File.ReadAllText(string.Format("{0}{2}{1}", filePathFormat, "run.json", Path.DirectorySeparatorChar));
            var runDto = JsonSerializer.Deserialize<RunDto>(runFileContent);
            runDto.BaseUrl = runDto.BaseUrl.Replace("#", "");
            var remoteRunDto = StartRun(runDto.BaseUrl, runDto.Application, runDto.Attribute, runDto.SuiteType, runDto.Browser, runDto.MaxThreads, runDto.Labels, runDto.Tests, runDto.StartTime, runDto.DatabaseName, runDto.DatabaseUsername);
            remoteRunDto.TestsCount = runDto.TestsCount;
            remoteRunDto.PassedCount = runDto.PassedCount;
            remoteRunDto.FailedCount = runDto.FailedCount;
            var labels = runDto.Labels;

            try
            {
                var executedTestsFiles = Directory.GetFiles(filePathFormat, "*test.json");
                var executedTests = executedTestsFiles.Select(x => JsonSerializer.Deserialize<TestRunner.Utilities.DTOs.TestDto>(System.IO.File.ReadAllText(x))).ToList();
                foreach (var testFile in executedTests)
                {
                    var remoteTestId = StartTest(remoteRunDto.RunId, testFile.TestId, testFile.Assembly, testFile.Fixture, testFile.Method, testFile.StartTime, labels);
                    //var dataFile = JsonSerializer.Deserialize<List<TestRunner.Utilities.DTOs.DataTreeDto>>(System.IO.File.ReadAllText(string.Format("{0}\\{1}{2}", filePathFormat, testFile.TestId, "_data.json")));
                    ErrorDto errorFile = null;
                    if (File.Exists(string.Format("{0}{3}{1}{2}", filePathFormat, testFile.TestId, "_error.json", Path.DirectorySeparatorChar)))
                    {
                        errorFile = JsonSerializer.Deserialize<TestRunner.Utilities.DTOs.ErrorDto>(System.IO.File.ReadAllText(string.Format("{0}{3}{1}{2}", filePathFormat, testFile.TestId, "_error.json", Path.DirectorySeparatorChar)));
                    }
                    EndTest(remoteRunDto.RunId, remoteTestId, testFile.Status, testFile.Attempts, null, errorFile, null, runDto.EndTime);
                }
                EndRun(remoteRunDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            runDto.ServerDashboardId = remoteRunDto.RunId;
            runFileContent = JsonSerializer.Serialize(runDto);
            System.IO.File.WriteAllText(string.Format("{0}{2}{1}", filePathFormat, "run.json", Path.DirectorySeparatorChar), runFileContent);
        }

        private void Call<T>(HttpMethod httpVerb, string uri, T dataObject, ref string _refVar)
        {
            try
            {
                var jsonContent = new StringContent(JsonSerializer.Serialize(dataObject), Encoding.UTF8, "application/json");
                HttpResponseMessage response = null;
                if (httpVerb == HttpMethod.Post)
                {
                    response = $"{RunnerConfiguration.LoggingAPIUrl}/{uri}".PostJsonAsync(dataObject)
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult()
                        .ResponseMessage;

                    var respContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    //Add the if-else block to figure out if respContent contains the testId, or the run json. If it's runjson in string format, it will contain :
                    if (respContent.Contains(":"))
                    {
                        var run = JsonSerializer.Deserialize<RunDto>(respContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        _refVar = run.RunId;
                    }
                    else
                    {
                        _refVar = JsonSerializer.Deserialize<string>(respContent);

                    }
                }
                else if (httpVerb == HttpMethod.Put)
                {
                    response = $"{RunnerConfiguration.LoggingAPIUrl}/{uri}".PutJsonAsync(dataObject)
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult()
                        .ResponseMessage;
                }
                else if (httpVerb == HttpMethod.Get)
                {
                    _refVar = $"{RunnerConfiguration.LoggingAPIUrl}/{uri}".GetStringAsync()
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult();
                }
            }
            catch (FlurlHttpException e)
            {
                throw e;
            }
        }
    }
}
