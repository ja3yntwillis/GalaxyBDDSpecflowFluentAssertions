using System;

namespace TestRunner.UI.DTOs
{
    public class FailedTestDto
    {
        public string TestId { get; set; }
        public string Fixture { get; set; }
        public string Method { get; set; }
        public string Assembly { get; set; }
        public int Attempts { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string Trace { get; set; }
        public string Url { get; set; }
        public string PageTitle { get; set; }
        public string ScreenshotBase64 { get; set; }
        public string ErrorType { get; set; }
        public string GeneralizedMessage { get; set; }
        public string Name => string.IsNullOrEmpty(Assembly) ? Fixture + "." + Method : Assembly + "." + Fixture + "." + Method;

    }
}
