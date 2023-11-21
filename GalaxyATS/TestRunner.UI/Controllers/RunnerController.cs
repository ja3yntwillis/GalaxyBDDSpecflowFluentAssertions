using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cedar.Configuration;
using Microsoft.AspNetCore.Mvc;
using TestRunner.Utilities;
using TestRunner.UI.Services;
using TestRunner.UI.Services.Interfaces;
using TestRunner.UI.ViewModels;
using TestRunner.Utilities.DTOs;
using LZAuto.Framework;

namespace TestRunner.UI.Controllers
{
    public class RunnerController : Controller
    {
        public IBackgroundTaskQueue Queue { get; }
        public IManageUserSettings UserSettingsManager { get; }
        public IAnalyzeTests TestAnalyzerService { get; }

        public RunnerController(IAnalyzeTests testAnalyzerService, IBackgroundTaskQueue queue, IManageUserSettings userSettingsManager)
        {
            Queue = queue;
            TestAnalyzerService = testAnalyzerService;
            UserSettingsManager = userSettingsManager;
        }

        [HttpGet, HttpPost, Route("Runner")]
        public IActionResult Runner(string reRunApplication, string reRunSuiteType, string reRunTestList)
        {
            if (RunnerConfiguration.ApplicationType != "local")
            {
                return Redirect("/");
            }

            TestAnalyzerService.BuildTestList();

            if (reRunSuiteType != null)
            {
                reRunSuiteType = reRunSuiteType.ToUpperInvariant();
            }

            var viewModel = new TestRunnerVM()
            {
                Assemblies = TestAnalyzerService.GetAssemblieList(),
                TestSuites = TestAnalyzerService.GetTestSuiteList().OrderBy(x => x).ToList(),
                Applications = TestAnalyzerService.GetAttributeList().OrderBy(x => x).ToList(),
                Urls = new List<string>()
                {
                    "API_ENT-DEV",
                    "API_ENT-UAT",
                    "API_ENT-STAGE",
                    "API_EUS-STAGE",
                    "API_ENT-PROD",
                    "API_EUS-PROD",
                    "API_ENT-SBX",
                    "API_ENTX-STRESS",
                    "API_ENT-ENTXPF",
                    "API_ENT-DEMO",
                    "API-USAA",
                    "LOCAL-UI",
                    "PORTAL_ENT-DEV",
                    "PORTAL_ENT-UAT",
                    "PORTAL_ENT-SBX",
                    "PORTAL_ENT-STS",
                    "PORTAL_ENT-XPF",
                    "PORTAL_ENT-STAGE",
                    "PORTAL_EUS-STAGE",
                    "PORTAL_ENT-PROD",
                    "PORTAL_EUS-PROD",
                    "PORTAL_ENT-DR",
                    "PORTAL_EUS-DR",
                    "PORTAL_FID-PROD",
                    "PORTAL_ENT-DEMO",
                    "PORTAL-USAA",
                    "ENT_DEV",
                    "ENT_UAT",
                    "ENT_STAGE",
                    "FID_DEV",
                    "ENT_PROD",
                    "EUS_PROD",
                    "ENT_PFX",
                    "ENT_USAA",
                    "MOBILE-IOS-DEV",
                    "MOBILE-IOS-UAT",
                    "MOBILE-IOS-SBX",
                    "MOBILE-IOS-DEMO",
                    "MOBILE-IOS-ENT-STAGE",
                    "MOBILE-IOS-EUS-STAGE",
                    "MOBILE-IOS-ENT-PROD",
                    "MOBILE-IOS-EUS-PROD",
                    "MOBILE-IOS-ENT-BETA",
                    "MOBILE-IOS-EUS-BETA",
                    "MOBILE-ANDROID-DEV",
                    "MOBILE-ANDROID-UAT",
                    "MOBILE-ANDROID-SBX",
                    "MOBILE-ANDROID-DEMO",
                    "MOBILE-ANDROID-ENT-STAGE",
                    "MOBILE-ANDROID-EUS-STAGE",
                    "MOBILE-ANDROID-ENT-PROD",
                    "MOBILE-ANDROID-EUS-PROD",
                    "MOBILE-ANDROID-ENT-BETA",
                    "MOBILE-ANDROID-EUS-BETA",
                },
                Presets = UserSettingsManager.GetAllTestPresetNames(),
                ReRunApplication = reRunApplication,
                ReRunSuiteType = reRunSuiteType,
                ReRunTestList = reRunTestList,
                DatabaseName = new List<string>()
                {
                    "ENTDEV_DEV_SUPPORT",
                    "ENT1RC_UAT_SUPPORT",
                    "ENIMP_UAT_SUPPORT",
                    "ENTS_STAGE_SUPPORT",
                    "EUSS_STAGE_SUPPORT",
                    "ENTX_STRESS_SUPPORT",
                    "ENTXX_STRESS_SUPPORT",
                    "ENTXPF_LOU_STRESS_SUPPORT",
                    "ENTDEMO_UAT_SUPPORT",
                    "ENTUSAA_UAT_SUPPORT"
                },
                VirtualMachine = new List<string>()
               {
                   "Select",
                   "AutomationTeam_Box1",
                   "AutomationTeam_Box2",
                   "Sajdevcoltst01_Box"
               },
                LocalServices = new List<string>()
               {
                    "AccountService",
                    "LoginService",
                    "ClaimService",
                    "AssistService",
                    "AlertNotificationService",
                    "ContributionService",
                    "DashboardService",
                    "DebitCardService",
                    "HelpTicketService",
                    "PaymentService",
                    "ReceiptService",
                    "TransactionService",
                    "UserDetailsService",
                    "RegistrationService",
                    "MenuService",
                    "InvestmentService"
               }
            };

            ViewBag.AppType = RunnerConfiguration.ApplicationType; // We need to find a better way then this for navigation bar

            return View(viewModel);
        }

