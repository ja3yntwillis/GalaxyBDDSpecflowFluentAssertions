using System.Collections.Generic;

namespace TestRunner.UI.ViewModels
{
    public class BrowserStackAppsVM
    {
        public string Username { get; set; }
        public string Key { get; set; }
        public List<Utilities.DTOs.BrowserStackAppsDto> Apps { get; set; }
    }
}
