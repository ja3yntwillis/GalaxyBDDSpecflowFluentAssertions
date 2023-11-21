using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TestRunner.UI.Services.Interfaces;
using TestRunner.UI.ViewModels;
using TestRunner.Utilities;
using TestRunner.Utilities.DTOs;
using Tests.Galaxy.Utility;

namespace TestRunner.UI.Services
{
    public class AZStorageRESTResultsReader : IReadResults
    {
        private RunDto Run { get; set; }
        private List<TestDto> Tests { get; set; }

        public string GetLastModified(string runId)
        {
            return "";
            string entityListJson = "";
            Call(HttpMethod.Get, $"()?$filter=PartitionKey%20eq%20'{runId}'&$select=PartitionKey,RowKey,Timestamp", "", "seRuns", ref entityListJson);
            var entityList = JsonSerializer.Deserialize<List<Dictionary<string, List<AZTableEntity>>>>(entityListJson).FirstOrDefault()["value"];
            var latestEntity = entityList.OrderByDescending(x => x.Timestamp).FirstOrDefault();
            return latestEntity.RowKey;
        }

        public RunDto GetRun(string runId)
        {
            string json = "";
            Call(HttpMethod.Get, $"(PartitionKey='{runId}',RowKey='run')", "", "seRuns", ref json);
            json = JsonSerializer.Deserialize<List<string>>(json).FirstOrDefault();
            Run = JsonSerializer.Deserialize<Utilities.DTOs.RunDto>(json);
            if (Run.TestsJson != null && Run.TestsJson != "")
            {
                Run.Tests = JsonSerializer.Deserialize<List<string>>(Run.TestsJson);
            }
            else
            {
                var testsJson = GetBlob(runId, "testlist", RunnerConfiguration.AZStorageScreenShotBlobContainer);
                Run.Tests = JsonSerializer.Deserialize<List<string>>(testsJson);
            }
            return Run;
        }

        public List<RunDto> GetRuns(string baseUrl = "", string startDate = "", string endDate = "")
        {
            string queryString;
            var earliestDate = DateTime.Now.AddDays(-15).ToUniversalTime().ToString("yyyyMMddHHmmssFFFF");
            var runList = new List<RunDto>();
            List<string> listOfAllRuns = new List<string>();

            if (startDate.StartsWith("0001010") || startDate == "")
            {
                startDate = earliestDate;
            }

            if (endDate.StartsWith("0001010") || endDate == "")
            {
                endDate = DateTime.Now.ToUniversalTime().ToString("yyyyMMddHHmmssfffffff");
            }

            if (baseUrl == "all")
            {
                queryString = $"()?$filter=PartitionKey%20gt%20'{earliestDate}'%20and%20RowKey%20eq%20'run'";
            }
            else
            {
                queryString = $"()?$filter=PartitionKey%20ge%20'{startDate}'%20and%20PartitionKey%20le%20'{endDate}'%20and%20RowKey%20eq%20'run'%20and%20BaseUrl%20eq%20'{baseUrl}'";
            }

            var runListJson = "";
            HttpResponseMessage response = Call(HttpMethod.Get, queryString, "", "seRuns", ref runListJson);
            listOfAllRuns = JsonSerializer.Deserialize<List<string>>(runListJson);
            foreach (var run in listOfAllRuns)
            {
                runList.AddRange(JsonSerializer.Deserialize<Dictionary<string, List<RunDto>>>(run)["value"]);
            }

            return runList;
        }

        public List<EnvironmentDto> GetBaseUrls()
        {
            string runListJson = null;
            string queryString = $"()?$select=RowKey,url";

            HttpResponseMessage response = Call(HttpMethod.Get, queryString, "", "environments", ref runListJson);
            runListJson = JsonSerializer.Deserialize<List<string>>(runListJson).FirstOrDefault();
            var jsonOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };
            var environmentList = JsonSerializer.Deserialize<Dictionary<string, List<EnvironmentDto>>>(runListJson, jsonOptions)["value"].Distinct().ToList();

            return environmentList;
        }

        public RunResultVM GetRunSummary(string runId)
        {
            string summaryJson = GetBlob(runId, "summary", RunnerConfiguration.AZStorageScreenShotBlobContainer);
            var summaryVM = JsonSerializer.Deserialize<RunResultVM>(summaryJson);
            return summaryVM;
        }

