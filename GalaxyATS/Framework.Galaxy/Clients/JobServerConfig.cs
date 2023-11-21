using Cedar.Configuration;
using Framework.Galaxy.Dtos;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace Framework.Galaxy
{
    /// <summary>
    /// Class to get/set Job Server Config
    /// </summary>
    public static class JobServerConfig
    {
        private static string jobServerHost;
        private static int jobServerPort;
        private static string sudoUser;
        private static string userName;
        private static string password;
        private static string ftpOutFilePath;
        private static string mailsacApiKey;

        public static string JobServerHost
        {
            get
            {
                if (jobServerHost == null)
                {
                    GetJobServerConfig();
                }
                return jobServerHost;
            }
            set
            {
                jobServerHost = value;
            }
        }

        public static int JobServerPort
        {
            get
            {
                if (jobServerPort == 0)
                {
                    GetJobServerConfig();
                }
                return jobServerPort;
            }
            set
            {
                jobServerPort = value;
            }
        }

        public static string SudoUser
        {
            get
            {
                if (sudoUser == null)
                {
                    GetJobServerConfig();
                }
                return sudoUser;
            }
            set
            {
                sudoUser = value;
            }
        }

        public static string Username
        {
            get
            {
                if (userName == null)
                {
                    GetJobServerConfig();
                }
                return userName;
            }
            set
            {
                userName = value;
            }
        }

        public static string Password
        {
            get
            {
                if (password == null)
                {
                    GetJobServerConfig();
                }
                return password;
            }
            set
            {
                password = value;
            }
        }

        public static string FtpOutFilePath
        {
            get
            {
                if(ftpOutFilePath == null)
                {
                    GetJobServerConfig();
                }
                return ftpOutFilePath;
            }
            set
            {
                ftpOutFilePath = value;
            }
        }

        public static string MailsacApiKey
        {
            get
            {
                if (mailsacApiKey == null)
                {
                    GetJobServerConfig();
                }
                return mailsacApiKey;
            }
            set
            {
                mailsacApiKey = value;
            }
        }

        /// <summary>
        /// Method to get JobServerConfigDetails from JobServerConfig.json file or CLI arguments
        /// </summary>
        private static void GetJobServerConfig()
        {
            //check BaseURL
            string environment = TestConfiguration.BaseURL;
            if (environment.Contains("http") || environment.Contains("MOBILE"))
            {
                switch (environment)
                {
                    case "https://ent-dev.participantportal.com":
                    case "https://dev-mobile-ig.participantportal.com/dev":
                        environment = "EnterpriseDev";
                        break;
                    case "https://ent-uat.participantportal.com":
                    case "https://dev-mobile-ig.participantportal.com/uat":
                        environment = "EnterpriseUAT";
                        break;
                    case "https://ent-stg.participantportal.com":
                    case "https://mobile-ig.participantportal.com/stg":
                        environment = "EnterpriseStage";
                        break;
                    case "https://dev-mobile-ig.participantportal.com/pfx":
                    case "https://pf-uat.participantportal.com":
                        environment = "Payflex";
                        break;
                    case "https://usaa-uat.participantportal.com":
                    case "https://dev-mobile-ig.participantportal.com/usaa":
                        environment = "USAA";
                        break;
                    default:
                        environment = "EnterpriseDev";
                        break;
                }
            }

            //get config details
            var configJson = File.ReadAllText(Path.Combine(TestConfiguration.ResourcePath, "JobServerConfig.json"));
            var jobServerConfigs = JsonConvert.DeserializeObject<JobServerConfigDto>(configJson);
            var jobServerConfig = jobServerConfigs.JobServerConfig.FirstOrDefault(j => j.Name == environment);

            jobServerHost = jobServerConfig.Host;
            jobServerPort = jobServerConfig.Port;
            sudoUser = jobServerConfig.SudoUser;
            ftpOutFilePath = jobServerConfig.FtpOutFilepath;
            userName = jobServerConfig.Username ?? JobServerRunConfig.JobServerUsername;
            password = jobServerConfig.Password ?? JobServerRunConfig.JobServerPassword;
            mailsacApiKey = jobServerConfigs.MailsacApiKey ?? JobServerRunConfig.MailsacApiKey;
        }
    }
}
