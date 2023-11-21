using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using TestRunner.Utilities.DTOs;
using System.Text.Json;
using System.Diagnostics;
using System.Collections;
using System.Net.Http;
using System.Net;
using Flurl.Http;
using System.Security.Cryptography;
using System.Globalization;

namespace TestRunner.Utilities
{
    public class MongoResultsLogger : ILogResults
    {

        [ThreadStatic]
        private string placeHolder = "";
        private static Random _random;
        private static Random _Random
        {
            get
            {
                if (_random == null)
                {
                    int currentProcessId = Process.GetCurrentProcess().Id;
                    int seed = Environment.TickCount + currentProcessId;
                    _random = new Random(seed);
                }
                return _random;
            }
            set
            { }
        }

        public void buildDataJson(object data, int parentId)
        {
            throw new NotImplementedException();
        }


        public void EndRun(RunDto runDto)
        {
            Call(HttpMethod.Put, $"(PartitionKey='{runDto.RunId}',RowKey='run')", runDto, "seRuns", ref placeHolder);
        }

        public TestDto EndTest(string runId, string testId, string status, int attempts, string testData, ErrorDto errorDto, ActionDto actionDto, DateTime endTime)
        {
            string testJson = "";
            Call(HttpMethod.Get, $"(PartitionKey='{runId}',RowKey='{testId}')", placeHolder, RunnerConfiguration.AZStorageTable, ref testJson);
            var testDto = JsonSerializer.Deserialize<TestDto>(testJson);

            testDto.Attempts = attempts;
            testDto.EndTime = endTime;
            testDto.Status = status;

            if (errorDto != null)
            {
                testDto.ErrorType = errorDto.ErrorType;
                testDto.Message = errorDto.Message;
                testDto.PageTitle = errorDto.PageTitle;
                testDto.Trace = errorDto.Trace;
                testDto.Url = errorDto.Url;
                testDto.ScreenshotBase64 = "";
                if (errorDto.ScreenshotBase64 != null && errorDto.ScreenshotBase64 != "")
                {
                    PutBlob(runId, testId, errorDto.ScreenshotBase64, RunnerConfiguration.AZStorageScreenShotBlobContainer);
                }
            }

            Call(HttpMethod.Put, $"(PartitionKey='{runId}',RowKey='{testId}')", testDto, RunnerConfiguration.AZStorageTable, ref placeHolder);

            return testDto;
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
            var runId = DateTime.UtcNow.ToString("yyyyMMddHHmmssfffffff");
            var runDto = new RunDto()
            {
                RunId = runId,
                Application = application,
                Attribute = attribute,
                SuiteType = suiteType,
                BaseUrl = url,
                Browser = browser,
                UserName = Environment.UserName,
                StartTime = startTime,
                MaxThreads = threads,
                Labels = labels,
                TestsCount = tests.Count,
                TestsJson = "",
                PartitionKey = runId,
                RowKey = "run",
                DatabaseName = dbName,
                DatabaseUsername = dbUsername,
            };
            Call(HttpMethod.Post, "", runDto, "seRuns", ref placeHolder);
            Call(HttpMethod.Post, "", runDto, RunnerConfiguration.AZStorageTable, ref placeHolder);
            PutBlob(runId, "testList", JsonSerializer.Serialize(tests), RunnerConfiguration.AZStorageScreenShotBlobContainer);
            return runDto;
        }

        public string StartTest(string runId, string testId, string assembly, string fixture, string method, DateTime startTime, string labels)
        {
            if (testId == null)
            {
                testId = GetTestId();
            }
            var testDto = new TestDto()
            {
                TestId = testId,
                Assembly = assembly,
                Fixture = fixture,
                Method = method,
                StartTime = startTime,
                Status = "Started",
                PartitionKey = runId,
                RowKey = testId,
                Labels = labels
            };
            Call(HttpMethod.Post, "", testDto, RunnerConfiguration.AZStorageTable, ref placeHolder);
            return testId;
        }

        public static string GetTestId()
        {
            var chars = "0123456789ABCDEFGHIJKLMOPQRSTUVWXYZ";
            var stringChars = new char[8];
            for (int i = 0; i < 8; i++)
            {
                stringChars[i] = chars[_Random.Next(chars.Length)];
            }
            return new string(stringChars);
        }


