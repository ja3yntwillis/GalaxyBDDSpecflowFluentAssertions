using System;
using System.Net;
using System.Collections.Generic;

namespace TestRunner.Utilities.DTOs
{
    public class ChromeDriverDto
    {
        public string timestamp { get; set; }
        public List<Version> versions { get; set; }
    }

    public class Version
    {
        public string version { get; set; }
        public string revision { get; set; }
        public Downloads downloads { get; set; }
    }

    public class Downloads
    {
        public List<Chrome> chrome { get; set; }
        public List<ChromeDriver> chromedriver { get; set; }
    }

    public class Chrome
    {
        public string platform { get; set; }
        public string url { get; set; }
    }

    public class ChromeDriver
    {
        public string platform { get; set; }
        public string url { get; set; }
    }
}