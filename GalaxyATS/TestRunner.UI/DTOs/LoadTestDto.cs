using System.Collections.Generic;

namespace TestRunner.UI.ViewModels
{
    public class LoadTestDto
    {
        public List<Value> Value { get; set; }
    }

    public class Value
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string RunID { get; set; }
        public string EnvironmentName { get; set; }
        public string Area { get; set; }
        public string UsersCount { get; set; }
        public string RampUpTime { get; set; }
        public string AvgTime { get; set; }
        public string MinTime { get; set; }
        public string MaxTime { get; set; }
        public string Completed { get; set; }
        public string EnrolledPortfolios { get; set; }
    }
}
