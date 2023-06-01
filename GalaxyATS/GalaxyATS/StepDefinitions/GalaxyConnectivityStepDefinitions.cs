using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using DataGeneration.Galaxy.DBOperation;
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
        



    }
}