        [HttpGet, Route("runner/testlist")]
        public IActionResult TestList(string testSuiteName, string attributeName, string assemblyName)
        {
            var viewModel = new TestListVM();
            viewModel.TestFixtures = TestAnalyzerService.GetTestListFiltered(testSuiteName, attributeName, assemblyName);
            return View(viewModel);
        }

        [HttpGet, Route("runner/reruntestlist/")]
        public IActionResult ReRunTestList(string reRunTestList)
        {
            var viewModel = new TestListVM();
            var testListSplit = reRunTestList.Split(",");
            var fixtureList = new List<TestFixtureDto>();
            var testListFormatted = new Dictionary<string, List<TestFixtureDto>>();
            foreach (var testName in testListSplit)
            {
                var nameSplit = testName.Split(".");
                var fixtureName = nameSplit[nameSplit.Count() - 2];
                var method = new TestMethodDto()
                {
                    Name = nameSplit[nameSplit.Count() - 1],
                    Description = "No documentation generated.",
                };
                var fixture = fixtureList.SingleOrDefault(x => x.Name == fixtureName);
                if (fixture == null)
                {
                    fixture = new TestFixtureDto()
                    {
                        Assembly = string.Join(".", nameSplit.Take(nameSplit.Count() - 2)),
                        Name = fixtureName,
                        Description = "",
                        TestMethods = new List<TestMethodDto>(),
                    };
                    fixtureList.Add(fixture);
                }
                fixture.TestMethods.Add(method);
            }
            viewModel.TestFixtures = fixtureList;
            return View("TestList", viewModel);
        }

