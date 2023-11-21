using System.Collections.Generic;
using TestRunner.Utilities.DTOs;

namespace TestRunner.Utilities
{
    public interface IManageUserSettings
    {
        List<RunSubmitDto> GetAllTestPresets();
        List<string> GetAllTestPresetNames();
        Dictionary<string, string> GetRunnerSettings();
        RunSubmitDto GetTestPreset(string presetName);
        string GetTestPresetsDirectory();
        string GetUserSettingsDirectory();
        bool SetRunnerSettings(Dictionary<string, string> runnerSettings);
        bool SetTestPreset(string presetName, RunSubmitDto testPreset);
        bool DeleteTestPreset(string presetName);
        bool CheckTestPreset(string presetName);
    }
}