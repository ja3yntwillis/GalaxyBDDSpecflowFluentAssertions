using System.Collections.Generic;

namespace TestRunner.Utilities.DTOs
{
    public class DataTreeDto
    {
        public Dictionary<string, object> Content { get; set; }
        public string DataType { get; set; }
        public List<DataTreeDto> Children { get; set; }
    }
}