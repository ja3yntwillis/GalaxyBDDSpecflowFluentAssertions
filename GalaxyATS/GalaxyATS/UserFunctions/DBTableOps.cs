using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace GalaxyATS.UserFunctions
{
    internal class DBTableOps
    {
        static SqlConnection Connection = DBConnection.getConnectionString();
        static string partnerid = "";
        public static DataTable getdatafromQuery(string Query)
        {

            SqlDataAdapter sda = new SqlDataAdapter(Query, Connection);
            DataTable dt = new DataTable();

            try
            {
                Connection.Open();
                sda.Fill(dt);
                Connection.Close();


            }
            catch (Exception e)
            {
                Assert.Fail(e.StackTrace);
            }
            return dt;
        }
    }
}
