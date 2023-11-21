using System;
using System.Collections.Generic;
using TestRunner.Utilities.DTOs;

namespace TestRunner.Utilities
{
    public interface ILogResults
    {
        RunDto StartRun(string url, string application, string atttribute, string suiteType, string browser, int threads, string labels, List<string> tests, DateTime startTime, string dbName, string dbUsername);

        void EndRun(RunDto runDto);

        string StartTest(string runId, string testId, string assembly, string fixture, string method, DateTime startTime, string Labels);

        void RetryTest();

        TestDto EndTest(string runId, string testId, string status, int attempts, string testDataJson, ErrorDto errorDto, ActionDto actionDt, DateTime endTime);

        /// <summary>
        /// List of failed Tests
        /// </summary>
        List<string> FailedTests();

        List<string> GetAllTestsFromRun(int runId, List<string> testStatusList);

        void LogInfo(string infoMessage);

        void LogError(string errorType, string message, string pageTitle, string screenshotBase64, string trace, string url);

        void LogScreenshot(string screenshotBase64, string pageTitle, string url, string groupId = null);

        void SetRunId(int runId);

        void SetTestId(int testId);

        void LogAction(string message = "", [System.Runtime.CompilerServices.CallerMemberName] string memberName = "", [System.Runtime.CompilerServices.CallerFilePath] string filePath = "");

        string GetActionDocumentation(string actionMemberName);
        string GetTestDescription(string testName);
    }
}
