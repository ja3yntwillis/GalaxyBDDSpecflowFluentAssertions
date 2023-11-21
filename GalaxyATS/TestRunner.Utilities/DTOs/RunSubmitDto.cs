using System;
using System.Collections.Generic;

namespace TestRunner.Utilities.DTOs
{
    public class RunSubmitDto
    {
        public string[] Tests { get; set; }
        public string BaseUrl { get; set; }
        public string Browser { get; set; }
        public string BrowserVersion { get; set; }
        public string OperatingSystem { get; set; }
        public string OsVersion { get; set; }
        public string DeviceOperatingSystem { get; set; }
        public string DeviceOsVersion { get; set; }
        public string DeviceDropdown { get; set; }
        public string DeviceOrientationDropdown { get; set; }
        public int Retries { get; set; }
        public string Labels { get; set; }
        public string Domain { get; set; }
        public string Application { get; set; }
        public string Assembly { get; set; }
        public string Attribute { get; set; }
        public List<string> Attributes { get; set; }
        public string AdminUsername { get; set; }
        public string AdminPassword { get; set; }
        public string EmployeeUrl { get; set; }
        public string SuiteType { get; set; }
        public int Timeout { get; set; }
        public int MaxThreads { get; set; }
        public bool LogTestData { get; set; }
        public bool UpdateDriver { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string DatabaseName { get; set; }
        public string DbUserName { get; set; }
        public string DBPassword { get; set; }
        public string BrowserStackUsername { get; set;}
        public string BrowserStackKey { get; set;}
        public string App { get; set; }
        public string Platform { get; set; }
        public string UdId { get; set; }
        public string DeviceType { get; set; }
        public string BrowserStackAppPath { get; set; }
        public bool RunOnVM { get; set; }
        public string VmUrl { get; set; }
        public string VmUserName { get; set; }
        public string VmPassword { get; set; }
        public string VmBranch { get; set; }
        public string mobileOSVersion { get; set; }
        public string mobileDevice { get; set; }
        public List<string> LocalServices { get; set; }
        public bool RunLocalUI { get; set; }
        public bool RunOnGrid { get; set; }
    }
}