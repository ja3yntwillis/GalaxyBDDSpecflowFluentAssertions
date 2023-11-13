using System.Data;
using System.Runtime.InteropServices;
using System.Security.Principal;
using DataGeneration.Galaxy.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Win32.SafeHandles;
using Dbo = DataGeneration.Galaxy.Models.DboDBModels;
using FundingCacheClient = DataGeneration.Galaxy.Models.FundingCacheClientDBModels;

namespace DataGeneration.Galaxy
{
    public class WindownImpersonationExtendHelath
    {
        //#pragma warning disable CS0414 // The field 'WindownImpersonationExtendHelath.conn' is assigned but its value is never used        
        static string domainName = "EXTENDHEALTH";
        static string userName = "";
        static string password = "";
        const int LOGON32_PROVIDER_DEFAULT = 0;
        //This parameter causes LogonUser to create a primary token.   
        const int LOGON32_LOGON_INTERACTIVE = 2;

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
        int dwLogonType, int dwLogonProvider, out SafeAccessTokenHandle phToken);

        public static void Main(String[] args)
        {
            // Call LogonUser to obtain a handle to an access token.   
            SafeAccessTokenHandle safeAccessTokenHandle;
            bool returnValue = LogonUser(userName, domainName, password,
                LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT,
                out safeAccessTokenHandle);

            if (!returnValue)
            {
                int ret = Marshal.GetLastWin32Error();
                Console.WriteLine("LogonUser failed with error code : {0}", ret);
                throw new System.ComponentModel.Win32Exception(ret);
            }

            Console.WriteLine("Did LogonUser Succeed? " + (returnValue ? "Yes" : "No"));
            // Check the identity.  
            Console.WriteLine("Before impersonation: " + WindowsIdentity.GetCurrent().Name);
            //var userToken = safeAccessTokenHandle;
            //Win32Api.GetSessionUserToken(ref userToken);
            SqlParameter personId = new SqlParameter("personId", 12);
            SqlParameter fundingSourceId = new SqlParameter("fundingSourceId", 12);

            //WindowsIdentity impersonatedIdentity = new WindowsIdentity(safeAccessTokenHandle.DangerousGetHandle());

            // Note: if you want to run as unimpersonated, pass  
            //       'SafeAccessTokenHandle.InvalidHandle' instead of variable 'safeAccessTokenHandle'  
            WindowsIdentity.RunImpersonated(
                safeAccessTokenHandle,
                () =>
                {
                    var partnerName = ThenICheckTheExistanceOfTheInThePartnerTableBelow("Ford Motor Company");
                    //ExecuteRefreshFrozenAccount();
                    InsertDataInPartnerTable();
                    Console.WriteLine("Partner name: " + partnerName);
                    Console.WriteLine("During impersonation: " + WindowsIdentity.GetCurrent().Name);
                }
                );           

            // Check the identity again.  
            Console.WriteLine("After impersonation: " + WindowsIdentity.GetCurrent().Name);

        }

        public static string ThenICheckTheExistanceOfTheInThePartnerTableBelow(string PartnerName)
        {
            string partnerName = "";
            using (var dbContext = new DevEHGalaxyContext())
            {
                partnerName = partnerName + dbContext.Partners.Where(x => x.PARTNERID.Equals(1)).First().LEGALENTITYNAME;
                partnerName = partnerName + dbContext.DboPartners.Where(x => x.PARTNERID.Equals(5)).First().PARTNERNAME;
            }
            return partnerName;
        }

        public static void InsertDataInPartnerTable()
        {
            FundingCacheClient.Partner partner = new FundingCacheClient.Partner();
            partner.LEGALENTITYNAME = "AutomationTest";
            partner.ORGANIZATIONNAME = "AutomationTest";
            partner.PARTNERGUID = Guid.NewGuid();
            using (var dbContext = new DevEHGalaxyContext())
            {
                dbContext.Partners.Add(partner);
                dbContext.SaveChanges();
            }
        }

        public static void ModifyDataInPartnerTable()
        {                        
            using (var dbContext = new DevEHGalaxyContext())
            {
                var partner = dbContext.DboPartners.FirstOrDefault(x => x.PARTNERNAME == "AutomationTest");
                partner.LASTMODIFIEDDATE = DateTime.Now;
                dbContext.Update(partner).Property(x => x.SFDCACCOUNTID).IsModified=false;
                dbContext.SaveChanges();
            }
        }

        public static void DeleteDataInPartnerTable()
        {
            using (var dbContext = new DevEHGalaxyContext())
            {
                var partner = dbContext.DboPartners.FirstOrDefault(x => x.PARTNERNAME == "AutomationTest");
                if (!(partner==null))
                {
                    dbContext.Remove(partner);
                    dbContext.SaveChanges();
                }                
            }
        }

        public static object ExecuteRefreshFrozenAccount(params SqlParameter[] parameters)
        {
            object outParameterValue = null;
            using (SqlConnection connection = new SqlConnection("Server=qasqlag.extendhealth.com;Database=QAExtendHealth;Trusted_Connection=True;"))
            {
                SqlCommand command = new SqlCommand("FundingCache_Client.hsp_RefreshPartner", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                if (parameters.Length > 0)
                {
                    foreach (var parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }
                }

                command.Connection.Open();
                command.ExecuteNonQuery();

                if (command.Parameters.Contains("outParameter"))
                {
                    outParameterValue = command.Parameters["outParameter"].Value;
                }

                command.Connection.Close();
            }
            return outParameterValue;
        }
    }
}
