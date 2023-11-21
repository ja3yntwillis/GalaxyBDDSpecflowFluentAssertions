using System.Collections.Generic;

namespace TestRunner.DTOs
{
    public class TestDocTestDto
    {
        public string ClassName { get; set; }
        public string Method { get; set; }
        public string Description { get; set; }
        public List<string> Actions { get; set; }
        public List<TestDocActionDto> ActionDtos { get; set; }
        public List<string> PageObjects { get; set; }
        public List<TestDocPageObjectDto> PageObjectDtos { get; set; }

    }
}