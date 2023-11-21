using System;

namespace TestRunner.Utilities.DTOs
{
    public class TestDto
    {
        public string TestId { get; set; }
        public string Assembly { get; set; }
        public string Fixture { get; set; }
        public string Method { get; set; }
        public int Attempts { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }
        public string ErrorJson { get; set; }
        public string Message { get; set; }
        public string Trace { get; set; }
        public string Url { get; set; }
        public string PageTitle { get; set; }
        public string ScreenshotBase64 { get; set; }
        public string ErrorType { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string Name => string.IsNullOrEmpty(Assembly) ? Fixture + "." + Method : Assembly + "." + Fixture + "." + Method;
        public string TestDescription { get; set; }
        public string Labels { get; set; }
    }
}