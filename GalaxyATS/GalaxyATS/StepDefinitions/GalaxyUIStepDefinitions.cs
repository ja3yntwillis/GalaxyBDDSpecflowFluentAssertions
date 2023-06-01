using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using TechTalk.SpecFlow;

namespace GalaxyATS.StepDefinitions
{
    [Binding]
    public class GalaxyUIStepDefinitions
    {
       // private ChromeDriver chromeDriver;
        IWebDriver driver = new ChromeDriver("Drivers/Chrome/chromedriver.exe");

        [Given(@"I have navigated to ""([^""]*)"" portal")]
        public void GivenIHaveNavigatedToPortal(string url)
        {
            driver.Navigate().GoToUrl(url);
            driver.Manage().Window.Maximize();
            System.Threading.Thread.Sleep(60000);
            Assert.IsTrue(driver.Title.Contains("Via Benefits - Account Management"));
            driver.Quit();
                }
    }
}
