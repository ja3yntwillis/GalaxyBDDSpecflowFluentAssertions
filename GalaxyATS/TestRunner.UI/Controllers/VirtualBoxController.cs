using Microsoft.AspNetCore.Mvc;
using TestRunner.Utilities;
using TestRunner.UI.ViewModels;
using Framework.Galaxy.Clients;

namespace TestRunner.UI.Controllers
{
    public class VirtualBoxController : Controller
    {
        public IManageUserSettings UserSettingsManager { get; }

        /// <summary>
        /// VirtualBoxController constructor
        /// </summary>
        /// <param name="userSettingsManager">bitbucket username</param>
        public VirtualBoxController(IManageUserSettings userSettingsManager)
        {
            UserSettingsManager = userSettingsManager;
        }

        [HttpGet, Route("/VirtualBox/SetUp")]
        public IActionResult MobileLanding()
        {
            var viewModel = new VirtualBoxSetupVM();
            return View("Setup", viewModel);
        }

        /// <summary>
        /// Sets up VM for first time users by checking out code from bitbucket
        /// </summary>
        /// <param name="bitbucketUsername">bitbucket username</param>
        /// <param name="bitbucketKey">bitbucket access key</param>
        /// <param name="vmUsername">VM username</param>
        /// <param name="vmPassword">VM password</param>
        /// <param name="vmUrl">VM url</param>
        [HttpPost, Route("/VirtualBox/SetUp"), RequestSizeLimit(1000000000), RequestFormLimits(MultipartBodyLengthLimit = 1000000000)]
        public IActionResult Setup(string bitbucketUsername, string bitbucketKey, string vmUsername, string vmPassword, string vmUrl)
        {
            var viewModel = new VirtualBoxSetupVM();
            var client = new VmClient();

            client.Setup(bitbucketUsername, bitbucketKey, vmUsername, vmPassword, vmUrl);

            viewModel.BitbucketUsername = bitbucketUsername;
            viewModel.BitbucketKey = bitbucketKey;
            viewModel.VMUsername = vmUsername;
            viewModel.VMPassword = vmPassword;
            viewModel.VirtualBox = vmUrl;

            return View("Setup", viewModel);
        }
    }
}
