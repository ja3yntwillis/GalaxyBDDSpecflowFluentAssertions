using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Framework.Galaxy.Dtos;
using Newtonsoft.Json;

namespace Framework.Galaxy
{
    /// <summary>
    /// Class that manages operations related to JIRA API
    /// </summary>
    public class JiraApiClient
    {
        private static string URL = "https://jira.extendhealth.com/rest/api/latest/issue";
        private static string JQL_URL = "https://jira.extendhealth.com/rest/api/latest/search";
        private static int subListCapacity = 500;

        /// <summary>
        /// Call JIRA API to retrieve the status of a specific JIRA issue
        /// </summary>
        /// <param name="username">JIRA username</param>
        /// <param name="password">JIRA password</param>
        /// <param name="jiraID">JIRA Issue ID</param>
        /// <returns>Status of the JIRA Issue</returns>
        public string GetJiraIssueStatus(string username, string password, string jiraID)
        {
            string uri = $"{URL}/{jiraID}?fields=status";
            string response = CallJiraApi(username, password, uri).Result;
            JiraIssueStatusDto jiraStatusDto = JsonConvert.DeserializeObject<JiraIssueStatusDto>(response);
            return jiraStatusDto.fields.status.name;
        }

        /// <summary>
        /// Call JIRA API to retrieve the status from a list of JIRA issues
        /// </summary>
        /// <param name="username">JIRA username</param>
        /// <param name="password">JIRA password</param>
        /// <param name="jiraIDs">List of JIRA Issues ID</param>
        /// <returns>Dictionary of JIRA ID and Status of list of issues</returns>
        public Dictionary<string, string> GetBulkJiraIssueStatus(string username, string password, List<String> jiraIDs)
        {
            Dictionary<string, string> jiraListData = new Dictionary<string, string>();
            //Creating List of multiple sublist based on sublists capacity
            List<List<string>> partitions = Partition(jiraIDs, subListCapacity);
            for (int i=0; i< partitions.Count; i++)
            {
                string uri = $"{JQL_URL}?jql=key%20in%20({String.Join(",", partitions[i])})&fields=id,key,status&maxResults={partitions[i].Count}";
                string response = CallJiraApi(username, password, uri).Result;
                List<JiraIssueStatusDto> jiraIssuesData = JsonConvert.DeserializeObject<JiraIssuesDto>(response).issues;
                foreach (JiraIssueStatusDto singleJiraIssueData in jiraIssuesData)
                {
                    if (!jiraListData.ContainsKey(singleJiraIssueData.key))
                    {
                        jiraListData.Add(singleJiraIssueData.key, singleJiraIssueData.fields.status.name);
                    }                    
                }
            }            
            return jiraListData;
        }

        /// <summary>
        /// Invoke the JIRA API after encoding the username & password with Base64
        /// </summary>
        /// <param name="username">JIRA username</param>
        /// <param name="password">JIRA password</param>
        /// <param name="endPoint">JIRA URI or Endpoint to invoke</param>
        /// <returns>HTTP Response</returns>
        private async Task<string> CallJiraApi(string username, string password, string endPoint)
        {
            HttpClient httpClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, endPoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}")));
            HttpResponseMessage response = await httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Method to create list of sub lists using chunkSize
        /// </summary>
        /// <param name="values">Main list of values</param>
        /// <param name="chunkSize">Chunk size for sublist</param>
        /// <returns>List of Sub lists based on chunk size</returns>
        private List<List<T>> Partition<T>(List<T> values, int chunkSize)
        {
            return values.Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }
}
