using TestRunner.Utilities;
using System.Collections.Generic;

namespace TestRunner.UI.ViewModels
{
    public class TestRunnerVM
    {
        public List<string> Applications { get; set; }
        public List<string> Assemblies { get; set; }
        public List<string> Presets { get; set; }
        public string ReRunApplication { get; set; }
        public string ReRunSuiteType { get; set; }
        public string ReRunTestList { get; set; }
        public List<string> TestSuites { get; set; }
        public List<string> Urls { get; set; }
        public List<string> DatabaseName { get; set; }
        public List<string> VirtualMachine { get; set; }
        public List<string> LocalServices { get; set; }
    }
}
