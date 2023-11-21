namespace TestRunner.Utilities.DTOs
{
    public class ErrorDto
    {
        public string Message { get; set; }
        public string Trace { get; set; }
        public string Url { get; set; }
        public string PageTitle { get; set; }
        public string ScreenshotBase64 { get; set; }
        public string ErrorType { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
    }
}