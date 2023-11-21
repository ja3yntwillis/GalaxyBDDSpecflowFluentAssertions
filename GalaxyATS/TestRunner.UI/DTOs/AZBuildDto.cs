using System;

namespace TestRunner.UI.DTOs
{
    public class AZBuildDto
    {
        public int Id { get; set; }
        public string buildNumber { get; set; }
        public DateTime startTime { get; set; }
        public DateTime queueTime { get; set; }
        public DateTime finishTime { get; set; }
    }
}
