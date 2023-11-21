using System;
using System.Collections.Generic;

namespace TestRunner.UI.DTOs
{
    public class AZReleaseEnvironmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public List<AZReleaseEnvDeployStepsDto> deploySteps { get; set; }
    }
}