        public TestDto GetTest(string runId, string testId)
        {
            string testJson = "";
            Call(HttpMethod.Get, $"(PartitionKey='{runId}',RowKey='{testId}')", "", RunnerConfiguration.AZStorageTable, ref testJson);
            testJson = JsonSerializer.Deserialize<List<string>>(testJson).FirstOrDefault();
            var testDto = JsonSerializer.Deserialize<Utilities.DTOs.TestDto>(testJson);
            if (testDto.Status.ToLower() == "failed")
            {
                try
                {
                    testDto.ScreenshotBase64 = GetBlob(runId, testId, RunnerConfiguration.AZStorageScreenShotBlobContainer);
                }
                catch (FlurlHttpException)
                {
                }
            }
            return testDto;
        }

        public List<Utilities.DTOs.DataTreeDto> GetTestData(string runId, string testId)
        {
            string testJson = "";
            var dataTreeDto = new List<DataTreeDto>();

            try
            {
                testJson = GetBlob(runId, $"{testId}_testData", RunnerConfiguration.AZStorageTestDataBlobContainer);
                dataTreeDto = JsonSerializer.Deserialize<List<Utilities.DTOs.DataTreeDto>>(testJson);
            }
            catch (FlurlHttpException)
            {
            }
            return dataTreeDto;
        }

        public ErrorDto GetTestError(string runId, string testId)
        {
            string errorJson = "";
            string screenshotBase64 = "";
            try
            {
                Call(HttpMethod.Get, $"(PartitionKey='{runId}',RowKey='{testId}_error')", "", RunnerConfiguration.AZStorageTable, ref errorJson);
                screenshotBase64 = GetBlob(runId, testId, RunnerConfiguration.AZStorageScreenShotBlobContainer);
            }
            catch (FlurlHttpException)
            {
            }
            errorJson = JsonSerializer.Deserialize<List<string>>(errorJson).FirstOrDefault();
            var errorDto = JsonSerializer.Deserialize<Utilities.DTOs.ErrorDto>(errorJson);
            if (errorDto != null)
            {
                errorDto.ScreenshotBase64 = screenshotBase64;
            }
            return errorDto;
        }

        public List<TestDto> GetTests(string runId)
        {
            string json = "";
            Call(HttpMethod.Get, $"()?$filter=PartitionKey%20eq%20'{runId}'%20and%20RowKey%20ne%20'run'%20and%20RowKey%20ne%20'summary'", "", RunnerConfiguration.AZStorageTable, ref json);
            var jsonArray = JsonSerializer.Deserialize<List<string>>(json);
            Tests = new List<Utilities.DTOs.TestDto>();
            foreach (var jsonString in jsonArray)
            {
                Tests.AddRange(JsonSerializer.Deserialize<Dictionary<string, List<Utilities.DTOs.TestDto>>>(jsonString)["value"]);
            }
            Tests = Tests.Where(x => !x.RowKey.Contains("_error")).ToList();
            return Tests;
        }

