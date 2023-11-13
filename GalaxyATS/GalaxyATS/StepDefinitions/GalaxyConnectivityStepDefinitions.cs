using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Security.Principal;
using DataGeneration.Galaxy.DBOperation;
using GalaxyATS.UserFunctions;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Win32.SafeHandles;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace GalaxyATS.StepDefinitions
{
    [Binding]
    public class GalaxyConnectivityStepDefinitions
    {
        static SqlConnection conn = null;
        string partnerid = "";
        string domainName = "EXTENDHEALTH";
        string userName = "AMROY";
        String password = "Nov2023@ar";
        const int LOGON32_PROVIDER_DEFAULT = 0;
        //This parameter causes LogonUser to create a primary token.   
        const int LOGON32_LOGON_INTERACTIVE = 2;

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
        int dwLogonType, int dwLogonProvider, out SafeAccessTokenHandle phToken);

        [Then(@"I have connected to database with the necessary connection details")]
        public void ThenIHaveConnectedToDatabaseWithTheNecessaryConnectionDetails()
        {
            try
            {
                conn = DBConnection.getConnectionString();
            }
           catch (Exception ex) { Assert.Fail("Failed to connect Database"); }
        }

        [Given(@"I have Connected to extendhealth domain")]
        public void GivenIHaveConnectedToExtendhealthDomain()
        {
            // Call LogonUser to obtain a handle to an access token.   
            SafeAccessTokenHandle safeAccessTokenHandle;
            bool returnValue = LogonUser(userName, domainName, password,
                LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT,
                out safeAccessTokenHandle);

            if (false == returnValue)
            {
                int ret = Marshal.GetLastWin32Error();
                Console.WriteLine("LogonUser failed with error code : {0}", ret);
                throw new System.ComponentModel.Win32Exception(ret);
            }

            Console.WriteLine("Did LogonUser Succeed? " + (returnValue ? "Yes" : "No"));
            // Check the identity.  
            Console.WriteLine("Before impersonation: " + WindowsIdentity.GetCurrent().Name);

            // Note: if you want to run as unimpersonated, pass  
            //       'SafeAccessTokenHandle.InvalidHandle' instead of variable 'safeAccessTokenHandle'  
            WindowsIdentity.RunImpersonated(
                safeAccessTokenHandle,
                // User action  
                () =>
                {
                    try
                    {
                        conn = DBConnection.getConnectionString();
                    }
                    catch (Exception ex) { Assert.Fail("Failed to connect Database"); }
                    // Check the identity.  
                    String curUser = WindowsIdentity.GetCurrent().Name;
                    Console.WriteLine("During impersonation: " + WindowsIdentity.GetCurrent().Name);
                }
                );

            // Check the identity again.  
            Console.WriteLine("After impersonation: " + WindowsIdentity.GetCurrent().Name);
        }




    }
}
