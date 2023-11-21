using System.Collections.Generic;
using TestRunner.DTOs;

namespace TestRunner.UI.ViewModels
{
    public class TestInformationVM
    {
        public string DocType { get; set; }
        public string ClassName { get; set; }
        public string Method { get; set; }
        public string Description { get; set; }
        public List<TestDocTestDto> Tests { get; set; }
        public List<TestDocActionDto> Actions { get; set; }
        public List<TestDocPageObjectDto> PageObjects { get; set; }
    }
}