        [HttpPost, Route("RunnerSubmit"), RequestFormLimits(ValueCountLimit = 5000)]
        public IActionResult RunnerSubmit(RunSubmitDto submittedRun)
        {
            TestManager testManager = null;

            UserSettingsManager.SetTestPreset("latest", submittedRun);

            if (string.IsNullOrWhiteSpace(submittedRun.BaseUrl))
            {
                submittedRun.BaseUrl = "localhost";
            }

            //Only look for http as it covers both the http & https case
            if (!submittedRun.BaseUrl.StartsWith("http"))
            {
                submittedRun.BaseUrl = SetBaseUrl(submittedRun.BaseUrl);
            }

            TestConfiguration.BaseURL = submittedRun.BaseUrl;
            TestConfiguration.Browser = submittedRun.Browser ?? "chromeHeadless";
            TestConfiguration.ResourcePath = string.Format("{0}Resources{1}", AppDomain.CurrentDomain.BaseDirectory, Path.DirectorySeparatorChar);
            RunnerConfiguration.ChromedriverPath = string.Format("{0}Resources", AppDomain.CurrentDomain.BaseDirectory);
            RunnerConfiguration.GeckodriverPath = string.Format("{0}Resources", AppDomain.CurrentDomain.BaseDirectory);
            RunnerConfiguration.EdgedriverPath = string.Format("{0}Resources", AppDomain.CurrentDomain.BaseDirectory);
            RunnerConfiguration.UpdateDriver = submittedRun.UpdateDriver;
            TestConfiguration.ImplicitTimeout = 15;
            TestConfiguration.ExplicitTimeout = submittedRun.Timeout;
            RunnerConfiguration.Attempts = 1 + submittedRun.Retries;
            TestConfiguration.LogToLocal = true;

            TestConfiguration.LogToDb = false;
            RunnerConfiguration.LogApiURL = "";
            TestConfiguration.EmailAddress = "LZSeleniumAutomation@gmx.com";
            TestConfiguration.AdminUsername = submittedRun.AdminUsername;
            TestConfiguration.AdminPassword = submittedRun.AdminPassword;
            TestConfiguration.Application = "Acclaris";
            if (submittedRun.Attributes == null || submittedRun.Attributes.Count == 0)
            {
                TestConfiguration.Attribute = "Acclaris";
            }
            else
            {
                TestConfiguration.Attribute = String.Join(",", submittedRun.Attributes);
            }

            //concatenating all the local service names with comma separation in a single string 
            if (submittedRun.LocalServices == null || submittedRun.LocalServices.Count == 0)
            {
                TestConfiguration.TfsUrl = "None selected";
            }
            else
            {
                TestConfiguration.TfsUrl = String.Join(",", submittedRun.LocalServices);
            }
            TestConfiguration.SuiteType = submittedRun.SuiteType;
            TestConfiguration.UserName = Environment.UserName;
            RunnerConfiguration.MaximumConcurrency = submittedRun.MaxThreads;
            RunnerConfiguration.Labels = $" Executed by {Environment.UserName}";
            RunnerConfiguration.SuiteType = submittedRun.SuiteType;

            RunnerConfiguration.LogTestData = submittedRun.LogTestData;
            TestConfiguration.LogTestData = submittedRun.LogTestData;
            TestConfiguration.DatabaseName = submittedRun.DatabaseName;
            TestConfiguration.DbUserName = submittedRun.DbUserName;
            TestConfiguration.DBPassword = submittedRun.DBPassword;
            TestConfiguration.App = submittedRun.App;
            TestConfiguration.Platform = submittedRun.SuiteType + "platform";

            BrowserstackConfiguration.Username = submittedRun.BrowserStackUsername;
            BrowserstackConfiguration.Key = submittedRun.BrowserStackKey;
            BrowserstackConfiguration.BrowserVersion = submittedRun.BrowserVersion;
            BrowserstackConfiguration.OperatingSystem = submittedRun.OperatingSystem;
            BrowserstackConfiguration.OsVersion = submittedRun.OsVersion;
            BrowserstackConfiguration.DeviceOperatingSystem = submittedRun.DeviceOperatingSystem;
            BrowserstackConfiguration.DeviceOsVersion = submittedRun.DeviceOsVersion;
            BrowserstackConfiguration.DeviceOrientation = submittedRun.DeviceOrientationDropdown;
            BrowserstackConfiguration.DeviceType = submittedRun.DeviceType;
            BrowserstackConfiguration.Device = submittedRun.DeviceDropdown;
            BrowserstackConfiguration.AppUrl = submittedRun.BrowserStackAppPath;
            BrowserstackConfiguration.BrowserVersion = submittedRun.BrowserVersion ?? "latest";
            BrowserstackConfiguration.mobileOSVersion = submittedRun.mobileOSVersion;
            BrowserstackConfiguration.mobileDevice = submittedRun.mobileDevice;

            //if (TelnyxPhoneNumbers.isFeatureAvailable) TelnyxPhoneNumbers.PopulateNumbers();

            if (submittedRun.UdId != null)
            {
                int port = 8101;
                TestConfiguration.UdId.Clear();
                submittedRun.UdId.Split(",").ToList().ForEach(i => TestConfiguration.UdId.Add(i.Replace("\r", "").Replace("\n", "").Trim(), port++));
                if (RunnerConfiguration.MaximumConcurrency > TestConfiguration.UdId.Count)
                {
                    RunnerConfiguration.MaximumConcurrency = TestConfiguration.UdId.Count;
                }
            }

            ILogResults logger = null;
            switch (RunnerConfiguration.StorageType)
            {
                case "file":
                    logger = new FileSystemResultsLogger();
                    break;
                case "azure":
                    logger = new AZStorageRESTResultsLogger();
                    break;
            }

            var dbServer = SetDbServer();
            SetConnectionString(dbServer);

            //Redirect to home when there are no tests chosen, or else it will error
            if (!(submittedRun.Tests?.Any() ?? false))
            {
                return Redirect("/Runner");
            }

            if (submittedRun.RunOnVM)
            {
                Dictionary<string, string> vmRunData = new Dictionary<string, string>()
                {
                    {"vmUrl", submittedRun.VmUrl},
                    {"vmUsername", submittedRun.VmUserName},
                    {"vmPassword", submittedRun.VmPassword},
                    {"vmBranch", submittedRun.VmBranch}
                };
                testManager = new TestManagerRemoteRunner(logger, submittedRun.Tests.ToList(), vmRunData);
            }

            else if (string.Equals(RunnerConfiguration.SuiteType, "ui", StringComparison.OrdinalIgnoreCase))
            {
                if (submittedRun.RunOnGrid)
                {
                    TestConfiguration.RunOnGrid = submittedRun.RunOnGrid;
                }
                testManager = new TestManagerUI(logger, TestConfiguration.Browser, submittedRun.Tests.ToList(), submittedRun.RunOnGrid);
            }
            else if (string.Equals(RunnerConfiguration.SuiteType, "nut", StringComparison.OrdinalIgnoreCase))
            {
                testManager = new TestManager(logger, submittedRun.Tests.ToList());
            }
            else if (string.Equals(RunnerConfiguration.SuiteType, "db", StringComparison.OrdinalIgnoreCase))
            {
                testManager = new TestManager(logger, submittedRun.Tests.ToList());
            }
            /*else if (string.Equals(RunnerConfiguration.SuiteType, "android", StringComparison.OrdinalIgnoreCase))
            {
                testManager = new TestManagerAppium(logger, BrowserstackConfiguration.DeviceType, submittedRun.Tests.ToList());
            }
            else if (string.Equals(RunnerConfiguration.SuiteType, "ios", StringComparison.OrdinalIgnoreCase))
            {
                testManager = new TestManagerAppium(logger, BrowserstackConfiguration.DeviceType, submittedRun.Tests.ToList());
            }
            else if (string.Equals(RunnerConfiguration.SuiteType, "mobile", StringComparison.OrdinalIgnoreCase))
            {
                testManager = new TestManagerAppium(logger, BrowserstackConfiguration.DeviceType, submittedRun.Tests.ToList());
            }*/
            Queue.QueueBackgroundWorkItem(async token =>
            {
                var guid = Guid.NewGuid().ToString();

                testManager.Execute(token);
            });
            System.Threading.Thread.Sleep(500);

            if (submittedRun.RunOnVM)
            {
                if (RunnerConfiguration.SuiteType.Equals("ANDROID"))
                {
                    return Redirect("https://automation-dashboard.participantportal.com/RunResult?url=https://app-automate.browserstack.com");
                }
                else
                {
                    return Redirect("https://automation-dashboard.participantportal.com/RunResult?url=" + TestConfiguration.BaseURL);
                }
            }
            else
            {
                return Redirect("/RunResult");
            }
        }

