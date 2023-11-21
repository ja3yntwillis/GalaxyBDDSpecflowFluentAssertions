using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Threading;
using Cedar.Configuration;
//using OraConnect.DbModel;
using TestRunner.Utilities.DTOs;
using LZAuto.Framework;
using DataGeneration.Galaxy.Models;

namespace TestRunner.Utilities
{
    public class TestManager
    {
        protected Runner _runner { get; set; }
        protected BackgroundWorker _bgWorkerTestManager { get; set; }
        protected List<BackgroundWorker> _bgWorkers { get; set; }
        protected List<string> TestList { get; set; }
        protected int MaximumConcurrency { get; set; }
        protected string _runId { get; set; }
        protected string _driverUrl { get; set; }
        protected ILogResults _logger { get; set; }
        protected Dictionary<string, string> vmRunData { get; set; }
        protected string _label { get; set; }

        public TestManager(ILogResults logger, List<string> testList)
        {
            _logger = logger;
            TestList = testList;
            MaximumConcurrency = RunnerConfiguration.MaximumConcurrency;
            _bgWorkers = new List<BackgroundWorker>();
        }

        public virtual void Execute(System.Threading.CancellationToken token)
        {
            RunDto runDto = _logger.StartRun(TestConfiguration.BaseURL, TestConfiguration.Application, TestConfiguration.Attribute, TestConfiguration.SuiteType, TestConfiguration.Browser, RunnerConfiguration.MaximumConcurrency, RunnerConfiguration.Labels, TestList, DateTime.UtcNow, TestConfiguration.DatabaseName, TestConfiguration.DbUserName);
            _runId = runDto.RunId;
            BrowserstackConfiguration.RunId = _runId;

            _label = RunnerConfiguration.Labels;
            
            //Shuffling the alphabetically sorted queued tests
            List<string> queuedTests = new List<string>();
            queuedTests.AddRange(TestList);
            int testCount = queuedTests.Count;
            Random random = new Random();
            for (int x = 0; x < 3; x++) //Looping it thrice to introduce more "Randomness"
            {
                for (int i = 0; i < testCount; i++)
                {
                    int r = i + random.Next(testCount - i);
                    var testName = queuedTests[r];
                    queuedTests[r] = queuedTests[i];
                    queuedTests[i] = testName;
                }
            }

            Setup();
            bool canConnect = false;

            if (!string.IsNullOrEmpty(TestConfiguration.DatabaseName))
            {
                try
                {
                    using (DevEHGalaxyContext dbContext = new DevEHGalaxyContext())
                    {
                        canConnect = dbContext.Database.CanConnect();
                    }
                }
                catch (Exception)
                {
                }
            }
            else
            {
                canConnect = true;
            }

            bool maximumConcurrencyMet = false;

            while (queuedTests.Count > 0)
            {
                if (!canConnect)
                {
                    string fullTestName = queuedTests.First();
                    var testNameParts = fullTestName.Split('.');
                    var errorDto = new ErrorDto
                    {
                        ErrorType = "DB Connection Error",
                        Message = "Cannot establish connection with DB",
                        Trace = null
                    };
                    var testId = _logger.StartTest(_runId, null, $"{testNameParts[0]}.{testNameParts[1]}", $"{testNameParts[2]}", $"{testNameParts[3]}", DateTime.UtcNow, _label);
                    var test = _logger.EndTest(_runId, testId, "Failed", 0, null, errorDto, null, DateTime.UtcNow);
                    switch(test.Status)
                    {
                        case "Passed":
                        Runner.passCount++;
                        break;
                        case "Failed":
                        Runner.failCount++;
                        break;
                    }
                    queuedTests.Remove(fullTestName);
                    continue;
                }

                if (_bgWorkers.Count < MaximumConcurrency)
                {
                    string test = queuedTests.First();
                    var bgWorker = new BackgroundWorker();
                    _bgWorkers.Add(bgWorker);
                    bgWorker.WorkerSupportsCancellation = true;
                    bgWorker.DoWork += new DoWorkEventHandler(backgroundTestRunner_DoWork);
                    bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundTestRunner_RunWorkerCompleted);

                    bgWorker.RunWorkerAsync(test);
                    queuedTests.Remove(test);
                }

                if (!maximumConcurrencyMet && RunnerConfiguration.SuiteType != "nut")
                {
                    if (_bgWorkers.Count == MaximumConcurrency)
                    {
                        maximumConcurrencyMet = true;
                    }
                    else
                    {
                        Thread.Sleep(1000);
                    }
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
            while (_bgWorkers.Count > 0)
            {
                Thread.Sleep(500);
            }

            Teardown();
            runDto.PassedCount = Runner.passCount;
            runDto.FailedCount = Runner.failCount;
            runDto.EndTime = DateTime.UtcNow;
            _logger.EndRun(runDto);

            //Reset counters to 0
            Runner.passCount = 0;
            Runner.failCount = 0;
        }

        private void backgroundTestRunner_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            _runner = new Runner(_logger, worker, _driverUrl);
            _runner.Execute(_runId, _label, new List<string> { (string)e.Argument });
        }

        // This event handler deals with the results of the
        // background operation.
        protected void backgroundTestRunner_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            // First, handle the case where an exception was thrown.
            if (e.Error != null)
            {
                Console.WriteLine(e.Error.Message);
            }
            // Dispose worker to release the thread
            _bgWorkers.Remove(worker);
            worker.Dispose();
        }

        virtual protected void Setup()
        {
        }
        virtual protected void Teardown()
        {
        }
    }
}