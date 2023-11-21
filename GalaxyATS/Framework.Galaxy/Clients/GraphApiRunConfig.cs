using System.IO;
using System.Linq;
using Cedar.Configuration;
using Framework.Galaxy.Dtos;
using Newtonsoft.Json;

namespace Framework.Galaxy
{
    /// <summary>
    /// Class to get/set Graph API Configuration
    /// </summary>
    public static class GraphApiRunConfig
    {
        private static string clientId;
        private static string clientSecret;
        private static string tenantName;

        public static string ClientId
        {
            get
            {
                if (clientId == null)
                {
                    GetGraphApiRunConfig();
                }
                return clientId;
            }
            set
            {
                clientId = value;
            }
        }

        public static string ClientSecret
        {
            get
            {
                if (clientSecret == null)
                {
                    GetGraphApiRunConfig();
                }
                return clientSecret;
            }
            set
            {
                clientSecret = value;
            }
        }

        public static string TenantName
        {
            get
            {
                if (tenantName == null)
                {
                    GetGraphApiRunConfig();
                }
                return tenantName;
            }
            set
            {
                tenantName = value;
            }
        }

        /// <summary>
        /// Method to read the details from GraphApiConfig.json based on the execution environment and configure the related parameters
        /// </summary>
        private static void GetGraphApiRunConfig()
        {
            string environment = TestConfiguration.BaseURL;
            if (environment.Contains("http") || environment.Contains("MOBILE"))
            {
                switch (environment)
                {
                    case "https://ent-dev.participantportal.com":
                    case "https://dev-mobile-ig.participantportal.com/dev":
                    case string env when env.Contains("DEV"):
                        environment = "EnterpriseDev";
                        break;
                    case "https://ent-uat.participantportal.com":
                    case "https://dev-mobile-ig.participantportal.com/uat":
                    case string env when env.Contains("UAT"):
                        environment = "EnterpriseUAT";
                        break;
                    case "https://ent-stg.participantportal.com":
                    case "https://mobile-ig.participantportal.com/stg":
                    case "https://eus-stg.viabenefitsaccounts.com":
                    case "https://mobile-ig.participantportal.com/eus":
                    case string env when env.Contains("STAGE"):
                        environment = "EnterpriseStage";
                        break;
                    case "https://ent-sts.participantportal.com":
                    case "https://dev-mobile-ig.participantportal.com/sts":
                        environment = "EnterpriseStress";
                        break;
                    case "https://pf-uat.participantportal.com":
                        environment = "Payflex";
                        break;
                    default:
                        environment = "EnterpriseDev";
                        break;
                }
            }

            var configJson = File.ReadAllText(Path.Combine(TestConfiguration.ResourcePath, "GraphApiConfig.json"));
            var graphApiConfigs = JsonConvert.DeserializeObject<GraphApiConfigDto>(configJson);
            var graphApiConfig = graphApiConfigs.GraphApiConfig.FirstOrDefault(env => env.Name == environment);

            clientId = graphApiConfig.ClientId;
            clientSecret = graphApiConfig.ClientSecret;
            tenantName = graphApiConfig.Tenant;
        }
    }
}
