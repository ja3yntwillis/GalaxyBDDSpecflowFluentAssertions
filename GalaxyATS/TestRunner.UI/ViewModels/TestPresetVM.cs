using System.Collections.Generic;

namespace TestRunner.UI.ViewModels
{
    public class TestPresetVM
    {
        public List<string> CurrentPresets { get; set; }
        public List<string> TestList { get; set; }
        public Dictionary<string, string> RunSettings { get; set; }
    }
}
