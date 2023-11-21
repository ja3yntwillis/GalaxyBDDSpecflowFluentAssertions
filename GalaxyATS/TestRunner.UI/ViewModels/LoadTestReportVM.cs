using System.Collections.Generic;

namespace TestRunner.UI.ViewModels
{
    public class LoadTestReportVM
    {
        public List<string> ReleaseNumbers { get; set; }
        public List<string> Areas { get; set; }
        public List<Content> Contents { get; set; }
    }

    public class Content
    {
        public string Release { get; set; }
        public string Area { get; set; }
        public string Time { get; set; }
    }
}
