using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
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
            //driver.Quit();
                }


        [Then(@"I have entered ""([^""]*)"" in the ""([^""]*)"" field")]
        public void ThenIHaveEnteredInTheField(string valueOfTheField, string fieldName)
        {
            var inputField = driver.FindElement(By.Id(fieldName));
          //  var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
           // wait.Until(ExpectedConditions.ElementIsVisible(By.Id(fieldName)));
            inputField.SendKeys(valueOfTheField);
            System.Threading.Thread.Sleep(1000);

           
        }

        [Then(@"I click on the ""([^""]*)"" button")]
        public void ThenIClickOnTheButton(string fieldName)
        {
            driver.FindElement(By.XPath(fieldName)).Click();
            System.Threading.Thread.Sleep(20000);
            //Assert.IsTrue(driver.Title.Contains("Via Benefits - Account Management"));
        }

        [Then(@"I have verified ""([^""]*)"" text")]
        public void ThenIHaveVerifiedText(string valueOfTheText)
        {
          var fieldName= driver.FindElement(By.XPath("//*[contains(text(),'"+ valueOfTheText +"')]")).Text;
            fieldName.Should().Be(valueOfTheText);
        } 

        [Then(@"I close the browser")]
        public void ThenICloseTheBrowser()
        {
            driver.Quit();
        }
        
    }
}
