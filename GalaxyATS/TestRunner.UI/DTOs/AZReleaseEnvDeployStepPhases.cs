using System;

namespace TestRunner.UI.DTOs
{
    public class AZReleaseEnvDeployStepPhases
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime startedOn { get; set; }
    }
}
