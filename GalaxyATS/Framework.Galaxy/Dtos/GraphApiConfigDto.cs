using System.Collections.Generic;

namespace Framework.Galaxy.Dtos
{
    /// <summary>
    /// Dto for the Graph API Configuration information
    /// </summary>
    public class GraphApiConfigDto
    {
        public List<GraphApiConfig> GraphApiConfig { get; set; }
    }

    /// <summary>
    /// Class for storing details of Graph API
    /// </summary>
    public class GraphApiConfig
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Tenant { get; set; }
    }
}
