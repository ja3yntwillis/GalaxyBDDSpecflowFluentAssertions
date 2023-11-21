using Microsoft.AspNetCore.Mvc;
using TestRunner.Utilities;
using TestRunner.UI.ViewModels;
using TestRunner.Utilities.DTOs;

namespace TestRunner.UI.Controllers
{
    public class SettingsController : Controller
    {
        public IManageUserSettings UserSettingsManager { get; }

        public SettingsController(IManageUserSettings userSettingsManager)
        {
            UserSettingsManager = userSettingsManager;
        }

        [HttpGet, Route("ApplicationSettings")]
        public IActionResult ApplicationSettings()
        {
            return View();
        }

        [HttpGet, Route("TestPresets")]
        public IActionResult TestPreset()
        {
            var presetList = UserSettingsManager.GetAllTestPresetNames();
            var viewModel = new TestPresetVM()
            {
                CurrentPresets = presetList,
            };

            return View(viewModel);
        }

        [HttpPost, Route("TestPreset")]
        public IActionResult SetTestPreset(string presetName, RunSubmitDto runSubmitDto)
        {
            return View();
        }
    }
}
