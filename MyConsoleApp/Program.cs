using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Security;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace MyConsoleApp
{
    class Program
    {
        private const string AadInstance = "https://login.windows.net/{0}";
        private const string ResourceId = "https://database.windows.net/";

        static void Main(string[] args)
        {
            try
            {
                string clientId = ConfigurationManager.AppSettings["ClientId"];
                string aadTenantId = ConfigurationManager.AppSettings["Tenant"];

                AuthenticationContext authenticationContext = new AuthenticationContext(string.Format(AadInstance, aadTenantId));

                AuthenticationResult authenticationResult = authenticationContext.AcquireTokenAsync(ResourceId,
                                                                                                    clientId,
                                                                                                    GetUserCredential()).Result;

                var sqlConnectionString = ConfigurationManager.ConnectionStrings["MyDatabase"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(sqlConnectionString))
                {
                    conn.AccessToken = authenticationResult.AccessToken;
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("SELECT SUSER_SNAME()", conn))
                    {
                        var result = cmd.ExecuteScalar();
                        Console.WriteLine(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static UserCredential GetUserCredential()
        {
            string pwd = ConfigurationManager.AppSettings["UserPassword"];
            string userId = ConfigurationManager.AppSettings["UserId"];

            SecureString securePassword = new SecureString();

            foreach (char c in pwd) { securePassword.AppendChar(c); }
            securePassword.MakeReadOnly();

            var userCredential = new UserPasswordCredential(userId, securePassword);

            return userCredential;
        }
    }
}
