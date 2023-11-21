using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TestRunner.DTOs;
using TestRunner.UI.Services;
using TestRunner.UI.ViewModels;
using TestRunner.Utilities;

namespace TestRunner.UI.Controllers
{
    public class TestInformationController : Controller
    {
        public IBackgroundTaskQueue Queue { get; }

        public TestInformationController(IBackgroundTaskQueue queue)
        {
            Queue = queue;
        }

        [Route("TestInformation")]
        public IActionResult TestInformation(string type = "tests")
        {
            ViewBag.AppType = RunnerConfiguration.ApplicationType; // We need to find a better way then this for navigation bar

            var testDoc = new TestDocumenter();
            var viewModel = new TestInformationVM();
            //testDoc.SearchTests();

            switch (type.ToLower())
            {
                case "actions":
                    viewModel.Actions = testDoc.GetAllActions();
                    viewModel.DocType = type;
                    break;
                case "pageobjects":
                    viewModel.PageObjects = testDoc.GetAllPageObjects();
                    viewModel.DocType = type;
                    break;
                case "tests":
                    viewModel.Tests = testDoc.GetAllTests();
                    viewModel.DocType = type;
                    break;
            }
            return View(viewModel);
        }

        [Route("TestInformation/Test")]
        public IActionResult TestInformation()
        {

            var testDoc = new TestDocumenter();
            var viewModel = new TestInformationVM()
            {
                Tests = testDoc.GetAllTests(),
                DocType = "test",
            };
            return View("TestInformation");
        }

        [Route("TestInformation/Test/{name}")]
        public IActionResult Test(string name, string type = "actions")
        {
            ViewBag.AppType = RunnerConfiguration.ApplicationType; // We need to find a better way then this for navigation bar

            var testDoc = new TestDocumenter();
            var testDto = testDoc.GetTest(name);
            var viewModel = new TestInformationVM()
            {
                ClassName = testDto.ClassName,
                Method = testDto.Method,
                DocType = "test",
            };
            if (type == "actions")
            {
                viewModel.Actions = testDto.ActionDtos;
            }
            else if (type == "pageobjects")
            {
                viewModel.PageObjects = testDto.PageObjectDtos;
            }
            return View("Test", viewModel);
        }

        [Route("TestInformation/Action/{name}")]
        public IActionResult Action(string name, string type = "pageobjects")
        {
            ViewBag.AppType = RunnerConfiguration.ApplicationType; // We need to find a better way then this for navigation bar

            var testDoc = new TestDocumenter();
            var dto = testDoc.GetAction(name);
            var viewModel = new TestInformationVM()
            {
                ClassName = dto.ClassName,
                Method = dto.Method,
                Description = dto.Description,
                DocType = "action",
            };
            if (type == "tests")
            {
                viewModel.Tests = dto.TestDtos;
            }
            else if (type == "pageobjects")
            {
                viewModel.PageObjects = dto.PageObjectDtos;
            }
            return View("Action", viewModel);
        }

        [Route("TestInformation/PageObject/{name}")]
        public IActionResult PageObject(string name, string type = "actions")
        {
            ViewBag.AppType = RunnerConfiguration.ApplicationType; // We need to find a better way then this for navigation bar

            var testDoc = new TestDocumenter();
            var dto = testDoc.GetPageObject(name);
            var viewModel = new TestInformationVM()
            {
                ClassName = dto.ClassName,
                Description = dto.Description,
                DocType = "pageobject",
            };
            if (type == "tests")
            {
                viewModel.Tests = dto.TestDtos;
            }
            else if (type == "actions")
            {
                viewModel.Actions = dto.ActionDtos;
            }
            return View("PageObject", viewModel);
        }

        [Route("TestInformation/Generate")]
        public IActionResult GenerateTestDocumentation()
        {
            ViewBag.AppType = RunnerConfiguration.ApplicationType; // We need to find a better way then this for navigation bar

            Queue.QueueBackgroundWorkItem(async token =>
            {
                var testDoc = new TestDocumenter();
                testDoc.DocumentFrameworkMethods("tests");
            });

            Queue.QueueBackgroundWorkItem(async token =>
            {
                var testDoc = new TestDocumenter();
                testDoc.DocumentFrameworkMethods("actions");
            });

            Queue.QueueBackgroundWorkItem(async token =>
            {
                var testDoc = new TestDocumenter();
                testDoc.DocumentFrameworkMethods("clients");
            });

            return Redirect("/TestInformation");
        }

        [Route("TestInformation/ApiClients")]
        public IActionResult ApiClients()
        {
            ViewBag.AppType = RunnerConfiguration.ApplicationType; // We need to find a better way then this for navigation bar

            var testDoc = new TestDocumenter();

            var tests = testDoc.GetAllTests();

            var testActionsList = tests.SelectMany(test => test.Actions).ToList();

            var uiReferences = testActionsList.GroupBy(method => method)
                      .Select(method => new ReferenceDto
                      {
                          Method = method.Key,
                          Count = method.Count()
                      })
                      .Where(reference => !reference.Method.Contains("NavigateURL"))
                      .OrderByDescending(method => method.Count)
                      .ThenBy(method => method.Method)
                      .ToList();

            var clients = testDoc.GetAllApiClients();

            var clientActionsList = clients.SelectMany(test => test.Actions).ToList();

            var apiReferences = clientActionsList.GroupBy(method => method)
                      .Select(method => new ReferenceDto
                      {
                          Method = method.Key,
                          Count = method.Count()
                      })
                      .OrderByDescending(method => method.Count)
                      .ThenBy(method => method.Method)
                      .ToList();

            var model = new ApiClientsVM()
            {
                UiReferences = uiReferences,
                ApiReferences = apiReferences,
            };

            return View(model);
        }
    }
}
