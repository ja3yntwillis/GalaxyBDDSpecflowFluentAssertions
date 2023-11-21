using System;
using TestRunner.Utilities;

namespace TestRunner.UI
{
    public class UIRunnerConfigurationParser : IParseRunnerConfiguration
    {
        public bool GetOption(string arg, string value, int pos)
        {
            string option = arg.Substring(pos, arg.Length - pos);

            if (option == "h" || option == "allowedhosts")
            {
                RunnerConfiguration.AllowedHosts = value.ToLower();
            }
            else if (option == "p" || option == "dashboardport")
            {
                RunnerConfiguration.DashboardPort = Convert.ToInt32(value);
            }
            else if (option == "azdevopskey")
            {
                RunnerConfiguration.AZDevOpsAPIKey = value;
            }
            else
            {
                return false;
            }

            return true;
        }

        public void LoadDefaultOptions()
        {
            RunnerConfiguration.AllowedHosts = "localhost;enforcers1.liazon.corp;automation.grpeapp.com";
            RunnerConfiguration.StorageType = "file";
            RunnerConfiguration.FileStoragePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}{System.IO.Path.DirectorySeparatorChar}SeTestResults";
            RunnerConfiguration.LocalResultsPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}{System.IO.Path.DirectorySeparatorChar}SeTestResults";
            RunnerConfiguration.TestInventoryFileStoragePath = string.Format("{0}TestInventory", AppDomain.CurrentDomain.BaseDirectory);
            RunnerConfiguration.DashboardPort = 5000;
            RunnerConfiguration.LoggingAPIUrl = "https://automation-dashboard.participantportal.com";
            RunnerConfiguration.ApplicationType = "local";
        }
    }
}
