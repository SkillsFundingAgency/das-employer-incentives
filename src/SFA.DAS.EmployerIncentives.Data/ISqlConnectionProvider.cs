using System.Data.SqlClient;

namespace SFA.DAS.EmployerIncentives.Data
{
    public interface ISqlConnectionProvider
    {
        public SqlConnection Get(string connectionString);
    }
}
