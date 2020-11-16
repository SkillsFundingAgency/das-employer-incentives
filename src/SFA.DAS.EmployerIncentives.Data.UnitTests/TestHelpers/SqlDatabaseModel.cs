using Microsoft.SqlServer.Dac;
using Polly;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Xml;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.TestHelpers
{
    public static class SqlDatabaseModel
    {
        public const string ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=model;Integrated Security=True";

        public static void Update()
        {
            var modelNeedsUpdating = false;
            try
            {
                var operationKeys = FetchRefactorLogRecords();
                modelNeedsUpdating = CheckModelNeedsUpdating(operationKeys);
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("Invalid object name 'dbo.__RefactorLog'"))
                    modelNeedsUpdating = true;
            }

            if (modelNeedsUpdating) PublishModel();
        }

        private static bool CheckModelNeedsUpdating(HashSet<Guid> operationKeys)
        {
            var refactorlog =
                Path.Combine(
                    Directory.GetCurrentDirectory()
                        .Substring(0, Directory.GetCurrentDirectory().IndexOf("src", StringComparison.Ordinal)),
                    "src\\SFA.DAS.EmployerIncentives.Database\\SFA.DAS.EmployerIncentives.Database.refactorlog");
            var document = new XmlDocument();
            document.Load(refactorlog);
            var nodes = document.GetElementsByTagName("Operation");
            var modelNeedsUpdating = false;
            // ReSharper disable once PossibleNullReferenceException
            for (var i = 0; i < nodes.Count; i++)
            {
                if (operationKeys.Contains(Guid.Parse(nodes[i].Attributes["Key"].Value))) continue;
                modelNeedsUpdating = true;
                break;
            }

            return modelNeedsUpdating;
        }

        private static HashSet<Guid> FetchRefactorLogRecords()
        {
            var operationKeys = new HashSet<Guid>();
            using var dbConn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand("SELECT * FROM [dbo].[__RefactorLog]", dbConn);
            dbConn.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                operationKeys.Add(Guid.Parse(reader[0].ToString()!));
            }

            dbConn.Close();
            return operationKeys;

        }

        private static void PublishModel()
        {
#if DEBUG
            const string environment = "debug";
#else
            const string environment = "release";
#endif
            var dacpacFileLocation =
                Path.Combine(
                    Directory.GetCurrentDirectory().Substring(0,
                        Directory.GetCurrentDirectory().IndexOf("src", StringComparison.Ordinal)),
                    $"src\\SFA.DAS.EmployerIncentives.Database\\bin\\{environment}\\SFA.DAS.EmployerIncentives.Database.dacpac");

            if (!File.Exists(dacpacFileLocation))
                throw new FileNotFoundException($"⚠ DACPAC File not found in: {dacpacFileLocation}");

            var dbPackage = DacPackage.Load(dacpacFileLocation);
            var services = new DacServices(ConnectionString);

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

            policy.Execute(() =>
            {
                Console.WriteLine($"[{nameof(SqlDatabaseModel)}] {nameof(PublishModel)} attempted");
                services.Deploy(dbPackage, "model", upgradeExisting: true);
            });
        }
    }
}
