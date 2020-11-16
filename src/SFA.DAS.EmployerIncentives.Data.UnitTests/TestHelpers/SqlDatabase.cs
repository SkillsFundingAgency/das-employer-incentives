using Microsoft.SqlServer.Dac;
using Polly;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.TestHelpers
{
    public class SqlDatabase : IDisposable
    {
        private bool _isDisposed;

        public DatabaseInfo DatabaseInfo { get; } = new DatabaseInfo();

        public SqlDatabase()
        {
#if DEBUG
            const string environment = "debug";
#else
            const string environment = "release";
#endif

            var dacpacFileLocation =
                Path.Combine(
                    Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("src", StringComparison.Ordinal)),
                    $"src\\SFA.DAS.EmployerIncentives.Database\\bin\\{environment}\\SFA.DAS.EmployerIncentives.Database.dacpac");

            if (!File.Exists(dacpacFileLocation))
                throw new FileNotFoundException($"⚠ DACPAC File not found in: {dacpacFileLocation}");

            DatabaseInfo.SetPackageLocation(dacpacFileLocation);

            CreateTestDatabase();
        }

        private void CreateTestDatabase()
        {
            DatabaseInfo.SetDatabaseName(Guid.NewGuid().ToString());
            DatabaseInfo.SetConnectionString(@$"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog={DatabaseInfo.DatabaseName};Integrated Security=True;Pooling=False;Connect Timeout=30");

            Publish();

            using var dbConnection = new SqlConnection(DatabaseInfo.ConnectionString);
            dbConnection.Open();
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
            catch { /*ignored*/ }
        }

        private static void DeleteFile(string file)
        {
            try
            {
                File.Delete(file);
            }
            catch { /*ignored*/ }
        }

        private void Publish()
        {
            var dbPackage = DacPackage.Load(DatabaseInfo.PackageLocation);
            var services = new DacServices(DatabaseInfo.ConnectionString);

            var policy = Policy
                .Handle<DacServicesException>()
                .WaitAndRetry(new[]
                {
                    // ℹ Tweak these time-outs if you're still getting errors 👇
                    TimeSpan.FromMilliseconds(250),
                    TimeSpan.FromMilliseconds(500),
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(4),
                });

            policy.Execute(() => services.Deploy(dbPackage, DatabaseInfo.DatabaseName));
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
