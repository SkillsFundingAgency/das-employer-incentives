using SFA.DAS.EmployerIncentives.Data.UnitTests.TestHelpers;
using SFA.DAS.EmployerIncentives.TestHelpers.Types;
using System.Data;
using System.Data.SqlClient;

namespace SFA.DAS.EmployerIncentives.TestHelpers.Services
{
    public class SqlDatabase : ISqlDatabase
    {
        public DatabaseInfo DatabaseInfo { get; private set; }

        private readonly string _databaseName;
        private readonly SqlServerImageInfo _sqlServerImageInfo;
        private bool _isDisposed;

        public SqlDatabase(SqlServerImageInfo sqlServerImageInfo, string databaseName)
        {
            _sqlServerImageInfo = sqlServerImageInfo;
            _databaseName = databaseName;

            DatabaseInfo = CreateTestDatabase(sqlServerImageInfo.DataSource, databaseName);
        }

        private DatabaseInfo CreateTestDatabase(string dataSource, string databaseName)
        {
            var dataBaseInfo = new DatabaseInfo(
                @$"Data Source={dataSource};Initial Catalog={databaseName};User Id=sa;Password=Pa55word!;MultipleActiveResultSets=True;Pooling=False;Connect Timeout=30;Integrated Security=False",
                _databaseName);

            var connectionString = @$"Data Source={dataSource};Initial Catalog=master;User Id=sa;Password=Pa55word!;MultipleActiveResultSets=true;Pooling=False;Connect Timeout=60;Integrated Security=False";

            using var dbConn = new SqlConnection(connectionString);
            try
            {
                var sql = @$"CREATE DATABASE [{dataBaseInfo.DatabaseName}]";
                using var cmd = new SqlCommand(sql, dbConn);

                dbConn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"CREATE DATABASE Error for connection {connectionString}", ex);
            }
            finally
            {
                if (dbConn.State == ConnectionState.Open)
                {
                    dbConn.Close();
                }

                dbConn.Dispose();
            }

            return dataBaseInfo;
        }

        private void DeleteDatabase()
        {
            try
            {
                var files = new List<string>();
                using var dbConn = new SqlConnection(DatabaseInfo.ConnectionString);
                using var cmd = new SqlCommand("SELECT DB_NAME()", dbConn);
                dbConn.Open();
                var dbName = cmd.ExecuteScalar();
                cmd.CommandText = "SELECT filename FROM sysfiles";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        files.Add((string)reader["filename"]);
                    }
                }
                cmd.CommandText = $"ALTER DATABASE [{dbName}] SET OFFLINE WITH ROLLBACK IMMEDIATE";
                cmd.ExecuteNonQuery();
                cmd.CommandText = $"EXEC sp_detach_db '{dbName}', 'true';";
                cmd.ExecuteNonQuery();
                dbConn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{nameof(SqlDatabase)}] {nameof(DeleteDatabase)} exception thrown: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                DeleteDatabase();
            }

            _isDisposed = true;
        }
    }
}
