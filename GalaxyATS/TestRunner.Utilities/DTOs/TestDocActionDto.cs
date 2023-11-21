using System.Collections.Generic;

namespace TestRunner.DTOs
{
    public class TestDocActionDto
    {
        public string ClassName { get; set; }
        public string Method { get; set; }
        public string Description { get; set; }
        public List<string> Tests { get; set; }
        public List<TestDocTestDto> TestDtos { get; set; }
        public List<string> PageObjects { get; set; }
        public List<TestDocPageObjectDto> PageObjectDtos { get; set; }

    }
}
