using System.Collections.Generic;
using System.ComponentModel;
using TestRunner.Utilities.Selenium;
using OpenQA.Selenium.Remote;
using System;

namespace TestRunner.Utilities
{
    public class TestManagerUI : TestManager
    {
        private ICommandServer DriverService;
        private string Browser;
        private bool isRunOnGrid;

        public TestManagerUI(ILogResults logger, string browser, List<string> testList, bool isRunOnGrid = false) : base(logger, testList)
        {
            _logger = logger;
            TestList = testList;
            MaximumConcurrency = RunnerConfiguration.MaximumConcurrency;
            _bgWorkers = new List<BackgroundWorker>();
            Browser = browser.Replace("headless", "");
            this.isRunOnGrid = isRunOnGrid;
        }

        protected override void Setup()
        {
            var webdriverManager = new Webdriver();
            if (Browser.Contains("-BS", StringComparison.OrdinalIgnoreCase))
            {
                _driverUrl = "https://hub-us.browserstack.com/wd/hub/";
            }
            else
            {
                string ip = isRunOnGrid ? "10.128.154.76" : "127.0.0.1";
                DriverService = webdriverManager.StartWebDriverService(Browser, ip, 4444);
                _driverUrl = $"http://{ip}:4444";
            }
        }
        protected override void Teardown()
        {
            if (DriverService != null)
            {
                DriverService.Dispose();
            }
        }
    }
}