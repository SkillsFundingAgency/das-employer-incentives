using SFA.DAS.EmployerIncentives.Infrastructure.SqlAzureIdentityAuthentication;
using System.Data.SqlClient;

namespace SFA.DAS.EmployerIncentives.Data
{
    public class SqlConnectionProvider : ISqlConnectionProvider
    {   
        private readonly ISqlAzureIdentityTokenProvider _sqlAzureIdentityTokenProvider;
        
        public SqlConnectionProvider(ISqlAzureIdentityTokenProvider sqlAzureIdentityTokenProvider)
        {
            _sqlAzureIdentityTokenProvider = sqlAzureIdentityTokenProvider;
        }
        public SqlConnection Get(string connectionString)
        {
            if (IsIntegratedSecurity(connectionString))
            {
                return new SqlConnection(connectionString);
            }
            else
            {
                return new SqlConnection(connectionString)
                {
                    AccessToken = _sqlAzureIdentityTokenProvider.GetAccessToken()
                };
            }
        }

        private bool IsIntegratedSecurity(string connectionString)
        {
            var conn = connectionString.Replace(" ", "").ToLower();
            return conn.Contains("integratedsecurity=true") || conn.Contains("integratedsecurity=sspi");
        }
    }
}
