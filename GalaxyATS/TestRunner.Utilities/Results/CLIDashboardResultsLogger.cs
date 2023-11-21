using Flurl.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using TestRunner.Utilities.DTOs;

namespace TestRunner.Utilities
{
    public class CLIDashboardResultsLogger : ILogResults
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
            Call<RunDto>(HttpMethod.Put, uri, runDto, ref _runId);
            _runId = "0";
        }

        public TestDto EndTest(string runId, string testId, string status, int attempts, string testData, ErrorDto errorDto, ActionDto actionDto, DateTime endTime)
        {
            var uri = $"api/results/run/{runId}/test/{testId}";
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

        private void Call<T>(HttpMethod httpVerb, string uri, T dataObject, ref string _refVar)
        {
            HttpResponseMessage response;
            try
            {
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
            catch (Exception ex)
            {
                string name = ex.GetType().Name;
                string msg = ex.Message;
                string trace = ex.StackTrace;
                Console.WriteLine("Inside Exception while making Azure call.");
                if (httpVerb == HttpMethod.Put)
                {
                    TestDto test = (TestDto)(object)dataObject;
                    if (test.ErrorType != null)
                    {
                        test.Message = $"{name} exception while uploading result to Azure {Environment.NewLine} Message : {msg}";
                        dataObject = (T)Convert.ChangeType(test, typeof(T));
                    }
                    try
                    {
                        response = $"{RunnerConfiguration.LoggingAPIUrl}/{uri}".PutJsonAsync(dataObject)
                       .ConfigureAwait(false)
                       .GetAwaiter()
                       .GetResult()
                       .ResponseMessage;
                    }
                    catch (Exception ex1)
                    {
                        name = ex1.GetType().Name;
                        msg = ex1.Message;
                        trace = ex1.StackTrace;
                    }
                }
                Console.WriteLine($"EXCEPTION TYPE - {name}");
                Console.WriteLine($"EXCEPTION MESSAGE - {msg}");
                Console.WriteLine($"STACK TRACE - {trace}");
            }
        }
    }
}
