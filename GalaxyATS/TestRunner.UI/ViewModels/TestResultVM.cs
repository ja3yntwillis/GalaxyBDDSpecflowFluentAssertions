using System;
using System.Collections.Generic;

namespace TestRunner.UI.ViewModels
{
    public class TestResultVM
    {
        public string TestId { get; set; }
        public string RunId { get; set; }
        public string Assembly { get; set; }
        public string Fixture { get; set; }
        public string Method { get; set; }
        public string TestDescription { get; set; }
        public int Attempts { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public List<string> Trace { get; set; }
        public string Url { get; set; }
        public string PageTitle { get; set; }
        public string ScreenshotBase64 { get; set; }
        public string ErrorType { get; set; }
        public List<TestRunner.Utilities.DTOs.DataTreeDto> DataTree { get; set; }
        public string DataTreeHtml { get; set; }
        public string Name => Assembly + "." + Fixture + "." + Method;
        public List<TestRunner.Utilities.DTOs.TestDto> ResultList { get; set; }

    }
}
