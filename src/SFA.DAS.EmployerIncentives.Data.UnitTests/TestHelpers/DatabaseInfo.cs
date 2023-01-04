
namespace SFA.DAS.EmployerIncentives.Data.UnitTests.TestHelpers
{
    public class DatabaseInfo
    {
        public string ConnectionString { get; private set; }
        public string DatabaseName { get; private set; }

        public DatabaseInfo(string connectionString, string databaseName)
        {
            SetConnectionString(connectionString);
            DatabaseName = databaseName;
        }

        public DatabaseInfo(string connectionString = null) : this(connectionString, null)
        {        
        }

        public void SetConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public void SetDatabaseName(string databaseName)
        {
            DatabaseName = databaseName;
        }
    }
}
