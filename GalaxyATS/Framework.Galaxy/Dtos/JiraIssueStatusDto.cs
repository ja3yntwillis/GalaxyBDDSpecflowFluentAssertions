namespace Framework.Galaxy.Dtos
{
    /// <summary>
    /// DTO for the information received about status of a JIRA issue by JIRA API
    /// </summary>
    public class JiraIssueStatusDto
    {
        public string expand { get; set; }
        public string id { get; set; }
        public string self { get; set; }
        public string key { get; set; }
        public Fields fields { get; set; }
    }

    /// <summary>
    /// Class that contains the details of the fields
    /// </summary>
    public class Fields
    {
        public Status status { get; set; }
    }

    /// <summary>
    /// Class that contains the details of the status
    /// </summary>
    public class Status
    {
        public string self { get; set; }
        public string description { get; set; }
        public string iconUrl { get; set; }
        public string name { get; set; }
        public string id { get; set; }
        public StatusCategory statusCategory { get; set; }
    }

    /// <summary>
    /// Class that contains the details of the status category
    /// </summary>
    public class StatusCategory
    {
        public string self { get; set; }
        public int id { get; set; }
        public string key { get; set; }
        public string colorName { get; set; }
        public string name { get; set; }
    }
}
