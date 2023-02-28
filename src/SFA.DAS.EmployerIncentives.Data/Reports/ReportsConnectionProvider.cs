using System.Data;
using System.Data.SqlClient;

namespace SFA.DAS.EmployerIncentives.Data.Reports
{
    public class ReportsConnectionProvider : IReportsConnectionProvider
    {
        private readonly string _connectionString;

        public ReportsConnectionProvider(string connectionString)
        {
            _connectionString = connectionString;
        }
        public IDbConnection New()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
