using System;

namespace TestRunner.Utilities.DTOs
{
    public class ActionDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Message { get; set; }
        public string File { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}