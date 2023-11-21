using System;

namespace TestRunner.Utilities.DTOs
{
    public class TestExecTimeDto
    {
        public string TestId { get; set; }
        public string Assembly { get; set; }
        public string Fixture { get; set; }
        public string Method { get; set; }
        public string Name => string.IsNullOrEmpty(Assembly) ? Fixture + "." + Method : Assembly + "." + Fixture + "." + Method;
        public string Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan ExecutionTime => new TimeSpan(EndTime.Subtract(StartTime).Ticks / TimeSpan.TicksPerSecond * TimeSpan.TicksPerSecond);
    }
}