using Cedar.Configuration;
using System.Collections.Generic;

namespace Framework.Galaxy.Clients
{
    public class VmClient : GenericSshClient
    {
        /// <summary>
        /// Creates the command to trigger test run on vm and triggers connect method
        /// </summary>
        /// <param name="vmRunData">VM run username, password, url</param>
        /// <param name="testList">vm run test list</param>
        /// <param name="threadCount">No of tests to be run parallely</param>
        /// <param name="retries">retry count</param>
        /// <param name="browserstackData">browserstack credentials</param>
        public void Execute(Dictionary<string, string> vmRunData, string[] testList, int threadCount, int retries, Dictionary<string, string> browserstackData)
        {
            //comma separated list of tests to run
            string tests = string.Join(',', testList);

            string command = null;
            string vmUrl = null;
            string script = null;
            //set VM Box
            if (!vmRunData["vmUrl"].Equals("Select"))
            {
                vmUrl = SetVmUrl(vmRunData["vmUrl"]);
            }

            string label = $"By{vmRunData["vmUsername"]}On{vmUrl.Split('.')[0].Substring(9)}";

            //select script to run
            switch (vmRunData["vmBranch"].ToLower())
            {
                case "develop":
                    script = "testRun.sh";
                    break;
                default:
                    script = $"branchRun.sh {vmRunData["vmBranch"]}";
                    break;
            }

            //set the vm command
            switch (TestConfiguration.SuiteType)
            {
                case "MOBILE":
                    command = $"bash {script} -s {TestConfiguration.SuiteType} -u {TestConfiguration.BaseURL} -t {tests} -c {threadCount} -r {retries} " +
                            $"-bsu {browserstackData["bsUsername"]} -bsk {browserstackData["bsKey"]} -bsau {browserstackData["bsAppUrl"]} -bsmos {browserstackData["bsMobOsVer"]} -bsmdv {browserstackData["bsMobDevice"]} " +
                            $"-dbn {TestConfiguration.DatabaseName} -dbu {TestConfiguration.DbUserName} -dbp {TestConfiguration.DBPassword} -mak {JobServerConfig.MailsacApiKey} " + 
                            $"-grid {GraphApiRunConfig.ClientId} -grk {GraphApiRunConfig.ClientSecret} -grt {GraphApiRunConfig.TenantName} -ls {label}";
                    break;
                case "UI":
                case "NUT":
                case "DB":
                    command = $"bash {script} -s {TestConfiguration.SuiteType} -u {TestConfiguration.BaseURL} -t {tests} -c {threadCount} -r {retries} " +
                            $"-b {TestConfiguration.Browser} -asmb Tests.Acclaris -app {TestConfiguration.Application} -dbn {TestConfiguration.DatabaseName} " +
                            $"-dbu {TestConfiguration.DbUserName} -dbp {TestConfiguration.DBPassword} -jsu {JobServerConfig.Username} -jsp {JobServerConfig.Password} " + 
                            $"-mak {JobServerConfig.MailsacApiKey} -grid {GraphApiRunConfig.ClientId} -grk {GraphApiRunConfig.ClientSecret} -grt {GraphApiRunConfig.TenantName} -ls {label}";
                    break;
            }

            Connect(vmUrl, vmRunData["vmUsername"], vmRunData["vmPassword"], new List<string> { command });
        }

        /// <summary>
        /// Sets up VM for first time users by checking out code from bitbucket
        /// </summary>
        /// <param name="username">bitbucket username</param>
        /// <param name="key">bitbucket access key</param>
        /// <param name="vmUsername">VM username</param>
        /// <param name="vmPassword">VM password</param>
        /// <param name="vmUrl">VM url</param>
        public void Setup(string username, string key, string vmUsername, string vmPassword, string vmUrl)
        {
            string path = "/u01/shared";
            string command = string.Empty;

            switch (vmUrl)
            {
                case "AutomationTeam_Box1":
                    command = $"bash {path}/remoteSetup.sh {SetVmUrl(vmUrl)} {username} {key}";
                    Connect(SetVmUrl("AutomationTeam_Box2"), vmUsername, vmPassword, new List<string> { command });
                    break;
                case "AutomationTeam_Box2":
                    command = $"bash {path}/setup.sh {username} {key}";
                    Connect(SetVmUrl("AutomationTeam_Box2"), vmUsername, vmPassword, new List<string> { command });
                    break;
                case "Sajdevcoltst01_Box":
                    command = $"bash {path}/setup.sh {username} {key}";
                    Connect(SetVmUrl("Sajdevcoltst01_Box"), vmUsername, vmPassword, new List<string> { command });
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Returns VM url based on vm name
        /// <param name="vmName">Virtual box name</param>
        /// <returns>vm url</returns>
        private string SetVmUrl(string vmName)
        {
            string vmUrl = string.Empty;
            switch (vmName)
            {
                case "AutomationTeam_Box1":
                    vmUrl = "loudevcolaut51.acclariscorp.com";
                    break;
                case "AutomationTeam_Box2":
                    vmUrl = "loudevcoltst01.acclariscorp.com";
                    break;
                case "Sajdevcoltst01_Box":
                    vmUrl = "sajdevcoltst01.acclariscorp.com";
                    break;
            }
            return vmUrl;
        }
    }
}