using System.Collections.Generic;

namespace TestRunner.DTOs
{
    public class TestDocApiClientDto
    {
        public string ClassName { get; set; }
        public string Method { get; set; }
        public string Description { get; set; }
        public List<string> Actions { get; set; }
    }
}
