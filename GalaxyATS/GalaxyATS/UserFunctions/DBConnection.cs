using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalaxyATS.UserFunctions
{
    internal class DBConnection
    {
        public static SqlConnection getConnectionString()
        {
            var databaseserver = "devsqlag.extendhealth.com";
            var user = "INTERNAL/AMIYA4963";
            var database = "DevEHGalaxy"; 
            String CS = String.Format("Data Source=" +databaseserver+ ";Initial Catalog=" + database + ";Integrated Security=SSPI;uid=" +user+ ";");
           return new SqlConnection(CS);

        }
    }
}
