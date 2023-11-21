using System.Collections.Generic;
using Newtonsoft.Json;

namespace Framework.Galaxy.Dtos
{
    /// <summary>
    /// DTO for the information received about commits from BitBucket API
    /// </summary>
    public class BitBucketCommitsResponseDto
    {
        public List<Commit> values { get; set; }
        public int size { get; set; }
        public bool isLastPage { get; set; }
        public int start { get; set; }
        public int limit { get; set; }
    }

    /// <summary>
    /// Class that contains the details of the author of the commit
    /// </summary>
    public class Author
    {
        public string name { get; set; }
        public string emailAddress { get; set; }
        public int id { get; set; }
        public string displayName { get; set; }
        public bool active { get; set; }
        public string slug { get; set; }
        public string type { get; set; }
        public Links links { get; set; }
    }

    /// <summary>
    /// Class that contains the details of the committer of the commit
    /// </summary>
    public class Committer
    {
        public string name { get; set; }
        public string emailAddress { get; set; }
        public int id { get; set; }
        public string displayName { get; set; }
        public bool active { get; set; }
        public string slug { get; set; }
        public string type { get; set; }
        public Links links { get; set; }
    }

    /// <summary>
    /// Class that wraps the 'self' details of the commit
    /// </summary>
    public class Links
    {
        public List<Self> self { get; set; }
    }

    /// <summary>
    /// Class that contains the details of the immediate previous commit of the current commit
    /// </summary>
    public class Parent
    {
        public string id { get; set; }
        public string displayId { get; set; }
    }

    /// <summary>
    /// Class that contains the details of the JIRA story ID(s) linked with the commit
    /// </summary>
    public class Properties
    {
        [JsonProperty("jira-key")]
        public List<string> jiraKey { get; set; }
    }

    /// <summary>
    /// Class that contains the URL of the author or committer of the commit
    /// </summary>
    public class Self
    {
        public string href { get; set; }
    }

    /// <summary>
    /// Class that contains the details of the commit
    /// </summary>
    public class Commit
    {
        public string id { get; set; }
        public string displayId { get; set; }
        public Author author { get; set; }
        public string authorTimestamp { get; set; }
        public Committer committer { get; set; }
        public string committerTimestamp { get; set; }
        public string message { get; set; }
        public List<Parent> parents { get; set; }
        public Properties properties { get; set; }
    }
}