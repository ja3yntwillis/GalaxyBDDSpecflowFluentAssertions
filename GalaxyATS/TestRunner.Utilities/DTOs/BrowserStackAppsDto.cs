using System.Text.Json.Serialization;

namespace TestRunner.Utilities.DTOs
{
    public class BrowserStackAppsDto
    {

        [JsonPropertyName("app_id")]
        public string Id { get; set; }
        [JsonPropertyName("app_name")]
        public string Name { get; set; }
        [JsonPropertyName("app_version")]
        public string Version { get; set; }
        [JsonPropertyName("app_url")]
        public string Url { get; set; }
        [JsonPropertyName("uploaded_at")]
        public string Uploaded { get; set; }
    }
}
