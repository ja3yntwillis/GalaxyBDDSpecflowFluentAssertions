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

    }
}
