using System.Collections.Generic;

namespace Framework.Galaxy.Dtos
{
    /// <summary>
    /// Dto for the violations information found by Axe Core
    /// </summary>
    public class AxeCoreViolationsDto
    {
        public List<Violations> Violations { get; set; }
    }

    /// <summary>
    /// Class for storing details of each violation
    /// </summary>
    public class Violations
    {
        public string Description { get; set; }
        public string Help { get; set; }
        public string HelpUrl { get; set; }
        public string Id { get; set; }
        public string Impact { get; set; }
        public List<Nodes> Nodes { get; set; }
        public List<string> Tags { get; set; }
    }

    /// <summary>
    /// Class for storing details of 'node'
    /// </summary>
    public class Nodes
    {
        public List<object> All { get; set; }
        public List<Any> Any { get; set; }
        public string FailureSummary { get; set; }
        public string Html { get; set; }
        public string Impact { get; set; }
        public List<object> None { get; set; }
        public List<string> Target { get; set; }
    }

    /// <summary>
    /// Class for storing details of other nodes that are related to the current node
    /// </summary>
    public class RelatedNodes
    {
        public string Html { get; set; }
        public List<string> Target { get; set; }
    }

    /// <summary>
    /// Class for storing details of 'any' node
    /// </summary>
    public class Any
    {
        public object Data { get; set; }
        public string Id { get; set; }
        public string Impact { get; set; }
        public string Message { get; set; }
        public List<RelatedNodes> RelatedNodes { get; set; }
    }
}
