using System;
using System.Collections.Generic;

namespace TestRunner.UI.DTOs
{
    public class AZReleaseEnvDeployStepsDto
    {
        public int Id { get; set; }
        public int deploymentId { get; set; }
        public string Status { get; set; }
        public List<AZReleaseEnvDeployStepPhases> releaseDeployPhases { get; set; }
}
}
