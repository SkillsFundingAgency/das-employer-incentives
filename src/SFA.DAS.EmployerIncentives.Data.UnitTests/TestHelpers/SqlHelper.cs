using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.TestHelpers
{
    public static class SqlHelper
    {
        public class DatabaseProperties
        {
            public string ConnectionString { get; }

            public DatabaseProperties(string connectionString)
            {
                ConnectionString = connectionString;
            }
        }

        public static DatabaseProperties CreateTestDatabase()
        {
            FileInfo dbFile;
            string connectionString;
            string dbName;

            dbName = Guid.NewGuid().ToString();
            dbFile = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), $"{dbName}.mdf"));
            File.Copy(Path.Combine(Directory.GetCurrentDirectory(), "SFA.DAS.EmployerIncentives.mdf"), dbFile.ToString());
            connectionString = $"AttachDbFilename={Path.Combine(Directory.GetCurrentDirectory(), $"{dbName}.mdf")};Trusted_Connection=Yes;";

            using (var dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                dbConnection.Close();
            }

            return new DatabaseProperties(connectionString);
        }

        public static void DeleteTestDatabase(DatabaseProperties databaseProperties)
        {
            try
            {
                var files = new List<string>();
                using (var dbConn = new SqlConnection(databaseProperties.ConnectionString))
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
    }
}
