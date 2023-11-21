using System.Collections.Generic;

namespace TestRunner.DTOs
{
    public class TestDocPageObjectDto
    {
        public string ClassName { get; set; }
        public string Description { get; set; }
        public List<string> Tests { get; set; }
        public List<TestDocTestDto> TestDtos { get; set; }
        public List<string> Actions { get; set; }
        public List<TestDocActionDto> ActionDtos { get; set; }

    }
}
