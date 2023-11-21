using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TestRunner.Utilities;
using TestRunner.UI.ViewModels;
using System.Net.Http;
using System.Collections.Generic;

namespace TestRunner.UI.Controllers
{
    public class BrowserStackController : Controller
    {
        public IManageUserSettings UserSettingsManager { get; }

        public BrowserStackController(IManageUserSettings userSettingsManager)
        {
            UserSettingsManager = userSettingsManager;
        }

        [HttpGet, Route("BrowserStack/MobileApps")]
        public IActionResult MobileLanding()
        {
            var viewModel = new BrowserStackAppsVM();
            return View("MobileApps", viewModel);
        }

        [HttpPost, Route("BrowserStack/MobileApps"), RequestSizeLimit(1000000000), RequestFormLimits(MultipartBodyLengthLimit = 1000000000)]
        public IActionResult MobileApps(string browserStackUsername, string browserStackKey, IFormFile browserStackAppFile)
        {
            var viewModel = new BrowserStackAppsVM();
            var client = new BrowserStackClient();
            List<Utilities.DTOs.BrowserStackAppsDto> result = null;

            if (browserStackAppFile != null)
            {
                var uploadResult = client.UploadApp(browserStackUsername, browserStackKey, browserStackAppFile.OpenReadStream(), browserStackAppFile.FileName);
            }

            try
            {
                result = client.GetAppList(browserStackUsername, browserStackKey);
            }
            catch (HttpRequestException)
            {
                return Redirect("/BrowserStack/MobileApps");
            }

            viewModel.Apps = result;
            viewModel.Username = browserStackUsername;
            viewModel.Key = browserStackKey;

            return View("MobileApps", viewModel);
        }
    }
}
