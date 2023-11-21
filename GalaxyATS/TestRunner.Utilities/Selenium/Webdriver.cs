using OpenQA.Selenium;
//using OpenQA.Selenium.Appium.Service;
//using OpenQA.Selenium.Appium.Service.Options;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using Microsoft.Edge.SeleniumTools;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;

namespace TestRunner.Utilities.Selenium
{
    public class Webdriver
    {

        public ICommandServer StartWebDriverService(string browserName, string hostnameToBind = "localhost", int portToBind = 0)
        {
            switch (browserName.ToLower())
            {
                case "chrome":
                case "chromeheadless":
                    if(RunnerConfiguration.UpdateDriver)
                    {
                        var installDriver = new InstallChromeDriver();
                        installDriver.Install();
                    }

                    var chromeDriverService = ChromeDriverService.CreateDefaultService(RunnerConfiguration.ChromedriverPath);
                    if (Environment.OSVersion.Platform == PlatformID.Unix)
                    {
                        chromeDriverService.WhitelistedIPAddresses = hostnameToBind + ",::ffff:7f00:1";
                    }
                    if (portToBind != 0)
                    {
                        chromeDriverService.Port = portToBind;
                    }

                    chromeDriverService.Start();
                    return chromeDriverService;
                    break;
                case "firefox":
                case "firefoxheadless":
                    // ToDo (Niravkumar/Matt): As of now we can run only one thread with Firefox browser. As of not moving forward with whatever capabilities we are getting with Firefox.
                    // Gecko driver is not supporting multiple instances to start at the same time. Need to look into this. Guess: We may need to bind different port for each instance?
                    var geckoDriverService = FirefoxDriverService.CreateDefaultService(RunnerConfiguration.GeckodriverPath);
                    if (portToBind != 0)
                    {
                        geckoDriverService.Port = portToBind;
                    }
                    geckoDriverService.Start();
                    return geckoDriverService;
                    break;
                case "edge":
                case "edgeheadless":
                    var edgeDriverService = EdgeDriverService.CreateDefaultService(RunnerConfiguration.EdgedriverPath, @"msedgedriver.exe");
                    if (portToBind != 0)
                    {
                        edgeDriverService.Port = portToBind;
                    }
                    edgeDriverService.Start();
                    return edgeDriverService;
                    break;
                default:
                    throw new NotSupportedException("Unsupported browser name of: ''");
            }
        }

        /*public AppiumLocalService StartAppiumServer()
        {
            var service = new AppiumServiceBuilder();
            var arguments = new OptionCollector();
            arguments.AddArguments(new KeyValuePair<string, string>("--relaxed-security", string.Empty));
            service.WithArguments(arguments);
            service.UsingAnyFreePort();
            var appiumLocalService = service.Build();
            appiumLocalService.Start();
            return appiumLocalService;
        }*/

        public ICapabilities GetDriverCapabilities(string browserName)
        {
            ICapabilities capabilities = null;
            switch (browserName.ToLower())
            {
                case "chrome":
                    var chromeOpts = new ChromeOptions();
                    chromeOpts.AddArguments("--no-sandbox");
                    //chromeOpts.AddArguments("--disable-infobars"); //Commented out since current version of Chrome has removed this feature
                    chromeOpts.AddAdditionalCapability("useAutomationExtension", false);
                    chromeOpts.AddExcludedArgument("enable-automation");
                    chromeOpts.AddUserProfilePreference("credentials_enable_service", false);
                    capabilities = chromeOpts.ToCapabilities();
                    break;
                case "chromeheadless":
                    var chromeHeadlessOpts = new ChromeOptions();
                    chromeHeadlessOpts.AddArguments("--no-sandbox");
                    //chromeHeadlessOpts.AddArguments("--disable-infobars"); //Commented out since current version of Chrome has removed this feature
                    chromeHeadlessOpts.AddArguments("--headless");
                    chromeHeadlessOpts.AddArguments("--window-size=1200x900");
                    chromeHeadlessOpts.AddArguments("--disable-gpu");
                    chromeHeadlessOpts.AddAdditionalCapability("useAutomationExtension", false);
                    chromeHeadlessOpts.AddExcludedArgument("enable-automation");
                    chromeHeadlessOpts.AddUserProfilePreference("credentials_enable_service", false);
                    capabilities = chromeHeadlessOpts.ToCapabilities();
                    break;
                default:
                    throw new NotSupportedException("Unsupported browser name of: ''");
            }
            return capabilities;
        }
    }
}
