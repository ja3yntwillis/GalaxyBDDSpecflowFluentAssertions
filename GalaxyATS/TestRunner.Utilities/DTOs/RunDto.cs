using System;
using System.Collections.Generic;

namespace TestRunner.Utilities.DTOs
{
    public class RunDto
    {
        public string RunId { get; set; }
        public string BaseUrl { get; set; }
        public string UserName { get; set; }
        public string Browser { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Labels { get; set; }
        public List<string> Tests { get; set; }
        public string TestsJson { get; set; }
        public int TestsCount { get; set; }
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }
        public int MaxThreads { get; set; }
        public string Application { get; set; }
        public string Attribute { get; set; }
        public string SuiteType { get; set; }
        public string ServerDashboardId { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string DatabaseName { get; set; }
        public string DatabaseUsername { get; set; }

    }
}