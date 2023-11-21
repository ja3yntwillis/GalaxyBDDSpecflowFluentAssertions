using System.Collections.Generic;

namespace TestRunner.UI.ViewModels
{
    public class ConsumerBuilderVM
    {
        public List<string> Urls { get; set; }
        public List<string> Scenarios { get; set; }
        public Dictionary<string, string> Applications { get; set; }
    }
}
