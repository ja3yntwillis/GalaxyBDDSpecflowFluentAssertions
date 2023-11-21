using System.Text.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Flurl.Http;
using System.Text;
using TestRunner.Utilities.DTOs;

namespace TestRunner.Utilities
{
    public class BrowserStackClient
    {
        protected readonly System.Text.Json.JsonSerializerOptions _jsonOptions = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public List<BrowserStackAppsDto> GetAppList(string username, string key)
        {
            var response = Get("https://api-cloud.browserstack.com/app-automate/recent_group_apps", username, key);
            var result = JsonSerializer.Deserialize<List<BrowserStackAppsDto>>(response);
            return result;
        }

        public BrowserStackAppsDto UploadApp(string username, string key, Stream fileStream, string fileName)
        {
            var response = PostFileBasicAuth("https://api-cloud.browserstack.com/app-automate/upload", username, key, fileStream, fileName);
            var result = JsonSerializer.Deserialize<BrowserStackAppsDto>(response);
            return result;
        }

        /// <summary>
        /// Get with a specified url, bearer token and request parameters
        /// </summary>
        /// <param name="url">url to hit for the endpoint</param>
        /// <param name="parameters">Parameters for GET call, defaults to Null</param>
        /// <returns>Response from Get Call</returns>
        protected string Get(string url, string username, string password, Dictionary<string, string> parameters = null)
        {
            string result = "";
            var headers = new Dictionary<string, string>();

            try
            {
                var response = $"{url}".WithBasicAuth(username, password)
                                            .WithHeaders(headers)
                                            .SetQueryParams(parameters)
                                            .GetAsync()
                                            .ConfigureAwait(false)
                                            .GetAwaiter()
                                            .GetResult();

                result = response.ResponseMessage.Content.ReadAsStringAsync()
                                        .ConfigureAwait(false)
                                        .GetAwaiter()
                                        .GetResult();

            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"REST API call failed. Attempted to GET from {url}", ex);
            }

            return result;
        }

        protected string Post<T>(string serviceUrl, string uri, T dataObject, IDictionary<string, object> headers = null)
        {
            string result = "";
            try
            {
                var response = $"{serviceUrl}/{uri}".WithHeaders(headers)
                                                .PostAsync(new StringContent(System.Text.Json.JsonSerializer.Serialize(dataObject, _jsonOptions), Encoding.UTF8, "application/json"))
                                                .ConfigureAwait(false)
                                                .GetAwaiter()
                                                .GetResult();

                result = response.ResponseMessage.Content.ReadAsStringAsync()
                                        .ConfigureAwait(false)
                                        .GetAwaiter()
                                        .GetResult();
            }
            catch (FlurlHttpException e)
            {
                throw new HttpRequestException($"REST API call failed. Attempted to POST to {serviceUrl}/{uri}", e);
            }

            return result;
        }

                /// <summary>
        /// POST call to an endpoint with a file parameter
        /// </summary>
        /// <param name="serviceUrl">URL of the service to hit directly</param>
        /// <param name="username">Username for Basic Auth</param>
        /// <param name="password">Password for Basic Auth</param>
        /// <param name="filePath">File Path to attach the file from</param>
        /// <param name="fileParam">Parameter name to attach the file to for request. Default = file</param>
        /// <param name="headers">Optional additional headers for request</param>
        /// <returns>Response of the POST call</returns>
        protected string PostFileBasicAuth(string url, string username, string password, Stream fileStream, string fileName, string fileParam = "file", IDictionary<string, object> headers = null)
        {
            string result = "";

            try
            {
                var response = $"{url}".WithBasicAuth(username, password)
                                                .WithHeaders(headers)
                                                .WithTimeout(240)
                                                .PostMultipartAsync(mp => mp
                                                .AddFile(fileParam, fileStream, fileName))
                                                .ConfigureAwait(false)
                                                .GetAwaiter()
                                                .GetResult();

                result = response.ResponseMessage.Content.ReadAsStringAsync()
                                        .ConfigureAwait(false)
                                        .GetAwaiter()
                                        .GetResult();
            }
            catch (FlurlHttpException e)
            {
                throw new HttpRequestException($"REST API call failed. Attempted to POST to {url}", e);
            }

            return result;
        }
    }
}