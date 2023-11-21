namespace TestRunner.Utilities
{
    public static class RunnerConfiguration
    {
        public static string AllowedHosts { get; set; }
        public static string StorageType { get; set; }
        public static string FileStoragePath { get; set; }
        public static string TestInventoryFileStoragePath { get; set; }
        public static string ApplicationType { get; set; }
        public static int DashboardPort { get; set; }
        public static string AZStorageAccount { get; set; }
        public static string AZStorageKey { get; set; }
        public static string AZStorageScreenShotBlobContainer { get; set; }
        public static string AZStorageTable { get; set; }
        public static string AZDevOpsAPIKey { get; set; }
        public static bool LogTestData { get; set; }
        public static bool LogTestAction { get; set; }
        public static string AZStorageTestDataBlobContainer { get; set; }
        public static string AZStorageTestActionBlobContainer { get; set; }
        public static string LoggingAPIUrl { get; set; }
        public static string ChromedriverPath { get; set; }
        public static string GeckodriverPath { get; set; }
        public static string EdgedriverPath { get; set; }
        public static string Assembly { get; set; }
        public static string SuiteType { get; set; }
        public static int Attempts { get; set; }
        public static string LogApiURL { get; set; }
        public static string LocalResultsPath { get; set; }
        public static int MaximumConcurrency { get; set; }
        public static string Labels { get; set; }
        public static string ExcludeAttribute { get; set; }
        public static bool UpdateDriver { get; set; }
    }
}
