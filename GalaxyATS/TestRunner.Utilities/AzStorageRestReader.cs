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
using TestRunner.Utilities.DTOs;

namespace TestRunner.Utilities
{
    public class AzStorageRestReader
    {
        public List<string> GetFailedTestList(string label)
        {
            var testList = new List<TestDto>();
            var runId = GetLatestLabelRunId(label);
            var query = $"()?$filter=PartitionKey%20eq%20'{runId}'%20and%20(Status%20ne%20'Passed'%20or%20Status%20eq%20'null')";

            var testListJson = "";
            HttpResponseMessage response = Call(HttpMethod.Get, query, "", "seResults", ref testListJson);
            testListJson = JsonSerializer.Deserialize<List<string>>(testListJson).FirstOrDefault();
            testList = JsonSerializer.Deserialize<Dictionary<string, List<TestDto>>>(testListJson)["value"];
            if (testList.Count == 0)
            {
                throw new Exception($"********No failed tests found for the runID {runId}");
            }
            else
            {
                Console.WriteLine("*********Fail Test Count is - " + testList.Count);
                var testNames = testList.ConvertAll(x => x.Name);

                /**************************Add queued tests***************************/
                var testsJson = GetBlob(runId, "testlist", RunnerConfiguration.AZStorageScreenShotBlobContainer);
                var allTestsNames = JsonSerializer.Deserialize<List<string>>(testsJson).ToList();
                Console.WriteLine("*****All Test Count Is -" + allTestsNames.Count);
                var executedTestsJson = "";
                Call(HttpMethod.Get, $"()?$filter=PartitionKey%20eq%20'{runId}'", "", "seResults", ref executedTestsJson);
                executedTestsJson = JsonSerializer.Deserialize<List<string>>(executedTestsJson).FirstOrDefault();
                var executedTestNames = JsonSerializer.Deserialize<Dictionary<string, List<TestDto>>>(executedTestsJson)["value"].ConvertAll(x => x.Name);
                Console.WriteLine("*****Executed Test Count  Is -" + executedTestNames.Count);
                var queuedTests = allTestsNames.Except(executedTestNames).OrderBy(x => x).ToList();
                Console.WriteLine("*****Queued Test Count Is -" + queuedTests.Count);
                testNames.AddRange(queuedTests);
                /**********************************************************************/

                foreach (var test in testNames)
                {
                    Console.WriteLine("*****Failed Test Names Are -" + test);
                }

                return testNames;
            }
        }

        public string GetLatestLabelRunId(string label)
        {
            string queryString;

            var yesterday = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd'T'HH:mm:sssZ");

            queryString = $"()?$filter=RowKey%20eq%20'run'%20and%20Labels%20eq%20'{label}'%20and%20Timestamp%20gt%20datetime'{yesterday}'";
            Console.WriteLine("************The query to run is:************" + queryString);

            var runListJson = "";
            HttpResponseMessage response = Call(HttpMethod.Get, queryString, "", "seRuns", ref runListJson);
            var allRunListJsons = JsonSerializer.Deserialize<List<string>>(runListJson).ToList();

            //runListJson fetches a collection of list, one of which has the run list, other is blank. The condition below is to pick up the run list
            runListJson = allRunListJsons.FirstOrDefault().Length > allRunListJsons.LastOrDefault().Length ? allRunListJsons.FirstOrDefault() : allRunListJsons.LastOrDefault();
            var runList = JsonSerializer.Deserialize<Dictionary<string, List<RunDto>>>(runListJson)["value"];
            if (runList.Count == 0)
            {
                throw new Exception($"********No run found for the query {queryString}");
            }
            else
            {
                Console.WriteLine("******Run Id is - " + runList.LastOrDefault().RunId);
                return runList.LastOrDefault().RunId;
            }

        }

        private HttpResponseMessage Call<T>(HttpMethod httpVerb, string uri, T dataObject, string azStorageTable, ref string _refVar)
        {
            var RequestDateString = DateTime.UtcNow.ToString("R", CultureInfo.InvariantCulture);
            var printUri = $"https://{RunnerConfiguration.AZStorageAccount}.table.core.windows.net/{azStorageTable}";
            Console.WriteLine("************The Azure URI to hit:" + printUri);
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
    }
}
