using AventStack.ExtentReports.Gherkin.Model;
using AventStack.ExtentReports;
using System.Reflection;
using AventStack.ExtentReports.Reporter;
using System.IO;
using TechTalk.SpecFlow;

namespace GalaxyATS
{
    [Binding]
    class Hook : TechTalk.SpecFlow.Steps
    {
        private static ExtentTest scenario;
        private static ExtentReports extent;
        private static ExtentTest featureName;
        private static string ReportPath;

        [BeforeScenario]
        public void BeforeScenario(ScenarioContext scenarioContext)
        {
            Console.WriteLine("BeforeScenario");
            scenario = extent.CreateTest(scenarioContext.ScenarioInfo.Title);
            //scenario = featureName.CreateNode<Scenario>(ScenarioContext.Current.ScenarioInfo.Title);
        }

        [BeforeTestRun]
        public static void InitializeReport()
        {
            string path1 = AppDomain.CurrentDomain.BaseDirectory.Replace("\\bin\\Debug\\net6.0", "");
            ReportPath = path1 + "Report\\index.html";
            var htmlReporter = new ExtentHtmlReporter(ReportPath);
            extent = new ExtentReports();
            extent.AttachReporter(htmlReporter);
        }
        [AfterTestRun]
        public static void TearDownReport()
        {
            extent.Flush();
            System.Diagnostics.Process.Start(ReportPath);
        }
        [AfterStep]
        public void InsertReportingSteps(ScenarioContext sc)
        {
           // ExtentTestfeatureName = extent.CreateTest<Feature>(FeatureContext.Current.FeatureInfo.Title);
           // ExtentTestscenario = featureName.CreateNode<Scenario>(ScenarioContext.Current.ScenarioInfo.Title);
            var stepType = ScenarioStepContext.Current.StepInfo.StepDefinitionType.ToString();
            PropertyInfo pInfo = typeof(ScenarioContext).GetProperty("ScenarioExecutionStatus", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo getter = pInfo.GetGetMethod(nonPublic: true);
            object TestResult = getter.Invoke(sc, null);
           // ScenarioContext.Current.ScenarioExecutionStatus.ToString();
            if (sc.TestError == null)
            {
                
                if (stepType == "Given")
                    scenario.CreateNode<Given>(stepType + " " + ScenarioStepContext.Current.StepInfo.Text);
                else if (stepType == "When")
                    scenario.CreateNode<When>(stepType + " " + ScenarioStepContext.Current.StepInfo.Text);
                else if (stepType == "Then")
                    scenario.CreateNode<Then>(stepType + " " + ScenarioStepContext.Current.StepInfo.Text);
                else if (stepType == "And")
                    scenario.CreateNode<And>(stepType + " " + ScenarioStepContext.Current.StepInfo.Text);
            }
            else
            {
                if (stepType == "Given")
                    scenario.CreateNode<Given>(stepType + " " + ScenarioStepContext.Current.StepInfo.Text).Fail(sc.TestError.Message);
                if (stepType == "When")
                    scenario.CreateNode<When>(stepType + " " + ScenarioStepContext.Current.StepInfo.Text).Fail(sc.TestError.Message);
                if (stepType == "Then")
                    scenario.CreateNode<Then>(stepType + " " + ScenarioStepContext.Current.StepInfo.Text).Fail(sc.TestError.Message);
                if (stepType == "And")
                    scenario.CreateNode<And>(stepType + " " + ScenarioStepContext.Current.StepInfo.Text).Fail(sc.TestError.Message);
            }
        }
        //[BeforeFeature]
        //public static void BeforeFeature(ScenarioContext scenarioContext)
        //{
        //    featureName = extent.CreateTest(scenarioContext.ScenarioInfo.Title);
        //}
    }
}