        private string SetBaseUrl(string url)
        {
            string baseUrl = string.Empty;

            switch (url)
            {
                case "LOCAL-UI":
                    baseUrl = "http://localhost/#/username";
                    break;
                case "API_ENT-DEV":
                    baseUrl = "https://dev-mobile-ig.participantportal.com/dev";
                    break;
                case "API_ENT-UAT":
                    baseUrl = "https://dev-mobile-ig.participantportal.com/uat";
                    break;
                case "API_ENT-STAGE":
                    baseUrl = "https://mobile-ig.participantportal.com/stg";
                    break;
                case "API_EUS-STAGE":
                    baseUrl = "https://mobile-ig.participantportal.com/stu";
                    break;
                case "API_ENT-PROD":
                    baseUrl = "https://mobile-ig.participantportal.com/ent";
                    break;
                case "API_EUS-PROD":
                    baseUrl = "https://mobile-ig.participantportal.com/eus";
                    break;
                case "API_ENT-SBX":
                    baseUrl = "https://dev-mobile-ig.participantportal.com/sbx";
                    break;
                case "API_ENTX-STRESS":
                    baseUrl = "https://dev-mobile-ig.participantportal.com/sts";
                    break;
                case "API_ENT-ENTXPF":
                    baseUrl = "https://dev-mobile-ig.participantportal.com/pfx";
                    break;
                case "API_ENT-DEMO":
                    baseUrl = "https://mobile-ig.participantportal.com/demo";
                    break;
                case "API-USAA":
                    baseUrl = "https://dev-mobile-ig.participantportal.com/usaa";
                    break;
                case "PORTAL_ENT-DEV":
                    baseUrl = "https://ent-dev.participantportal.com";
                    break;
                case "PORTAL_ENT-STAGE":
                    baseUrl = "https://ent-stg.participantportal.com";
                    break;
                case "PORTAL_EUS-STAGE":
                    baseUrl = "https://eus-stg.viabenefitsaccounts.com";
                    break;
                case "PORTAL_ENT-PROD":
                    baseUrl = "https://viabenefitsaccounts.com";
                    break;
                case "PORTAL_EUS-PROD":
                    baseUrl = "https://eus.viabenefitsaccounts.com";
                    break;
                case "PORTAL_ENT-DR":
                    baseUrl = "https://ent-dr.participantportal.com";
                    break;
                case "PORTAL_EUS-DR":
                    baseUrl = "https://eus-dr.participantportal.com";
                    break;
                case "PORTAL_FID-PROD":
                    baseUrl = "https://www.acclarisonline.com";
                    break;
                case "PORTAL_ENT-UAT":
                    baseUrl = "https://ent-uat.participantportal.com";
                    break;
                case "PORTAL_ENT-SBX":
                    baseUrl = "https://ent-sbx.participantportal.com";
                    break;
                case "PORTAL_ENT-STS":
                    baseUrl = "https://ent-sts.participantportal.com";
                    break;
                case "PORTAL_ENT-XPF":
                    baseUrl = "https://pf-uat.participantportal.com";
                    break;
                case "PORTAL_ENT-DEMO":
                    baseUrl = "https://demo.viabenefitsaccounts.com";
                    break;
                case "PORTAL-USAA":
                    baseUrl = "https://usaa-uat.participantportal.com";
                    break;
                case "ENT_DEV":
                    baseUrl = "EnterpriseDev";
                    break;
                case "ENT_UAT":
                    baseUrl = "EnterpriseUAT";
                    break;
                case "ENT_STAGE":
                    baseUrl = "EnterpriseStage";
                    break;
                case "FID_DEV":
                    baseUrl = "Fidelity Dev";
                    break;
                case "ENT_PROD":
                    baseUrl = "Enterprise PROD";
                    break;
                case "EUS_PROD":
                    baseUrl = "Onshore PROD";
                    break;
                case "ENT_PFX":
                    baseUrl = "Payflex";
                    break;
                case "ENT_USAA":
                    baseUrl = "USAA";
                    break;
                case "MOBILE-ANDROID-DEV":
                case "MOBILE-ANDROID-UAT":
                case "MOBILE-ANDROID-SBX":
                case "MOBILE-ANDROID-DEMO":
                case "MOBILE-ANDROID-ENT-STAGE":
                case "MOBILE-ANDROID-EUS-STAGE":
                case "MOBILE-ANDROID-ENT-PROD":
                case "MOBILE-ANDROID-EUS-PROD":
                case "MOBILE-ANDROID-ENT-BETA":
                case "MOBILE-ANDROID-EUS-BETA":
                case "MOBILE-IOS-DEV":
                case "MOBILE-IOS-UAT":
                case "MOBILE-IOS-SBX":
                case "MOBILE-IOS-DEMO":
                case "MOBILE-IOS-ENT-STAGE":
                case "MOBILE-IOS-EUS-STAGE":
                case "MOBILE-IOS-ENT-PROD":
                case "MOBILE-IOS-EUS-PROD":
                case "MOBILE-IOS-ENT-BETA":
                case "MOBILE-IOS-EUS-BETA":
                    baseUrl = url;
                    break;
            }

            return baseUrl;
        }

