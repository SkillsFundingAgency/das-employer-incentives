using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using Microsoft.SqlServer.Dac;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.TestHelpers
{
    public class SqlDatabase : IDisposable
    {
        private readonly string _databasePackageLocation;
        private bool isDisposed;

        public DatabaseInfo DatabaseInfo { get; private set; }

        public SqlDatabase()
        {
            DatabaseInfo = new DatabaseInfo();
            DatabaseInfo.SetPackageLocation(Path.Combine(Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("src")), @"src\SFA.DAS.EmployerIncentives.Database\bin\Debug\SFA.DAS.EmployerIncentives.Database.dacpac"));
            
            CreateTestDatabase();
        }
        
        private void CreateTestDatabase()
        {
            DatabaseInfo.SetDatabaseName(Guid.NewGuid().ToString());
            DatabaseInfo.SetConnectionString($"Data Source=.;Initial Catalog={DatabaseInfo.DatabaseName};Integrated Security=True;Pooling=False;Connect Timeout=30");

            Publish();

            using (var dbConnection = new SqlConnection(DatabaseInfo.ConnectionString))
            {
                dbConnection.Open();
                dbConnection.Close();
            }
        }

        private void DeleteTestDatabase()
        {
            try
            {
                var files = new List<string>();
                using (var dbConn = new SqlConnection(DatabaseInfo.ConnectionString))
                {
                    using (var cmd = new SqlCommand($"SELECT DB_NAME()", dbConn))
                    {
                        dbConn.Open();
                        var dbName = cmd.ExecuteScalar();
                        cmd.CommandText = $"SELECT filename FROM sysfiles";
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                files.Add((string)reader["filename"]);
                            }
                        }
                        cmd.CommandText = $"ALTER DATABASE [{dbName}] SET OFFLINE";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = $"EXEC sp_detach_db '{dbName}', 'true';";
                        cmd.ExecuteNonQuery();
                        dbConn.Close();
                    }
                }
                files.ForEach(DeleteFile);
            }
#pragma warning disable S108 // Nested blocks of code should not be left empty
            catch { }
#pragma warning restore S108 // Nested blocks of code should not be left empty
        }
        private static void DeleteFile(string file)
        {
            try
            {
                File.Delete(file);
            }
#pragma warning disable S108 // Nested blocks of code should not be left empty
            catch { }
#pragma warning restore S108 // Nested blocks of code should not be left empty
        }
        private void Publish()
        {
            var dbPackage = DacPackage.Load(DatabaseInfo.PackageLocation);
            var services = new DacServices(DatabaseInfo.ConnectionString);
            services.Deploy(dbPackage, DatabaseInfo.DatabaseName);
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing)
            {
                DeleteTestDatabase();
            }

            isDisposed = true;
        }
    }
}
