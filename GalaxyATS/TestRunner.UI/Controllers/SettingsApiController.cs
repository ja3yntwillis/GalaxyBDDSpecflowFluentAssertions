using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TestRunner.Utilities;
using TestRunner.Utilities.DTOs;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace TestRunner.UI.Controllers
{
    public class SettingsApiController : Controller
    {
        public IManageUserSettings SettingsManager { get; }

        public SettingsApiController(IManageUserSettings settingsManager)
        {
            SettingsManager = settingsManager;
        }

        [HttpGet, Route("api/settings/preset")]
        public IActionResult GetPrest(string name)
        {
            var result = SettingsManager.GetTestPreset(name);
            return new JsonResult(result);
        }

        [HttpPost, Route("api/settings/preset")]
        public IActionResult SetPrest(string presetName, RunSubmitDto presetDto, IFormFile presetFile)
        {
            var response = new JsonResult("");
            if (presetFile != null)
            {
                try
                {
                    if (presetFile.FileName.EndsWith(".json"))
                    {
                        presetName = Path.GetFileName(presetFile.FileName);
                        var content = "";
                        using (var reader = new StreamReader(presetFile.OpenReadStream()))
                        {
                            content = reader.ReadToEnd();
                        }
                        presetDto = JsonSerializer.Deserialize<RunSubmitDto>(content);
                    }
                }
                catch (Exception)
                {

                }
            }
            presetName = Regex.Replace(presetName, @"json|[^\w]", "", RegexOptions.None);
            if (presetName != "" && presetName.Length <= 75)
            {
                if (presetDto != null)
                {
                    if (!SettingsManager.CheckTestPreset(presetName))
                    {
                        if (SettingsManager.SetTestPreset(presetName, presetDto))
                        {
                            response.Value = presetName;
                        }
                    }
                    else
                    {
                        response.Value = "Test Preset already exists with that name.";
                        response.StatusCode = 400;
                    }
                }
                else
                {
                    response.Value = "Invalid Preset object.";
                    response.StatusCode = 400;
                }
            }
            else
            {
                response.Value = "Invalid Preset name. Names may contain letters, numbers, or underscores.  Maximum character count is 75.";
                response.StatusCode = 400;
            }
            return response;
        }

        [HttpDelete, Route("api/settings/preset")]
        public IActionResult DeletePrest(string name)
        {
            var result = SettingsManager.DeleteTestPreset(name);
            return new JsonResult(result);
        }

        [HttpPut, Route("api/settings/preset")]
        public IActionResult UpdateOldPrest(string name)
        {
            var response = new JsonResult("");
            var oldPresetDto = SettingsManager.GetTestPreset(name);
            if (oldPresetDto != null)
            {
                oldPresetDto.SuiteType = "UI";
                oldPresetDto.Tests = oldPresetDto.Tests.Select(x => "Tests.UI." + x).ToArray();
                oldPresetDto.Application = oldPresetDto.Application.Split("-")[0];
                oldPresetDto.Attribute = oldPresetDto.Application;

                if (SettingsManager.SetTestPreset(name, oldPresetDto))
                {
                    response.Value = true;
                }
            }
            else
            {
                response.Value = "Invalid Preset object.";
                response.StatusCode = 400;
            }
            return response;
        }
    }
}
