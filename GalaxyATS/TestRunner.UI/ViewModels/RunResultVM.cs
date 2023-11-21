using System;
using System.Collections.Generic;

namespace TestRunner.UI.ViewModels
{
    public class RunResultVM
    {
        public string RunId { get; set; }
        public string RunDate { get; set; }
        public string BaseUrl { get; set; }
        public string Browser { get; set; }
        public string Application { get; set; }
        public string SuiteType { get; set; }
        public string RunEnvironment { get; set; }
        public string DashboardUrl { get; set; }
        public int Threads { get; set; }
        public int TestCount { get; set; }
        public int QueuedCount { get; set; }
        public int PassedCount { get; set; }
        public int BlockedCount { get; set; }
        public int FailedCount { get; set; }
        public int StartedCount { get; set; }
        public bool IsReleaseId { get; set; }
        public string RunReportUrl { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public List<DTOs.FailedTestDto> FailedTests { get; set; }
        public Dictionary<string, List<DTOs.FailedTestDto>> FailedTestsByMessage { get; set; }
        public List<TestRunner.Utilities.DTOs.TestDto> PassedTests { get; set; }
        public List<TestRunner.Utilities.DTOs.TestDto> BlockedTests { get; set; }
        public List<TestRunner.Utilities.DTOs.TestDto> StartedTests { get; set; }
        public List<string> QueuedTests { get; set; }
        public int NonPassedTestCount { get; set; }
        public List<string> NonPassedTests { get; set; }
        public string Labels { get; set; }
    }
}