        private string SetDbServer()
        {
            string dbServer = string.Empty;

            switch (TestConfiguration.DatabaseName)
            {
                case "ENTDEV_DEV_SUPPORT":
                case "ENT1RC_UAT_SUPPORT":
                case "ENIMP_UAT_SUPPORT":
                case "ENTX_STRESS_SUPPORT":
                case "ENTXX_STRESS_SUPPORT":
                case "ENTXPF_LOU_STRESS_SUPPORT":
                case "ENTUSAA_UAT_SUPPORT":
                    dbServer = "na44-ba-exac001d-mgduo-scan.drmpintdatabas.bana44.oraclevcn.com";
                    break;
                case "EUSS_STAGE_SUPPORT":
                case "ENTS_STAGE_SUPPORT":
                case "ENTDEMO_UAT_SUPPORT":
                    dbServer = "na42-ba-exac001p-dykqs-scan.prmplintdatabas.bana42.oraclevcn.com";
                    break;
            }
            return dbServer;
        }

        private void SetConnectionString(string dbServer)
        {
            TestConfiguration.connectionString = $"DATA SOURCE={dbServer}:1521/{TestConfiguration.DatabaseName}; " +
            $"PERSIST SECURITY INFO=True;" +
            $"USER ID={TestConfiguration.DbUserName}; password={TestConfiguration.DBPassword}; " +
            $"Pooling = True;" +
            "Connection Timeout=75;";
        }
    }
}
