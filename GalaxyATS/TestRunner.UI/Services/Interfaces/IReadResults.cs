using System;
using System.Collections.Generic;
using TestRunner.UI.ViewModels;

namespace TestRunner.UI.Services.Interfaces
{
    public interface IReadResults
    {
        RunResultVM GetRunSummary(string runId);
        void SetRunSummary(string runId, RunResultVM runSummary);
        Utilities.DTOs.RunDto GetRun(string runId);
        List<Utilities.DTOs.RunDto> GetRuns(string baseUrl, string startDate, string endDate);
        List<Utilities.DTOs.RunDto> GetLabelRuns(string label, int hours = 0);
        List<Utilities.DTOs.TestDto> GetLabelTests(string label, int hours = 0);
        List<Utilities.DTOs.RunDto> GetLabelRuns(string label, string startDate, string endDate, bool rerun = false);
        bool IsFailedRun(string runId);
        bool CheckAllTestsForStatus(string runId, string status);
        List<Utilities.DTOs.TestDto> GetTests(string runId);
        Utilities.DTOs.TestDto GetTest(string runId, string testId);
        Utilities.DTOs.ErrorDto GetTestError(string runId, string testId);
        List<Utilities.DTOs.DataTreeDto> GetTestData(string runId, string testId);
        string GetLastModified(string runId);
        List<Utilities.DTOs.EnvironmentDto> GetBaseUrls();
        List<Utilities.DTOs.RunDto> GetDailyRuns(List<string> labels, string date = "");
        List<Utilities.DTOs.TestDto> GetLastNTests(string testName, int count = 10);
    }
}
