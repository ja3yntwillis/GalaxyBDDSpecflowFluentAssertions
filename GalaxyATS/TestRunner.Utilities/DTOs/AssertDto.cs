using System.Diagnostics;

namespace TestRunner.Utilities.DTOs
{
    public class AssertDTO
    {
        public string Message { get; set; }
        public bool IsPass { get; set; }
        public StackFrame StackTrace { get; set; }
    }
}