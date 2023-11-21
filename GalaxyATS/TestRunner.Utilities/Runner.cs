using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using TestRunner.Utilities.DTOs;
using System.Text.Json;
using System.IO;
using System.Linq;
using LZAuto.Framework;

namespace TestRunner.Utilities
{
    public class Runner
    {
        private BackgroundWorker _bgWorker;
        private ILogResults _logger;
        private string _driverUrl;
        public static int passCount = 0;
        public static int failCount = 0;

        public Runner(ILogResults logger, BackgroundWorker bgWorker, string driverUrl)
        {
            _logger = logger;
            _bgWorker = bgWorker;
            _driverUrl = driverUrl;
        }

        public bool Execute(string runId, string label, List<string> Tests = null)
        {
            Type testFixture = null;

            if (_bgWorker.CancellationPending)
            {
                return true;
            }
            if (Tests == null)
            {
                Tests = new List<string>();
            }

            foreach (string testName in Tests)
            {
                var splitList = testName.Split('.').ToList();
                int findIndex = (splitList.Count - 1);
                string assemblyName = null;

                string testMethodName = splitList.ElementAt(findIndex);
                splitList.RemoveAt(findIndex);
                string fixtureName = splitList.ElementAt(findIndex - 1);
                splitList.RemoveAt(findIndex - 1);

                for(int i = 0; i <= splitList.Count - 1; i++)
                {
                    if (i < splitList.Count - 1)
                    {
                        assemblyName = (assemblyName + splitList[i] + ".");
                    }
                    else
                    {
                        assemblyName = (assemblyName + splitList[i]);
                    }
                }

                testFixture = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + $"{Path.DirectorySeparatorChar}{assemblyName}.dll").GetType("LZAuto.Tests." + fixtureName);

                var isTextFixture = Attribute.IsDefined(testFixture, typeof(LZAuto.Attributes.TestFixtureAttribute), false);

                if (isTextFixture)
                {
                    var testMethod = testFixture.GetMethod(testMethodName, BindingFlags.Instance | BindingFlags.Public);
                    var isTest = Attribute.IsDefined(testMethod, typeof(LZAuto.Attributes.TestAttribute), false);
                    if (isTest && !_bgWorker.CancellationPending)
                    {
                        //TODO: Clean up retry code
                        var testId = _logger.StartTest(runId, null, assemblyName, testFixture.Name, testMethod.Name, DateTime.UtcNow, label);
                        var i = 0;
                        StatusDto statusDto = null;
                        ErrorDto errorDto = null;
                        string status = "Failed";
                        while (i < RunnerConfiguration.Attempts)
                        {
                            if (_bgWorker.CancellationPending)
                            {
                                break;
                            }

                            i++;

                            try
                            {
                                var statusJson = ExecuteTest(testFixture, testMethod, _driverUrl);
                                statusDto = JsonSerializer.Deserialize<StatusDto>(statusJson);
                                status = statusDto.Status;
                                errorDto = statusDto.Error;
                            }
                            catch (Exception ex)
                            {
                                if (ex.InnerException != null)
                                {
                                    errorDto = new ErrorDto
                                    {
                                        ErrorType = ex.InnerException.GetType().FullName,
                                        Message = ex.InnerException.Message,
                                        Trace = ex.InnerException.StackTrace
                                    };
                                }
                                else
                                {
                                    errorDto = new ErrorDto
                                    {
                                        ErrorType = ex.GetType().FullName,
                                        Message = ex.Message,
                                        Trace = ex.StackTrace
                                    };
                                }
                            }
                        }
                        var test = _logger.EndTest(runId, testId, status, i, statusDto.TestData, errorDto, null, DateTime.UtcNow);
                        switch (test.Status)
                        {
                            case "Passed":
                                passCount++;
                                break;
                            case "Failed":
                                failCount++;
                                break;
                        }
                    }
                }
            }
            return true;
        }

        private string ExecuteTest(Type testFixture, MethodInfo testMethod, string driverUrl = "")
        {
            string statusJson = "";
            object instance = Activator.CreateInstance(testFixture);
            BrowserstackConfiguration.TestName = testMethod.Name;

            if (RunnerConfiguration.SuiteType.ToLower() == "nut")
            {
                statusJson = (string)testFixture.GetMethod("ExecuteTest").Invoke(instance, new object[] { testMethod.Name });
            }
            if (RunnerConfiguration.SuiteType.ToLower() == "db")
            {
                statusJson = (string)testFixture.GetMethod("ExecuteTest").Invoke(instance, new object[] { testMethod.Name });
            }
            if (RunnerConfiguration.SuiteType.ToLower() == "ui")
            {
                statusJson = (string)testFixture.GetMethod("ExecuteTest").Invoke(instance, new object[] { testMethod.Name, driverUrl });
            }
            if (RunnerConfiguration.SuiteType.ToLower() == "mobile")
            {
                statusJson = (string)testFixture.GetMethod("ExecuteMobileTest").Invoke(instance, new object[] { testMethod.Name, driverUrl });
            }


            return statusJson;
        }
    }
}
