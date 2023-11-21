using System;
using System.Collections.Generic;
using TestRunner.Utilities.DTOs;

namespace TestRunner.UI.ViewModels
{
    public class ExecutionTimeSummaryVM
    {
        public string RunId { get; set; }
        public string RunDate { get; set; }
        public int ThreadCount { get; set; }
        public string Browser { get; set; }
        public int TestCount { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public List<TestExecTimeDto> TotalTests { get; set; }
    }
}