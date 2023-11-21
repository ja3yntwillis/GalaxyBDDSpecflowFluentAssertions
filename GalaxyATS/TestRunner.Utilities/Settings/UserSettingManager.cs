using System.Text.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TestRunner.Utilities.DTOs;

namespace TestRunner.Utilities
{
    public class UserSettingManager : IManageUserSettings
    {
        public string GetUserSettingsDirectory()
        {
            var result = string.Format("{0}{1}{2}", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Path.DirectorySeparatorChar, "LZTestRunner");
            
            if (!Directory.Exists(result))
            {
                Directory.CreateDirectory(result);
            }
            return result;
        }

        public Dictionary<string, string> GetRunnerSettings()
        {
            var jsonFilePath = string.Format("{0}{1}{2}", GetUserSettingsDirectory(), Path.DirectorySeparatorChar, "runnersettings.json");
            var result = new Dictionary<string, string>();
            if (File.Exists(jsonFilePath))
            {
                result = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(jsonFilePath));
            }
            return result;
        }

        public bool SetRunnerSettings(Dictionary<string, string> runnerSettings)
        {
            var result = true;
            var jsonFilePath = string.Format("{0}{1}{2}", GetUserSettingsDirectory(), Path.DirectorySeparatorChar, "runnersettings.json");
            var jsonContent = JsonSerializer.Serialize(runnerSettings);
            File.WriteAllText(jsonFilePath, jsonContent);
            return result;
        }

        public string GetTestPresetsDirectory()
        {
            var result = string.Format("{0}{1}{2}", GetUserSettingsDirectory(), Path.DirectorySeparatorChar, "TestPresets");
            if (!Directory.Exists(result))
            {
                Directory.CreateDirectory(result);
            }
            return result;
        }

        public List<RunSubmitDto> GetAllTestPresets()
        {
            var jsonFilesPath = Directory.GetFiles(GetTestPresetsDirectory(), "*.json", SearchOption.AllDirectories);
            var result = new List<RunSubmitDto>();
            foreach (var jsonFilePath in jsonFilesPath)
            {
                var runSubmitDto = JsonSerializer.Deserialize<RunSubmitDto>(File.ReadAllText(jsonFilePath));
                result.Add(runSubmitDto);
            }
            return result;
        }

        public List<string> GetAllTestPresetNames()
        {
            var result = Directory.GetFiles(GetTestPresetsDirectory(), "*.json", SearchOption.AllDirectories).Select(x => Path.GetFileNameWithoutExtension(x)).ToList<string>();
            return result;
        }

        public RunSubmitDto GetTestPreset(string presetName)
        {
            var jsonFilePath = string.Format("{0}{1}{2}{3}", GetTestPresetsDirectory(), Path.DirectorySeparatorChar, presetName, ".json");
            var result = new RunSubmitDto();
            if (File.Exists(jsonFilePath))
            {
                result = JsonSerializer.Deserialize<RunSubmitDto>(File.ReadAllText(jsonFilePath));
                if (result.Attribute != null && result.Attributes == null)
                {
                    result.Attributes = new List<string>();
                    result.Attributes.Add(result.Attribute);
                }
            }
            return result;
        }

        public bool SetTestPreset(string presetName, RunSubmitDto testPreset)
        {
            var result = true;
            var jsonFilePath = string.Format("{0}{1}{2}{3}", GetTestPresetsDirectory(), Path.DirectorySeparatorChar, presetName, ".json");
            var jsonContent = JsonSerializer.Serialize(testPreset);
            File.WriteAllText(jsonFilePath, jsonContent);
            return result;
        }

        public bool DeleteTestPreset(string presetName)
        {
            var jsonFilePath = string.Format("{0}{1}{2}{3}", GetTestPresetsDirectory(), Path.DirectorySeparatorChar, presetName, ".json");
            var result = false;
            if (File.Exists(jsonFilePath))
            {
                File.Delete(jsonFilePath);
                result = true;
            }
            return result;
        }

        public bool CheckTestPreset(string presetName)
        {
            var jsonFilePath = string.Format("{0}{1}{2}{3}", GetTestPresetsDirectory(), Path.DirectorySeparatorChar, presetName, ".json");
            var result = false;
            if (File.Exists(jsonFilePath))
            {
                result = true;
            }
            return result;
        }
    }
}
