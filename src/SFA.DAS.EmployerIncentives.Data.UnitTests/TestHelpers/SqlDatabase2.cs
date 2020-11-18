using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.TestHelpers
{
    public class SqlDatabase2 : IDisposable
    {
        private bool _isDisposed;
        public DatabaseInfo DatabaseInfo { get; } = new DatabaseInfo();

        public SqlDatabase2()
        {
            CreateTestDatabase();
        }

        private void CreateTestDatabase()
        {
            DatabaseInfo.SetDatabaseName(Guid.NewGuid().ToString());
            DatabaseInfo.SetConnectionString(
                @$"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog={DatabaseInfo.DatabaseName};Integrated Security=True;Pooling=False;Connect Timeout=30");

            using var dbConn = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True");
            try
            {
                var sql = @$"CREATE DATABASE [{DatabaseInfo.DatabaseName}] ON PRIMARY
                     (NAME = [{DatabaseInfo.DatabaseName}_Data],
                      FILENAME = 'C:\\temp\\{DatabaseInfo.DatabaseName}.mdf')
                      LOG ON (NAME = [{DatabaseInfo.DatabaseName}_Log],
                      FILENAME = 'C:\\temp\\{DatabaseInfo.DatabaseName}.ldf')";
                using var cmd = new SqlCommand(sql, dbConn);
                dbConn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                if (dbConn.State == ConnectionState.Open)
                {
                    dbConn.Close();
                }
            }

            using var dbConnection = new SqlConnection(DatabaseInfo.ConnectionString);
            dbConnection.Open();
        }

        private void DeleteTestDatabase()
        {
            try
            {
                var files = new List<string>();
                using var dbConn = new SqlConnection(DatabaseInfo.ConnectionString);
                using var cmd = new SqlCommand("SELECT DB_NAME()", dbConn);
                dbConn.Open();
                var dbName = cmd.ExecuteScalar();
                cmd.CommandText = "SELECT filename FROM sysfiles";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    files.Add((string)reader["filename"]);
                }
                cmd.CommandText = $"ALTER DATABASE [{dbName}] SET OFFLINE";
                cmd.ExecuteNonQuery();
                cmd.CommandText = $"EXEC sp_detach_db '{dbName}', 'true';";
                cmd.ExecuteNonQuery();
                dbConn.Close();

                files.ForEach(DeleteFile);
            }
            catch
            {
                Console.WriteLine($"[{nameof(SqlDatabase2)}] {nameof(DeleteTestDatabase)} exception thrown");
            }
        }

        private static void DeleteFile(string file)
        {
            try
            {
                File.Delete(file);
            }
            catch
            {
                Console.WriteLine($"[{nameof(SqlDatabase2)}] {nameof(DeleteFile)} exception thrown");
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
                DeleteTestDatabase();
            }

            _isDisposed = true;
        }
    }
}
