using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using DataGeneration.Galaxy.Models;
using DataGeneration.Galaxy.Models.FundingCacheClientDBModels;
using GalaxyATS.UserFunctions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Org.BouncyCastle.Asn1.Cms;
using TechTalk.SpecFlow;

namespace GalaxyATS.StepDefinitions
{
    [Binding]
    public class GalaxyStepDefinitions
    {
        SqlConnection Connection= DBConnection.getConnectionString();  // It is for SQL connection
        static SqlConnection conn = null;
        string partnerid = "";
        [Then(@"I select ""([^""]*)"" from ""([^""]*)"" table")]
        public void ThenISelectFromTable(string columnname, string tablename)
        {
            var query = "select " + columnname + " from " + tablename;
            SqlDataAdapter sda = new SqlDataAdapter(query, Connection);

            DataTable dt = new DataTable();

            try
            {
                Connection.Open();
                sda.Fill(dt);
                Connection.Close();
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        Console.Write(dt.Columns[i].ColumnName + " ");
                        Console.WriteLine(dt.Rows[j].ItemArray[i]);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

        }
        [Then(@"I validate there are no campaignsegments for the campaign ""([^""]*)""")]
        public void ThenIValidateThereAreNoCampaignsegmentsForTheCampaign(string campaignName)
        {
            IDictionary<string, int> campaigns = (IDictionary<string, int>)ScenarioContext.Current["campaignDict"];
            var currentCampaign = campaigns[campaignName];
            List<CampaignSegment> campaignData;
            using (var dbContext = new DevEHGalaxyContext())
            {
                campaignData = dbContext.Campaignsegments.Where(x => x.campaignId.Equals(currentCampaign)).ToList();
            }
            Console.WriteLine(currentCampaign.ToString());
            campaignData.Count.Should().Be(0);
        }
  
        [Then(@"I check the existance of the ""([^""]*)"" in the Partner table below")]
        public int ThenICheckTheExistanceOfTheInThePartnerTableBelow(string PartnerName, Table table)
        {
            int partner_Id;
            using (var dbContext = new DevEHGalaxyContext())
            {
                partner_Id = dbContext.Partners.Where(x => x.LEGALENTITYNAME.Equals(PartnerName)).First().PARTNERID;

            }
            //partner_Id.Should().Be(Int32.Parse(table.Rows[0]["partnerid"]));
            //ScenarioContext.Current["partnerId"] = table.Rows[0]["partnerid"];
            return partner_Id;
        }


        [When(@"I find the partner is existing in the system")]
        public void WhenIFindThePartnerIsExistingInTheSystem()
        {
            Console.WriteLine("PartnerID is : " + ScenarioContext.Current["partnerId"]);
        }
        [Then(@"I validate the following campaigns are available for the partner")]
        public void ThenIValidateTheFollowingCampaignsAreAvailableForThePartner(Table table)
        {
            IDictionary<string, int> campaigns = new Dictionary<string, int>();
            List<Campaign> campaignData;
            using (var dbContext = new DevEHGalaxyContext())
            {
                campaignData = dbContext.Campaigns.Where(x => x.partnerId.Equals(ScenarioContext.Current["partnerId"])).ToList();
            }
            for(int i = 0; i < campaignData.Count; i++)
            {
                var actual_campaignid = campaignData[i].campaignId;
                var expected_campaignId = Int32.Parse(table.Rows[i]["campaignId"].ToString());
                expected_campaignId.Should().Be(actual_campaignid);
                var actual_campaignname = campaignData[i].campaignName.ToString(); ;
                var expected_campaignname = table.Rows[i]["campaignName"].ToString(); ;
                expected_campaignname.Should().Be(actual_campaignname);
                campaigns.Add(actual_campaignname, expected_campaignId);
                ScenarioContext.Current["campaignDict"] = campaigns;
            }
                       
        }
        [Then(@"I validate the following campaignsegments of the campaign ""([^""]*)""")]
        public void ThenIValidateTheFollowingCampaignsegmentsOfTheCampaign(string campaignName, Table table)
        {
            IDictionary<string, int> campaigns = (IDictionary<string, int>)ScenarioContext.Current["campaignDict"];
            var currentCampaign = campaigns[campaignName];
            Console.WriteLine(currentCampaign.ToString());
           
            List<CampaignSegment> campaignSegmentData;
            using (var dbContext = new DevEHGalaxyContext())
            {
                campaignSegmentData = dbContext.Campaignsegments.Where(x => x.campaignId.Equals(currentCampaign)).ToList();
            }
            
            for (int i = 0; i < campaignSegmentData.Count; i++)
            {
                var actual_campaignName = campaignSegmentData[i].campaignSegmentName.ToString();
                var expected_campaignName = table.Rows[i]["CampaignSegmentName"].ToString();
                expected_campaignName.Should().Be(actual_campaignName);

            }

        }

    }
}