        private void Call<T>(HttpMethod httpVerb, string uri, T dataObject, string azStorageTable, ref string _refVar)
        {
            try
            {
                var RequestDateString = DateTime.UtcNow.ToString("R", CultureInfo.InvariantCulture);
                var requestUri = new Uri($"https://{RunnerConfiguration.AZStorageAccount}.table.core.windows.net/{azStorageTable}" + uri);
                var canonicalizedStringToBuild = string.Format("{0}\n{1}", RequestDateString, $"/{RunnerConfiguration.AZStorageAccount}/{requestUri.AbsolutePath.TrimStart('/')}");

                string signature;
                using (var hmac = new HMACSHA256(Convert.FromBase64String(RunnerConfiguration.AZStorageKey)))
                {
                    byte[] dataToHmac = Encoding.UTF8.GetBytes(canonicalizedStringToBuild);
                    signature = Convert.ToBase64String(hmac.ComputeHash(dataToHmac));
                }

                string authorizationHeader = string.Format($"{RunnerConfiguration.AZStorageAccount}:" + signature);

                var jsonContent = new StringContent(JsonSerializer.Serialize(dataObject), Encoding.UTF8, "application/json");
                HttpResponseMessage response = null;
                if (httpVerb == HttpMethod.Post)
                {
                    response = requestUri.ToString()
                        .WithHeader("x-ms-date", RequestDateString)
                        .WithHeader("date", null)
                        .WithHeader("x-ms-version", "2017-07-29")
                        .WithHeader("Accept", "application/json;odata=nometadata")
                        .WithHeader("Authorization", $"SharedKeyLite {authorizationHeader}")
                        .WithHeader("Content-Type", "application/json")
                        .PostAsync(jsonContent)
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult()
                        .ResponseMessage;

                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        var respContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        _refVar = respContent;
                    }
                }
                else if (httpVerb == HttpMethod.Put)
                {
                    response = $"{requestUri}".ToString()
                        .WithHeader("x-ms-date", RequestDateString)
                        .WithHeader("x-ms-version", "2017-07-29")
                        .WithHeader("Accept", "application/json;odata=nometadata")
                        .WithHeader("Authorization", $"SharedKeyLite {authorizationHeader}")
                        .WithHeader("Content-Type", "application/json")
                        .PutAsync(jsonContent)
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult()
                        .ResponseMessage;
                }
                else if (httpVerb == HttpMethod.Get)
                {
                    _refVar = requestUri.ToString()
                        .WithHeader("x-ms-date", RequestDateString)
                        .WithHeader("x-ms-version", "2017-07-29")
                        .WithHeader("Accept", "application/json;odata=nometadata")
                        .WithHeader("Authorization", $"SharedKeyLite {authorizationHeader}")
                        .GetStringAsync()
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

        public void PutBlob(string runId, string identifier, string dataObject, string storageContainerName)
        {
            try
            {
                var RequestDateString = DateTime.UtcNow.ToString("R", CultureInfo.InvariantCulture);
                var requestUri = new Uri($"https://{RunnerConfiguration.AZStorageAccount}.blob.core.windows.net/{storageContainerName}/{runId}_{identifier.ToLower()}");
                var stringContent = new StringContent(dataObject);
                var contentLength = dataObject.Length;
                var canonicalizedStringToBuild = "PUT" +
                    "\n" +
                    "\n" +
                    "\n" + contentLength +
                    "\n" +
                    "\ntext/plain; charset=UTF-8" +
                    "\n" +
                    "\n" +
                    "\n" +
                    "\n" +
                    "\n" +
                    "\n" +
                    "\nx-ms-blob-type:BlockBlob" +
                    "\nx-ms-date:" + RequestDateString +
                    "\nx-ms-version:2017-07-29" +
                    "\n" + $"/{RunnerConfiguration.AZStorageAccount}/{requestUri.AbsolutePath.TrimStart('/')}"
                    ;

                string signature;
                using (var hmac = new HMACSHA256(Convert.FromBase64String(RunnerConfiguration.AZStorageKey)))
                {
                    byte[] dataToHmac = Encoding.UTF8.GetBytes(canonicalizedStringToBuild);
                    signature = Convert.ToBase64String(hmac.ComputeHash(dataToHmac));
                }

                string authorizationHeader = string.Format($"{RunnerConfiguration.AZStorageAccount}:" + signature);

                HttpResponseMessage response = null;
                response = $"{requestUri}".ToString()
                    .WithHeader("x-ms-date", RequestDateString)
                    .WithHeader("x-ms-version", "2017-07-29")
                    .WithHeader("x-ms-blob-type", "BlockBlob")
                    .WithHeader("Content-Length", contentLength)
                    .WithHeader("Authorization", $"SharedKey {authorizationHeader}")
                    .WithHeader("Content-Type", "text/plain; charset=UTF-8")
                    .PutAsync(stringContent)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult()
                    .ResponseMessage;
            }
            catch (FlurlHttpException e)
            {
                throw e;
            }
        }
    }
}
