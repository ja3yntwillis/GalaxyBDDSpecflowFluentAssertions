namespace TestRunner.UI.DTOs
{
    public class AZReleaseArtifactDto
    {
        public string sourceId { get; set; }
        public string Type { get; set; }
        public string Alias { get; set; }
        public AZReleaseArtifactDefinionReferenceDto definitionReference { get; set; }
    }
}
