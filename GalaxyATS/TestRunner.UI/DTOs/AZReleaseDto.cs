using System;
using System.Collections.Generic;

namespace TestRunner.UI.DTOs
{
    public class AZReleaseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<AZReleaseEnvironmentDto> Environments { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string Status { get; set; }
        public List<AZReleaseArtifactDto> Artifacts { get; set; }
    }
}
