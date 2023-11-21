using System.Collections.Generic;
using System.ComponentModel;
using Framework.Galaxy.Clients;
using LZAuto.Framework;

namespace TestRunner.Utilities
{
    public class TestManagerRemoteRunner : TestManager
    {

        public TestManagerRemoteRunner(ILogResults logger, List<string> testList, Dictionary<string, string> vmRunInformation) : base(logger, testList)
        {
            _logger = logger;
            TestList = testList;
            vmRunData = vmRunInformation;
        }

        public override void Execute(System.Threading.CancellationToken token)
        {

            var bgWorker = new BackgroundWorker();
            _bgWorkers.Add(bgWorker);
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.DoWork += new DoWorkEventHandler(backgroundTestRunner_DoWork);
            bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundTestRunner_RunWorkerCompleted);

            bgWorker.RunWorkerAsync(TestList);

        }

        private void backgroundTestRunner_DoWork(object sender, DoWorkEventArgs e)
        {
            string[] testlist = new string[TestList.Count];
            TestList.CopyTo(testlist);

            Dictionary<string, string> browserstackData = new Dictionary<string, string>()
                {
                    {"bsUsername", BrowserstackConfiguration.Username},
                    {"bsKey", BrowserstackConfiguration.Key},
                    {"bsAppUrl", BrowserstackConfiguration.AppUrl},
                    {"bsMobOsVer", BrowserstackConfiguration.mobileOSVersion},
                    {"bsMobDevice", BrowserstackConfiguration.mobileDevice}
                };

            //Call VM Execute method with all necessary information
            new VmClient().Execute(vmRunData, testlist, RunnerConfiguration.MaximumConcurrency, RunnerConfiguration.Attempts - 1, browserstackData);

        }
    }
}