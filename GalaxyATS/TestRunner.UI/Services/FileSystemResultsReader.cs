using System.Text.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TestRunner.UI.Services.Interfaces;
using TestRunner.UI.ViewModels;
using TestRunner.Utilities;
using TestRunner.Utilities.DTOs;

namespace TestRunner.UI.Services
{
    public class FileSystemResultsReader : IReadResults
    {
        public bool IsFailedRun(string runId)
        {
            throw new NotImplementedException();
        }

        public List<EnvironmentDto> GetBaseUrls()
        {
            throw new NotImplementedException();
        }

        public List<RunDto> GetLabelRuns(string label, int hours)
        {
            throw new NotImplementedException();
        }

        public List<Utilities.DTOs.RunDto> GetLabelRuns(string label, string startDate, string endDate, bool rerun = false)
        {
            throw new NotImplementedException();
        }

        public string GetLastModified(string runId)
        {
            var filePathFormat = string.Format("{0}{1}{2}", RunnerConfiguration.FileStoragePath, Path.DirectorySeparatorChar, runId);
            var lastModifiedFile = new DirectoryInfo(filePathFormat).GetFiles().OrderByDescending(x => x.LastWriteTime).First();
            var lastModifiedKey = lastModifiedFile.Name.Replace(".json", "");
            return lastModifiedKey;
        }

        public RunDto GetRun(string runId)
        {
            var filePathFormat = string.Format("{0}{1}{2}", RunnerConfiguration.FileStoragePath, Path.DirectorySeparatorChar, runId);
            var runFileContent = System.IO.File.ReadAllText(string.Format("{0}{1}{2}", filePathFormat, Path.DirectorySeparatorChar, "run.json"));
            var runDto = JsonSerializer.Deserialize<Utilities.DTOs.RunDto>(runFileContent);
            return runDto;
        }

        public List<RunDto> GetRuns(string baseUrl = "", string startDate = "", string endDate = "")
        {
            var runList = new List<TestRunner.Utilities.DTOs.RunDto>();

            var runDirs = Directory.GetDirectories(RunnerConfiguration.FileStoragePath);
            foreach (var runDir in runDirs)
            {
                var runDto = new TestRunner.Utilities.DTOs.RunDto();
                if (File.Exists(string.Format("{0}{1}{2}", runDir, Path.DirectorySeparatorChar, "run.json")))
                {
                    var runFileContent = File.ReadAllText(string.Format("{0}{1}{2}", runDir, Path.DirectorySeparatorChar, "run.json"));
                    runDto = JsonSerializer.Deserialize<TestRunner.Utilities.DTOs.RunDto>(runFileContent);
                }
                else
                {
                    var runId = runDir.Split(Path.DirectorySeparatorChar).Last();
                    runDto.RunId = runId;
                    runDto.Tests = new List<string>();
                }
                runList.Add(runDto);
            }
            return runList;
        }

        public RunResultVM GetRunSummary(string runId)
        {
            var filePathFormat = string.Format("{0}{1}{2}", RunnerConfiguration.FileStoragePath, Path.DirectorySeparatorChar, runId);
            var summaryJson = File.ReadAllText(string.Format("{0}{1}{2}", filePathFormat, Path.DirectorySeparatorChar, "summary.json"));
            var summaryModel = JsonSerializer.Deserialize<RunResultVM>(summaryJson);
            return summaryModel;
        }

        public TestDto GetTest(string runId, string testId)
        {
            var filePathFormat = string.Format("{0}{1}{2}", RunnerConfiguration.FileStoragePath, Path.DirectorySeparatorChar, runId);
            var testFileContent = File.ReadAllText(string.Format("{0}{1}{2}_{3}.json", filePathFormat, Path.DirectorySeparatorChar, testId, "test"));
            var testDto = JsonSerializer.Deserialize<TestRunner.Utilities.DTOs.TestDto>(testFileContent);
            return testDto;
        }

        public List<DataTreeDto> GetTestData(string runId, string testId)
        {
            string testDataFileContent = "";
            var dataTreeDto = new List<DataTreeDto>();
            var filePathFormat = string.Format("{0}{1}{2}", RunnerConfiguration.FileStoragePath, Path.DirectorySeparatorChar, runId);

            if (File.Exists(string.Format("{0}{1}{2}_{3}.json", filePathFormat, Path.DirectorySeparatorChar, testId, "testData")))
            {
                testDataFileContent = File.ReadAllText(string.Format("{0}{1}{2}_{3}.json", filePathFormat, Path.DirectorySeparatorChar, testId, "testData"));
                dataTreeDto = JsonSerializer.Deserialize<List<TestRunner.Utilities.DTOs.DataTreeDto>>(testDataFileContent);
            }

            return dataTreeDto;
        }

        public ErrorDto GetTestError(string runId, string testId)
        {
            ErrorDto errorDto = null;
            var filePathFormat = string.Format("{0}{1}{2}", RunnerConfiguration.FileStoragePath, Path.DirectorySeparatorChar, runId);
            if (File.Exists(string.Format("{0}{1}{2}_{3}.json", filePathFormat, Path.DirectorySeparatorChar, testId, "error")))
            {
                var errorFileContent = File.ReadAllText(string.Format("{0}{1}{2}_{3}.json", filePathFormat, Path.DirectorySeparatorChar, testId, "error"));
                errorDto = JsonSerializer.Deserialize<TestRunner.Utilities.DTOs.ErrorDto>(errorFileContent);
            }
            return errorDto;
        }

        public List<TestDto> GetTests(string runId)
        {
            var filePathFormat = string.Format("{0}{1}{2}", RunnerConfiguration.FileStoragePath, Path.DirectorySeparatorChar, runId);
            var executedTestsFiles = Directory.GetFiles(filePathFormat, "*test.json");
            var executedTests = executedTestsFiles.Select(x => JsonSerializer.Deserialize<TestRunner.Utilities.DTOs.TestDto>(System.IO.File.ReadAllText(x))).ToList();
            return executedTests;
        }

        public void SetRunSummary(string runId, RunResultVM runSummary)
        {
            var summaryJson = JsonSerializer.Serialize(runSummary);
            var filePathFormat = string.Format("{0}{1}{2}", RunnerConfiguration.FileStoragePath, Path.DirectorySeparatorChar, runId);
            File.WriteAllText(string.Format("{0}{1}{2}", filePathFormat, Path.DirectorySeparatorChar, "summary.json"), summaryJson);
        }

        public bool CheckAllTestsForStatus(string runId, string status)
        {
            throw new NotImplementedException();
        }

        List<TestDto> IReadResults.GetLabelTests(string label, int hours)
        {
            throw new NotImplementedException();
        }
        
        public List<RunDto> GetDailyRuns(List<string> labels = null, string date = "")
        {
            throw new NotImplementedException();
        }

        public List<TestDto> GetLastNTests(string testName, int count = 10)
        {
            //returning an empty list of tests until implemented
            return new List<TestDto>();
        }
    }
}
