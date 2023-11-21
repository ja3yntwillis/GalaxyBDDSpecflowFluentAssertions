using Cedar.Configuration;
using Framework.Galaxy.Dtos;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

namespace Framework.Galaxy
{
    /// <summary>
    /// Class to get debit card Pool data
    /// </summary>
    public static class DebitCardPoolConfig
    {
        private static DebitCardPoolConfigDto debitCardData;

        /// <summary>
        /// Method to get a record from debit card pool
        /// </summary>
        /// <param name="planCombination">Plan combination of the card</param>
        /// <returns>DebitCardPool object</returns>
        public static List<DebitCardPool> GetDebitCard(string planCombination)
        {
            if (debitCardData == null)
            {
                GetDebitCardPoolConfig();
            }
            var environmentName = GetEnvironmentName();
            var cardData = debitCardData.DebitCardPool.FindAll(x => x.Environment == environmentName && x.PlanCombination == planCombination);
            return cardData;
        }
        
        /// <summary>
        /// Method to fetch debit card pool data from DebitCardPool.json
        /// </summary>
        private static void GetDebitCardPoolConfig()
        {
            var configJson = File.ReadAllText(Path.Combine(TestConfiguration.ResourcePath, "DebitCardPool.json"));
            debitCardData = JsonConvert.DeserializeObject<DebitCardPoolConfigDto>(configJson);
        }

        /// <summary>
        /// Method to fetch Environment name from base url
        /// </summary>
        /// <returns>environment name</returns>
        private static string GetEnvironmentName()
        {
            var baseUrl = TestConfiguration.BaseURL;
            switch(baseUrl)
            {
                case "https://dev-mobile-ig.participantportal.com/uat":
                case "https://ent-uat.participantportal.com":
                case "EnterpriseUAT":
                case "MOBILE-ANDROID-UAT":
                case "MOBILE-IOS-UAT":
                    return "EnterpriseUat";

                case "https://dev-mobile-ig.participantportal.com/sts":
                case "https://ent-sts.participantportal.com":
                    return "EnterpriseStress";

                case "https://mobile-ig.participantportal.com/demo":
                case "https://demo.viabenefitsaccounts.com":
                case "MOBILE-ANDROID-DEMO":
                case "MOBILE-IOS-DEMO":
                    return "EnterpriseDemo";

                case "https://mobile-ig.participantportal.com/stg":
                case "https://ent-stg.participantportal.com":
                case "EnterpriseStage":
                case "MOBILE-ANDROID-ENT-STAGE":
                case "MOBILE-IOS-ENT-STAGE":
                    return "EnterpriseStage";

                default:
                    return "EnterpriseDev";    
            }
        }
    }
}
