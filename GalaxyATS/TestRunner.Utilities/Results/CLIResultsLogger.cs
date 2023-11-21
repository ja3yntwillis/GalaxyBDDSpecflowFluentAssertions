using System;
using System.Collections.Generic;
using TestRunner.Utilities.DTOs;

namespace TestRunner.Utilities
{
    public class CLIResultsLogger : ILogResults
    {
        public void buildDataJson(object data, int parentId)
        {
            throw new NotImplementedException();
        }

        public DataTreeDto buildDataTree(object data)
        {
            DataTreeDto dataDto = new DataTreeDto();
            return dataDto;
        }

        public void EndRun(RunDto runDto)
        {
        }

        public TestDto EndTest(string runId, string testId, string status, int attempts, string testData, ErrorDto errorDto, ActionDto actionDto, DateTime endTime)
        {
            if (status.ToLower() == "passed")
            {
                Console.WriteLine(".");
            }
            else if (status.ToLower() == "failed")
            {
                Console.WriteLine("_");
                Console.WriteLine(errorDto.ErrorType);
                Console.WriteLine(errorDto.Message);
                Console.WriteLine(errorDto.Trace);
            }
            return new TestDto();
        }

        public string GetTestDescription(string methodName)
        {
            throw new NotImplementedException();
        }

        public List<string> FailedTests()
        {
            throw new NotImplementedException();
        }

        public string GetActionDocumentation(string actionMemberName)
        {
            throw new NotImplementedException();
        }

        public List<string> GetAllTestsFromRun(int runId, List<string> testStatusList)
        {
            throw new NotImplementedException();
        }

        public void LogAction(string message, string memberName, string filePath)
        {
            throw new NotImplementedException();
        }

        public void LogError(string errorType, string message, string pageTitle, string screenshotBase64, string trace, string url)
        {
            throw new NotImplementedException();
        }

        public void LogInfo(string infoMessage)
        {
            throw new NotImplementedException();
        }

        public void LogScreenshot(string screenshotBase64, string pageTitle, string url, string groupId)
        {
            throw new NotImplementedException();
        }

        public void RetryTest()
        {
            throw new NotImplementedException();
        }

        public void SetRunId(int runId)
        {
            throw new NotImplementedException();
        }

        public void SetTestId(int testId)
        {
            throw new NotImplementedException();
        }

        public RunDto StartRun(string url, string application, string attribute, string suiteType, string browser, int threads, string labels, List<string> tests, DateTime startTime, string dbName, string dbUsername)
        {
            return null;
        }

        public string StartTest(string runId, string testId, string assembly, string fixture, string method, DateTime startTime, string labels)
        {
            return "";
        }
    }
}
