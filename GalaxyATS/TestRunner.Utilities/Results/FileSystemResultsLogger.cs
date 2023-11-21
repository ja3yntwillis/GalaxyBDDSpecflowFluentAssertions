using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using TestRunner.Utilities.DTOs;

namespace TestRunner.Utilities
{
    public class FileSystemResultsLogger : ILogResults
    {
        private static Random _random;
        private static Random _Random
        {
            get
            {
                if (_random == null)
                {
                    int currentProcessId = Process.GetCurrentProcess().Id;
                    int seed = Environment.TickCount + currentProcessId;
                    _random = new Random(seed);
                }
                return _random;
            }
            set
            { }
        }
        private string ResultsPath { get; set; }


        public FileSystemResultsLogger()
        {
            ResultsPath = RunnerConfiguration.FileStoragePath;
            if (!Directory.Exists(ResultsPath))
            {
                Directory.CreateDirectory(ResultsPath);
            }
        }

        public void buildDataJson(object data, int parentId)
        {
            throw new NotImplementedException();
        }

        public void EndRun(RunDto runDto)
        {
            var filePath = string.Format("{0}{1}{2}{1}run.json", ResultsPath, Path.DirectorySeparatorChar, runDto.RunId);
            var runJson = JsonSerializer.Serialize(runDto);
            File.WriteAllText(filePath, runJson);
        }

        public TestDto EndTest(string runId, string testId, string status, int attempts, string testData, ErrorDto errorDto, ActionDto actionDto, DateTime endTime)
        {
            var filePathFormat = string.Format("{0}{1}{2}{1}{3}_{4}.json", ResultsPath, Path.DirectorySeparatorChar, runId, testId, "{0}");
            var testFileContent = File.ReadAllText(string.Format(filePathFormat, "test"));

            var testDto = JsonSerializer.Deserialize<TestDto>(testFileContent);

            testDto.Attempts = attempts;
            testDto.EndTime = endTime;
            testDto.Status = status;
            testDto.TestDescription = GetTestDescription(testDto.Method);
            var testJson = JsonSerializer.Serialize(testDto);
            var errorJson = JsonSerializer.Serialize(errorDto);
            var actionJson = JsonSerializer.Serialize(actionDto);


            if (!string.IsNullOrEmpty(testData))
            {
                File.WriteAllText(string.Format(filePathFormat, "testData"), testData);
            }

            if (errorJson != "null")
                File.WriteAllText(string.Format(filePathFormat, "error"), errorJson);
            if (testJson != "null")
                File.WriteAllText(string.Format(filePathFormat, "test"), testJson);
            if (actionJson != "null")
                File.WriteAllText(string.Format(filePathFormat, "actions"), actionJson);

            return testDto;
        }

        public string GetTestDescription(string methodName)
        {
            string description = string.Empty;
            string docPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar + "Tests.Acclaris.xml";

            if (File.Exists(docPath))
            {
                var xmlDoc = XDocument.Load(docPath);
                description = xmlDoc.Descendants("members").FirstOrDefault().Descendants("member")
                    .Where(t => t.Attribute("name").Value.Contains(methodName))
                    .FirstOrDefault()?.Element("summary").Value.Trim() ?? "NO DOCUMENTATION FOR THIS TEST!";
            }

            return description;
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
            var runId = DateTime.Now.ToString("yyyyMMddHHmmssfffffff");
            if (!Directory.Exists(string.Format("{0}{1}{2}", ResultsPath, Path.DirectorySeparatorChar, runId)))
            {
                Directory.CreateDirectory(string.Format("{0}{1}{2}", ResultsPath, Path.DirectorySeparatorChar, runId));
            }
            var runDto = new RunDto()
            {
                RunId = runId,
                Application = application,
                Attribute = attribute,
                SuiteType = suiteType,
                BaseUrl = url,
                Browser = browser,
                UserName = Environment.UserName,
                StartTime = startTime,
                MaxThreads = threads,
                Labels = labels,
                Tests = tests,
                TestsCount = tests.Count,
                DatabaseName = dbName,
                DatabaseUsername = dbUsername,
            };
            var filePathFormat = string.Format("{0}{1}{2}{1}{3}.json", ResultsPath, Path.DirectorySeparatorChar, runId, "run");
            var runJson = JsonSerializer.Serialize(runDto);
            if (runJson != "null")
            {
                File.WriteAllText(filePathFormat, runJson);
            }
            return runDto;
        }

        public string StartTest(string runId, string testId, string assembly, string fixture, string method, DateTime startTime, string labels)
        {
            testId = GetTestId();
            var testDto = new TestDto()
            {
                Assembly = assembly,
                TestId = testId,
                Fixture = fixture,
                Method = method,
                StartTime = startTime,
                Status = "Started",
                Labels = labels
            };
            var filePathFormat = string.Format("{0}{1}{2}{1}{3}_{4}.json", ResultsPath, Path.DirectorySeparatorChar, runId, testId, "test");
            var testJson = JsonSerializer.Serialize(testDto);
            if (testJson != "null")
            {
                File.WriteAllText(filePathFormat, testJson);
            }
            return testId;
        }

        public static string GetTestId()
        {
            var chars = "0123456789ABCDEFGHIJKLMOPQRSTUVWXYZ";
            var stringChars = new char[8];
            for (int i = 0; i < 8; i++)
            {
                stringChars[i] = chars[_Random.Next(chars.Length)];
            }
            return new string(stringChars);
        }
    }
}
