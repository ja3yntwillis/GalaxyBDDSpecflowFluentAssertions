using System;
using Microsoft.AspNetCore.Mvc;
using TestRunner.Utilities;
using TestRunner.Utilities.DTOs;

namespace TestRunner.UI.Controllers
{
    public class ResultsApiController : Controller
    {
        public ILogResults ResultsLogger { get; }

        public ResultsApiController(ILogResults resultsLogger)
        {
            ResultsLogger = resultsLogger;
        }

        [HttpPost, Route("api/results/run")]
        public IActionResult CreateRun([FromBody] RunDto runDto)
        {
            var result = ResultsLogger.StartRun(runDto.BaseUrl, runDto.Application, runDto.Attribute, runDto.SuiteType, runDto.Browser, runDto.MaxThreads, runDto.Labels, runDto.Tests, runDto.StartTime, runDto.DatabaseName, runDto.DatabaseUsername);
            return new JsonResult(result);
        }

        [HttpPut, Route("api/results/run/{runId}")]
        public IActionResult EndRun([FromBody] RunDto runDto)
        {
            ResultsLogger.EndRun(runDto);
            return new JsonResult(runDto.RunId);
        }

        [HttpPost, Route("api/results/run/{runId}/test")]
        public IActionResult CreateTest(string runId, [FromBody] TestDto testDto)
        {
            var result = ResultsLogger.StartTest(runId, testDto.TestId, testDto.Assembly, testDto.Fixture, testDto.Method, testDto.StartTime, testDto.Labels);
            return new JsonResult(result);
        }

        [HttpPut, Route("api/results/run/{runId}/test/{testId}")]
        public IActionResult EndTest(string runId, [FromBody] TestDto testDto)
        {
            var errorDto = new ErrorDto()
            {
                ErrorType = testDto.ErrorType,
                Message = testDto.Message,
                PageTitle = testDto.PageTitle,
                Trace = testDto.Trace,
                Url = testDto.Url,
                ScreenshotBase64 = testDto.ScreenshotBase64,
            };
            ResultsLogger.EndTest(runId, testDto.TestId, testDto.Status, testDto.Attempts, null, errorDto, null, testDto.EndTime);
            return new JsonResult(testDto.TestId);
        }
    }
}
