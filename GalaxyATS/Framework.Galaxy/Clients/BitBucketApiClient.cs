using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Framework.Galaxy.Dtos;

namespace Framework.Galaxy
{
    /// <summary>
    /// Class that manages operations related to BitBucket API
    /// </summary>
    public class BitBucketApiClient
    {
        private static string URL = "https://bitbucket.acclariscorp.com/rest/api/latest/users";
        private static string reservedCharacters = "!*'();:@&=+$,/?%#[]";

        /// <summary>
        /// Call BitBucket API to retrieve the list of commits for a specific time period
        /// </summary>
        /// <param name="repository">BitBucket repository specifier</param>
        /// <param name="username">BitBucket username</param>
        /// <param name="password">BitBucket password</param>
        /// <param name="startDate">Start Date</param>
        /// <param name="endDate">End Date, where End Date >= Start Date</param>
        /// <param name="branch">Git branch for the BitBucket repository, defaulted to develop</param>
        /// <returns>All commits for the specific time period</returns>
        public BitBucketCommitsResponseDto GetCommitsForSpecificTimePeriod(string repository, string username, string password, 
                                        DateTime startDate, DateTime endDate, string branch = "develop", string dateFormatter = "dd-MMM-yyyy hh:mm:ss")
        {
            DateTime referenceTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            int start = 0;
            int limit = 1000;
            BitBucketCommitsResponseDto bitBucketCommitsDto = new BitBucketCommitsResponseDto();
            do
            {
                string uri = $"{URL}/{repository}/commits?until={branch}&limit={limit}&start={start}";
                string response = CallBitBucketApi(username, password, uri).Result;
                BitBucketCommitsResponseDto commitsResponseDto = JsonConvert.DeserializeObject<BitBucketCommitsResponseDto>(response);
                var commitTime = referenceTime.AddMilliseconds(Convert.ToDouble(commitsResponseDto.values.Last().committerTimestamp)).ToLocalTime();
                if (endDate < commitTime)
                {
                    start += limit;
                    continue;
                }
                foreach (var commit in commitsResponseDto.values)
                {
                    commitTime = referenceTime.AddMilliseconds(Convert.ToDouble(commit.committerTimestamp)).ToLocalTime();
                    if (commitTime >= startDate && commitTime < endDate)
                    {
                        if (bitBucketCommitsDto.values == null)
                        {
                            bitBucketCommitsDto.values = new List<Commit>();
                        }
                        commit.committerTimestamp = commitTime.ToString(dateFormatter);
                        bitBucketCommitsDto.values.Add(commit);
                    }
                }
                try
                {
                    commitTime = referenceTime.AddMilliseconds(Convert.ToDouble(commitsResponseDto.values.Last().committerTimestamp)).ToLocalTime();
                }
                catch (Exception)
                {
                    commitTime = DateTime.Parse(commitsResponseDto.values.Last().committerTimestamp, CultureInfo.InvariantCulture);
                }
                if (startDate > commitTime || commitsResponseDto.isLastPage)
                {
                    break;
                }
                start += limit;
            } while (true);
            return bitBucketCommitsDto;
        }

        /// <summary>
        /// Call BitBucket API to retrieve the list of branches that are not updated since a given cutoff date
        /// </summary>
        /// <param name="repository">BitBucket repository specifier</param>
        /// <param name="username">BitBucket username</param>
        /// <param name="password">BitBucket password</param>
        /// <param name="cutoffDate">The given cutoff date</param>
        /// <returns>All branches that are stale since the given cutoff date</returns>
        public BitBucketBranchesResponseDto GetIdleBranchesForSpecificDate(string repository, string username, string password, DateTime cutoffDate)
        {
            DateTime referenceTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            int start = 0;
            int limit = 1000;
            BitBucketBranchesResponseDto bitBucketBranchesDto = new BitBucketBranchesResponseDto();
            do
            {
                string uri = $"{URL}/{repository}/branches?orderBy=MODIFICATION&limit={limit}&start={start}";
                string response = CallBitBucketApi(username, password, uri).Result;
                BitBucketBranchesResponseDto branchesResponseDto = JsonConvert.DeserializeObject<BitBucketBranchesResponseDto>(response);
                foreach (var branch in branchesResponseDto.values)
                {
                    BitBucketCommitsResponseDto commitsResponseDto = GetMostRecentCommitOfSpecificGitBranch(repository, username, password, branch.displayId);
                    var commitDate = referenceTime.AddMilliseconds(Convert.ToDouble(commitsResponseDto.values.FirstOrDefault().authorTimestamp)).ToLocalTime();
                    if (commitDate < cutoffDate)
                    {
                        if (bitBucketBranchesDto.values == null)
                        {
                            bitBucketBranchesDto.values = new List<Branch>();
                        }
                        branch.latestCommit = commitsResponseDto.values.FirstOrDefault().displayId;
                        branch.lastCommitDate = commitDate.ToString("dd-MMM-yyyy hh:mm:ss");
                        branch.author = commitsResponseDto.values.FirstOrDefault().author;
                        bitBucketBranchesDto.values.Add(branch);
                    }
                }
                if (branchesResponseDto.isLastPage)
                {
                    break;
                }
                start += limit;
            } while (true);
            return bitBucketBranchesDto;
        }

        /// <summary>
        /// Call BitBucket API to retrieve the most recent commit details
        /// </summary>
        /// <param name="repository">BitBucket repository specifier</param>
        /// <param name="username">BitBucket username</param>
        /// <param name="password">BitBucket password</param>
        /// <param name="branch">Git branch for the BitBucket repository, defaulted to develop</param>
        /// <returns>Most recent commit details</returns>
        private BitBucketCommitsResponseDto GetMostRecentCommitOfSpecificGitBranch(string repository, string username, string password, string branch = "develop")
        {
            string uri = $"{URL}/{repository}/commits?until={EncodedBranchName(branch)}&limit=1";
            string response = CallBitBucketApi(username, password, uri).Result;
            return JsonConvert.DeserializeObject<BitBucketCommitsResponseDto>(response);
        }

        /// <summary>
        /// Replace special characters from the branch name and encode it
        /// </summary>
        /// <param name="branch">Original branch name</param>
        /// <returns>Encoded branch name</returns>
        private string EncodedBranchName(string branch)
        {
            if (String.IsNullOrEmpty(branch))
            {
                return String.Empty;
            }

            var builder = new StringBuilder();
            foreach (char ch in branch)
            {
                if (reservedCharacters.IndexOf(ch) == -1)
                {
                    builder.Append(ch);
                }
                else
                {
                    builder.AppendFormat("%{0:X2}", (int)ch);
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Invoke the BitBucket API after encoding the username & password with Base64
        /// </summary>
        /// <param name="username">BitBucket username</param>
        /// <param name="password">BitBucket password</param>
        /// <param name="endPoint">BitBucket URI or Endpoint to invoke</param>
        /// <returns>HTTP Response</returns>
        private async Task<string> CallBitBucketApi(string username, string password, string endPoint)
        {
            HttpClient httpClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, endPoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}")));
            HttpResponseMessage response = await httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }
    }
}