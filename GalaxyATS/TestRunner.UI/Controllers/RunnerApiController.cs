using Microsoft.AspNetCore.Mvc;
using System.Linq;
using TestRunner.UI.Services.Interfaces;
using TestRunner.Utilities;
using TestRunner.Utilities.DTOs;

namespace TestRunner.UI.Controllers
{
    public class RunnerApiController : Controller
    {
        public IAnalyzeTests TestAnalyzerService { get; }

        public RunnerApiController(IAnalyzeTests testAnalyzerService)
        {
            TestAnalyzerService = testAnalyzerService;
        }

        [HttpGet, Route("api/runner/testlist")]
        public IActionResult GetTestList(string testSuiteName, string attributeName, string assemblyName)
        {
            var result = TestAnalyzerService.GetTestListFiltered(testSuiteName, attributeName, assemblyName);
            return new JsonResult(result);
        }
    }
}
