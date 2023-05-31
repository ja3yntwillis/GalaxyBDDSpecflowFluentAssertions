using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using GalaxyATS.UserFunctions;
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
            IDictionary<string, string> campaigns = (IDictionary<string, string>)ScenarioContext.Current["campaignDict"];
            var currentCampaign = campaigns[campaignName];
            Console.WriteLine(currentCampaign.ToString());
            var query = "select campaignSegmentName from dbo.campaignsegment where campaignid = " + campaigns[campaignName];
            DataTable dt = DBTableOps.getdatafromQuery(query);
            dt.Rows.Count.Should().Be(0);
        }
        [Then(@"I check the existance of the ""([^""]*)"" in the ""([^""]*)"" table")]

        public void ThenICheckTheExistanceOfThePartnerAtTheTable(string value, string tablename, Table tab)
        {
            var searchparam = "partnerId";
            var query = "select " + searchparam + " from " + tablename + " where partnerName='" + value + "'";
            DataTable dt = DBTableOps.getdatafromQuery(query);
            partnerid = tab.Rows[0]["partnerid"];
            string expected_partnerID = dt.Rows[0][searchparam].ToString();
            expected_partnerID.Should().Be(partnerid);
            ScenarioContext.Current["partnerId"] = expected_partnerID;


        }
        [When(@"I find the partner is existing in the system")]
        public void WhenIFindThePartnerIsExistingInTheSystem()
        {
            Console.WriteLine("PartnerID is : " + ScenarioContext.Current["partnerId"]);
        }
        [Then(@"I validate the following campaigns are available for the partner")]
        public void ThenIValidateTheFollowingCampaignsAreAvailableForThePartner(Table table)
        {
            var query = "select campaignId,campaignName from dbo.campaign where partnerid=" + ScenarioContext.Current["partnerId"];
            IDictionary<string, string> campaigns = new Dictionary<string, string>();
            DataTable dt = DBTableOps.getdatafromQuery(query);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var actual_campaignid = dt.Rows[i]["campaignId"].ToString();
                var expected_campaignId = table.Rows[i]["campaignId"].ToString();
                expected_campaignId.Should().Be(actual_campaignid);
                var actual_campaignname = dt.Rows[i]["campaignName"].ToString(); ;
                var expected_campaignname = table.Rows[i]["campaignName"].ToString(); ;
                expected_campaignname.Should().Be(actual_campaignname);
                campaigns.Add(actual_campaignname, expected_campaignId);
                ScenarioContext.Current["campaignDict"] = campaigns;

            }
        }
        [Then(@"I validate the following campaignsegments of the campaign ""([^""]*)""")]
        public void ThenIValidateTheFollowingCampaignsegmentsOfTheCampaign(string campaignName, Table table)
        {
            IDictionary<string, string> campaigns = (IDictionary<string, string>)ScenarioContext.Current["campaignDict"];
            var currentCampaign = campaigns[campaignName];
            Console.WriteLine(currentCampaign.ToString());
            var query = "select campaignSegmentName from dbo.campaignsegment where campaignid = " + campaigns[campaignName];
            DataTable dt = DBTableOps.getdatafromQuery(query);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var actual_campaignid = dt.Rows[i]["campaignSegmentName"].ToString();
                var expected_campaignId = table.Rows[i]["CampaignSegmentName"].ToString();
                expected_campaignId.Should().Be(actual_campaignid);

            }

        }

    }
}
