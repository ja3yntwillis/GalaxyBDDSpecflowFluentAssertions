using System.Collections.Generic;

namespace TestRunner.DTOs
{
    public class TestDocDto
    {
        public string ClassName { get; set; }
        public string Method { get; set; }
        public List<string> Tests { get; set; }
        public List<string> Actions { get; set; }
        public List<string> PageObjects { get; set; }

    }
}