        public void SetRunSummary(string runId, RunResultVM runSummary)
        {
            return;
            var dataObject = JsonSerializer.Serialize(runSummary);
            var identifier = "summary";
            try
            {
                var RequestDateString = DateTime.UtcNow.ToString("R", CultureInfo.InvariantCulture);
                var requestUri = new Uri($"https://{RunnerConfiguration.AZStorageAccount}.blob.core.windows.net/{RunnerConfiguration.AZStorageScreenShotBlobContainer}/{runId}_{identifier.ToLower()}");
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

        public LoadTestDto GetLoadTests()
        {
            string json = "";

            Call(HttpMethod.Get, "", "", "loadTests", ref json);

            var jsonArray = JsonSerializer.Deserialize<List<string>>(json).FirstOrDefault();

            var loadTestDto = JsonSerializer.Deserialize<LoadTestDto>(jsonArray);

            return loadTestDto;
        }

        public LoadTestDto GetLoadTestData(string testName)
        {
            string json = "";

            Call(HttpMethod.Get, $"?$filter=PartitionKey%20eq%20'{testName}'", "", "loadTestResults", ref json);

            var jsonArray = JsonSerializer.Deserialize<List<string>>(json).FirstOrDefault();

            var loadTestDto = JsonSerializer.Deserialize<LoadTestDto>(jsonArray);

            return loadTestDto;
        }

        private HttpResponseMessage Call<T>(HttpMethod httpVerb, string uri, T dataObject, string azStorageTable, ref string _refVar)
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
            try
            {
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
                    IEnumerable<string> rowToken = new List<string>();
                    IEnumerable<string> partitionToken = new List<string>();
                    string originalUri = requestUri.OriginalString;
                    var resultsSets = new List<string>();

                    do
                    {
                        var request = requestUri.ToString()
                            .WithHeader("x-ms-date", RequestDateString)
                            .WithHeader("x-ms-version", "2017-07-29")
                            .WithHeader("Accept", "application/json;odata=nometadata")
                            .WithHeader("Authorization", $"SharedKeyLite {authorizationHeader}");


                        response = request.GetAsync()
                            .ConfigureAwait(false)
                            .GetAwaiter()
                            .GetResult()
                            .ResponseMessage;
                        response.Headers.TryGetValues("x-ms-continuation-NextPartitionKey", out partitionToken);
                        response.Headers.TryGetValues("x-ms-continuation-NextRowKey", out rowToken);

                        resultsSets.Add(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                        if (rowToken != null && rowToken.Count() > 0 && partitionToken != null && partitionToken.Count() > 0)
                        {
                            requestUri = new Uri(originalUri + "&NextPartitionKey=" + partitionToken.FirstOrDefault() + "&NextRowKey=" + rowToken.FirstOrDefault());
                        }
                    } while (rowToken != null);

                    _refVar = JsonSerializer.Serialize(resultsSets);
                }

                return response;
            }
            catch (Exception ex)
            {
                string name = ex.GetType().Name;
                string msg = ex.Message;
                string trace = ex.StackTrace;
                Console.WriteLine("Inside Exception while making Azure call.");
                try
                {
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
                        IEnumerable<string> rowToken = new List<string>();
                        IEnumerable<string> partitionToken = new List<string>();
                        string originalUri = requestUri.OriginalString;
                        var resultsSets = new List<string>();

                        do
                        {
                            var request = requestUri.ToString()
                                .WithHeader("x-ms-date", RequestDateString)
                                .WithHeader("x-ms-version", "2017-07-29")
                                .WithHeader("Accept", "application/json;odata=nometadata")
                                .WithHeader("Authorization", $"SharedKeyLite {authorizationHeader}");


                            response = request.GetAsync()
                                .ConfigureAwait(false)
                                .GetAwaiter()
                                .GetResult()
                                .ResponseMessage;
                            response.Headers.TryGetValues("x-ms-continuation-NextPartitionKey", out partitionToken);
                            response.Headers.TryGetValues("x-ms-continuation-NextRowKey", out rowToken);

                            resultsSets.Add(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                            if (rowToken != null && rowToken.Count() > 0 && partitionToken != null && partitionToken.Count() > 0)
                            {
                                requestUri = new Uri(originalUri + "&NextPartitionKey=" + partitionToken.FirstOrDefault() + "&NextRowKey=" + rowToken.FirstOrDefault());
                            }
                        } while (rowToken != null);

                        _refVar = JsonSerializer.Serialize(resultsSets);
                    }
                }
                catch (Exception ex1)
                {
                    name = ex1.GetType().Name;
                    msg = ex1.Message;
                    trace = ex1.StackTrace;
                }
                Console.WriteLine($"EXCEPTION TYPE - {name}");
                Console.WriteLine($"EXCEPTION MESSAGE - {msg}");
                Console.WriteLine($"STACK TRACE - {trace}");
                return response;
            }
        }
        public string GetBlob(string runId, string identifier, string storageContainerName)
        {
            var blobData = "";
            try
            {
                var RequestDateString = DateTime.UtcNow.ToString("R", CultureInfo.InvariantCulture);
                var requestUri = new Uri($"https://{RunnerConfiguration.AZStorageAccount}.blob.core.windows.net/{storageContainerName}/{runId}_{identifier.ToLower()}");
                var canonicalizedStringToBuild = "GET" +
                    "\n" +
                    "\n" +
                    "\n" +
                    "\n" +
                    "\n" +
                    "\n" +
                    "\n" +
                    "\n" +
                    "\n" +
                    "\n" +
                    "\n" +
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
                    .WithHeader("Authorization", $"SharedKey {authorizationHeader}")
                    .GetAsync()
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult()
                    .ResponseMessage;
                blobData = response.Content.ReadAsStringAsync().Result;
            }
            catch (FlurlHttpException e)
            {
                throw e;
            }
            return blobData;
        }

        /// <summary>
        /// Method to get all Test runs based on Labels
        /// </summary>
        /// <param name="label">Run to get based on a Label</param>
        /// <param name="hours">Number of hours to filter on, defaults to no hourly filter</param>
        /// <returns>List of Runs</returns>
        public List<RunDto> GetLabelRuns(string label, int hours = 0)
        {
            string queryString;

            var runList = new List<RunDto>();
            List<string> listOfAllRuns = new List<string>();

            if (hours == 0)
            {
                queryString = $"()?$filter=RowKey%20eq%20'run'%20and%20Labels%20eq%20'{label}'";
            }
            else
            {
                var queryTime = DateTime.UtcNow.AddHours(-hours).ToString("yyyy-MM-dd'T'HH:mm:sssZ");
                queryString = $"()?$filter=RowKey%20eq%20'run'%20and%20Labels%20eq%20'{label}'%20and%20Timestamp%20ge%20datetime'{queryTime}'";
            }

            var runListJson = "";
            HttpResponseMessage response = Call(HttpMethod.Get, queryString, "", "seRuns", ref runListJson);
            listOfAllRuns = JsonSerializer.Deserialize<List<string>>(runListJson);
            foreach (var run in listOfAllRuns)
            {
                runList.AddRange(JsonSerializer.Deserialize<Dictionary<string, List<RunDto>>>(run)["value"]);
            }

            return runList;
        }

        /// <summary>
        /// Method to get all Test based on Labels for a duration
        /// </summary>
        /// <param name="label">Test to get based on a Label</param>
        /// <param name="hours">Number of hours to filter on, defaults to last 1 hour</param>
        /// <returns>List of Tests</returns>
        public List<TestDto> GetLabelTests(string label, int hours = 1)
        {
            string queryString;

            var queryTime = DateTime.Now.AddHours(-hours).ToUniversalTime().ToString("yyyyMMddHHmmssFFFF");
            var testList = new List<TestDto>();
            List<string> listOfAllTests = new List<string>();

            queryString = $"()?$filter=PartitionKey%20gt%20'{queryTime}'%20and%20RowKey%20ne%20'run'%20and%20Labels%20eq%20'{label}'";

            var testListJson = "";
            HttpResponseMessage response = Call(HttpMethod.Get, queryString, "", "seResults", ref testListJson);
            listOfAllTests = JsonSerializer.Deserialize<List<string>>(testListJson);

            foreach (var test in listOfAllTests)
            {
                testList.AddRange(JsonSerializer.Deserialize<Dictionary<string, List<TestDto>>>(test)["value"]);
            }

            return testList;
        }

        /// <summary>
        /// Method to fetch Last N number of Tests for a specific Test Name
        /// </summary>
        /// <param name="testName">Full test name</param>
        /// <param name="days">Number of test execution days, defaults to 10</param>
        /// <returns>List of Tests</returns>
        public List<TestDto> GetLastNTests(string testName, int days = 10)
        {
            string queryString;
            var testList = new List<TestDto>();
            var queryTime = DateTime.Now.AddDays(-days).ToUniversalTime().ToString("yyyyMMddHHmmssFFFF");

            queryString = $"()?$filter=PartitionKey%20ge%20'{queryTime}'%20and%20RowKey%20ne%20'run'%20and%20Name%20eq%20'{testName}'";

            var testListJson = "";
            Call(HttpMethod.Get, queryString, "", "seResults", ref testListJson);
            var listOfAllTests = JsonSerializer.Deserialize<List<string>>(testListJson);

            foreach (var test in listOfAllTests)
            {
                testList.AddRange(JsonSerializer.Deserialize<Dictionary<string, List<TestDto>>>(test)["value"]);
            }
            return testList;
        }

        /// <summary>
        /// Method to get all Test runs based on Labels
        /// </summary>
        /// <param name="label">Run to get based on a Label</param>
        /// <param name="startDate">Date from which to fetch run records</param>
        /// <param name="endDate">Date up to which to fetch run records</param>
        /// <param name="rerun">boolean indicator to include rerun label in query or not</param>
        /// <returns>List of runs for the specified run label and time period</returns>
        public List<RunDto> GetLabelRuns(string label, string startDate, string endDate, bool rerun = false)
        {
            string queryString;
            var runList = new List<RunDto>();
            List<string> listOfAllRuns = new List<string>();

            if (startDate == null || startDate.Equals(""))
            {
                if (rerun)
                {
                    queryString = $"()?$filter=RowKey%20eq%20'run'%20and%20(Labels%20eq%20'{label}'%20or%20Labels%20eq%20'{label}Rerun'";
                }
                else
                {
                    queryString = $"()?$filter=RowKey%20eq%20'run'%20and%20Labels%20eq%20'{label}'";
                }
            }
            else
            {
                var start = DateTime.Parse(startDate).ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:sssZ");
                var end = (endDate == null || endDate.Equals("") ? DateTime.UtcNow : DateTime.Parse(endDate).AddDays(1).AddSeconds(-1).ToUniversalTime()).ToString("yyyy-MM-dd'T'HH:mm:sssZ");
                if (rerun)
                {
                    queryString = $"()?$filter=RowKey%20eq%20'run'%20and%20(Labels%20eq%20'{label}'%20or%20Labels%20eq%20'{label}Rerun')%20and%20StartTime%20ge%20'{start}'%20and%20StartTime%20le%20'{end}'";
                }
                else
                {
                    queryString = $"()?$filter=RowKey%20eq%20'run'%20and%20Labels%20eq%20'{label}'%20and%20StartTime%20ge%20'{start}'%20and%20StartTime%20le%20'{end}'";
                }
            }

            var runListJson = "";
            Call(HttpMethod.Get, queryString, "", "seRuns", ref runListJson);
            listOfAllRuns = JsonSerializer.Deserialize<List<string>>(runListJson);
            foreach (var run in listOfAllRuns)
            {
                runList.AddRange(JsonSerializer.Deserialize<Dictionary<string, List<RunDto>>>(run)["value"]);
            }

            if (rerun && runList.Count == 0)
            {
                runList.Add(new RunDto());
            }
            return runList;
        }

        /// <summary>
        /// Checks if a run has even a single Failed test
        /// </summary>
        /// <param name="runId">Run Id</param>
        /// <returns></returns>
        public bool IsFailedRun(string runId)
        {
            Console.WriteLine("***Inside AZStorageRESTResultsReader - IsFailedRun Method***");
            var testList = new List<TestDto>();
            var query = $"()?$filter=PartitionKey%20eq%20'{runId}'%20and%20Status%20eq%20'Failed'%20and%20RowKey%20ne%20'run'";
            var testListJson = "";
            HttpResponseMessage response = Call(HttpMethod.Get, query, "", "seResults", ref testListJson);
            testListJson = JsonSerializer.Deserialize<List<string>>(testListJson).FirstOrDefault();
            testList = JsonSerializer.Deserialize<Dictionary<string, List<TestDto>>>(testListJson)["value"];

            return testList.Count > 0 ? true : false;
        }

        /// <summary>
        /// Checks if all tests in a run have a specific status
        /// </summary>
        /// <param name="runId">Run Id</param>
        /// <param name="status">Test status to check</param>
        /// <returns></returns>
        public bool CheckAllTestsForStatus(string runId, string status)
        {
            var testList = new List<TestDto>();
            var query = $"()?$filter=PartitionKey%20eq%20'{runId}'%20and%20RowKey%20ne%20'run'";
            var testListJson = "";
            HttpResponseMessage response = Call(HttpMethod.Get, query, "", "seResults", ref testListJson);
            testListJson = JsonSerializer.Deserialize<List<string>>(testListJson).FirstOrDefault();
            testList = JsonSerializer.Deserialize<Dictionary<string, List<TestDto>>>(testListJson)["value"];

            return testList.All(t => t.Status.Equals(status));
        }

        /// <summary>
        /// Fetch the list of latest runs for the given list of labels
        /// </summary>
        /// <param name="labels">Labels list</param>
        /// <param name="date">Date of runs</param>
        /// <returns>Latest list of runs</returns>
        public List<RunDto> GetDailyRuns(List<string> labels = null, string date = "")
        {
            var today = new DateUtility().GetTodaysEasternDate();
            var runList = new List<RunDto>();
            List<RunDto> allRuns;
            RunDto Rerun = new RunDto();


            if (date == "")
            {
                date = today.AddDays(-1).ToString();
            }

            foreach (string label in labels)
            {
                allRuns = GetLabelRuns(label, date, null, rerun: true);
                if (allRuns[0].TestsCount > 0)
                {
                    if (allRuns.Exists(x => x.Labels.Equals(label)))
                        Run = allRuns.Where(x => x.Labels.Equals(label)).ToList().LastOrDefault();

                    if (Run.TestsCount > 0 && Run.TestsCount != Run.PassedCount)
                    {
                        /**************************Rerun Count*******************************/
                        if (allRuns.Exists(x => x.Labels.Equals(label + "Rerun")))
                            Rerun = allRuns.Where(x => x.Labels.Equals(label + "Rerun")).ToList().LastOrDefault();
                        if (Rerun.TestsCount > 0 && Rerun.StartTime > Run.StartTime)
                        {
                            Run.RunId = Rerun.RunId;
                            Run.PassedCount += Rerun.PassedCount;
                            Run.FailedCount = Rerun.FailedCount;
                        }
                        /********************************************************************/
                        runList.Add(Run);
                    }
                }
            }
            return runList;
        }
    }
    public class AZTableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTime Timestamp { get; set; }
    }
    public class AZTableSummaryEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTime Timestamp { get; set; }
        public string RunSummary { get; set; }
    }
}
