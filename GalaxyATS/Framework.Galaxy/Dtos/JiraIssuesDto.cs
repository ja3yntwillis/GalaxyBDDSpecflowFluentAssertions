using System.Collections.Generic;

namespace Framework.Galaxy.Dtos
{
    /// <summary>
    /// DTO for the information received for list of JIRA issues from JIRA API
    /// </summary>
    public class JiraIssuesDto
    {
        public string expand { get; set; }
        public int startAt { get; set; }
        public int maxResults { get; set; }
        public int total { get; set; }
        public List<JiraIssueStatusDto> issues { get; set; }
    }
}
