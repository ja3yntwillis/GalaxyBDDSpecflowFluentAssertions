using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using GalaxyATS.UserFunctions;
using Google.Protobuf.WellKnownTypes;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace GalaxyATS.StepDefinitions
{
    [Binding]
    public class GalaxyConnectivityStepDefinitions
    {
        static SqlConnection conn = null;
        string partnerid = "";

         [Given(@"I have connected to database with the necessary connection details")]
        public void GivenIHaveConnectedToDatabaseWithTheNecessaryConnectionDetails()
        {
            try
            {
                conn = DBConnection.getConnectionString();
            }
           catch (Exception ex) { Assert.Fail("Failed to connect Database"); }
        }
        [Then(@"I check the existance of the ""([^""]*)"" in the ""([^""]*)"" table")]
       
        public void ThenICheckTheExistanceOfThePartnerAtTheTable(string value, string tablename,Table tab)
        {
            var searchparam = "partnerId";
            var query = "select " + searchparam + " from " + tablename+" where partnerName='"+value+"'";
            DataTable dt=DBTableOps.getdatafromQuery(query);
            partnerid = tab.Rows[0]["partnerid"];
            string expected_partnerID = dt.Rows[0][searchparam].ToString();
            expected_partnerID.Should().Be(partnerid);
            ScenarioContext.Current["partnerId"] = expected_partnerID;


        }
        [When(@"I find the partner is existing in the system")]
        public void WhenIFindThePartnerIsExistingInTheSystem()
        {
            Console.WriteLine("PartnerID is : "+ScenarioContext.Current["partnerId"]);
        }
        [Then(@"I validate the following campaigns are available for the partner")]
        public void ThenIValidateTheFollowingCampaignsAreAvailableForThePartner(Table table)
        {
            var query = "select campaignId,campaignName from dbo.campaign where partnerid="+ ScenarioContext.Current["partnerId"];
            DataTable dt = DBTableOps.getdatafromQuery(query);
            for(int i=0; i<dt.Rows.Count; i++)
            {
                var actual_campaignid = dt.Rows[i]["campaignId"].ToString();
                var expected_campaignId=table.Rows[i]["campaignId"].ToString();
                actual_campaignid.Should().Be(expected_campaignId);
                var actual_campaignname = dt.Rows[i]["campaignName"].ToString(); ;
                var expected_campaignname = table.Rows[i]["campaignName"].ToString(); ;
                actual_campaignname.Should().Be(expected_campaignname);

            }
        }


    }
}
