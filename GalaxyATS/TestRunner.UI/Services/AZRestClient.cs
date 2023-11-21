using Flurl.Http;
using System.Text.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using TestRunner.UI.DTOs;
using TestRunner.UI.Services.Interfaces;
using TestRunner.Utilities;

namespace TestRunner.UI.Services
{
    public class AZRestClient : IAZClient
    {
        public AZReleaseDto GetRelease(string releaseID)
        {
            AZReleaseDto result = null;
            var response = "";

            var organization = "liazon";
            var project = "BrightChoices";

            Call<AZReleaseDto>(HttpMethod.Get, "https://vsrm.dev.azure.com/", organization + "/" + project + "/_apis/release/releases/" + releaseID + "?api-version=5.0", null, ref response);
            result = JsonSerializer.Deserialize<AZReleaseDto>(response);

            return result;
        }

        public AZBuildDto GetBuild(string buildID)
        {
            AZBuildDto result = null;
            var response = "";

            var organization = "liazon";
            var project = "BrightChoices";

            Call<AZBuildDto>(HttpMethod.Get, "https://dev.azure.com/", organization + "/" + project + "/_apis/build/builds/" + buildID + "?api-version=5.0", null, ref response);
            result = JsonSerializer.Deserialize<AZBuildDto>(response);

            return result;
        }

        public AZCommitDto GetCommit(string buildID)
        {
            AZCommitDto result = null;
            var response = "";

            var organization = "liazon";
            var project = "BrightChoices";
            var repository = "008ec244-c781-430b-8c24-db8510dd3c46";

            var responseMsg = Call<AZCommitDto>(HttpMethod.Get, "https://dev.azure.com/", organization + "/" + project + "/_apis/build/builds/" + buildID + "/sources", null, ref response);
            var redirectUrl = responseMsg.Headers.GetValues("X-Tfs-Location").FirstOrDefault();
            var commitId = Regex.Match(redirectUrl, "commits%2F(.*)&redirect").Groups[1].Value;

            Call<AZCommitDto>(HttpMethod.Get, "https://dev.azure.com/", organization + "/" + project + "/_apis/git/repositories/" + repository + "/commits/" + commitId, null, ref response);
            result = JsonSerializer.Deserialize<AZCommitDto>(response);

            result.pullRequestId = Regex.Match(result.comment, "Merged PR (\\d+):").Groups[1].Value;

            return result;
        }

        private HttpResponseMessage Call<T>(HttpMethod httpVerb, string baseUri, string uri, T dataObject, ref string _refVar)
        {
            try
            {
                var requestUri = new Uri(baseUri + uri);

                string _personalAccessToken = RunnerConfiguration.AZDevOpsAPIKey;
                string _credentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _personalAccessToken)));

                HttpResponseMessage response = null;
                if (httpVerb == HttpMethod.Get)
                {
                    response = requestUri.ToString()
                        .WithHeader("Accept", "application/json")
                        .WithHeader("Authorization", $"Basic {_credentials}")
                        .WithHeader("Content-Type", "application/json")
                        .GetAsync()
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult()
                        .ResponseMessage;

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        _refVar = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    }
                }

                return response;

            }
            catch (FlurlHttpException e)
            {
                throw e;
            }
        }
    }
}
