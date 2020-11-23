﻿using Microsoft.SqlServer.Dac;
using Polly;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.TestHelpers
{
    public static class SqlDatabaseModel
    {
        public const string ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True";
        public const string DatabaseProjectName = "SFA.DAS.EmployerIncentives.Database";
        private static string _dacpacFileLocation;

        public static void Update()
        {
            SetDacpacLocation();

            var modelNeedsUpdating = false;
            try
            {
                modelNeedsUpdating = DacpacFileHasBeenModified();
                Console.WriteLine($"[{nameof(SqlDatabaseModel)}] {nameof(Update)}: ModelNeedsUpdating={modelNeedsUpdating}");
            }
            catch (Exception ex)
            {
                modelNeedsUpdating = true;
                Console.WriteLine($"[{nameof(SqlDatabaseModel)}] {nameof(Update)}: Exception={ex.Message}");
            }

            if (!modelNeedsUpdating) return;

            PublishModel();
            SaveModifiedDateTime();
        }

        private static bool DacpacFileHasBeenModified()
        {
            var current = File.GetLastWriteTime(_dacpacFileLocation);
            var previous = GetSavedModifiedDateTime();

            return (current - previous).Seconds != 0; // a way to compare 2 dates while ignoring milliseconds
        }

        private static DateTime GetSavedModifiedDateTime()
        {
            var stored = File.ReadAllText(_dacpacFileLocation + ".tmp");

            return DateTime.TryParse(stored, out var result) ? result : DateTime.MinValue;
        }

        private static void SaveModifiedDateTime()
        {
            try
            {
                var current = File.GetLastWriteTime(_dacpacFileLocation);
                File.WriteAllText(_dacpacFileLocation + ".tmp", current.ToString(CultureInfo.CurrentCulture));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{nameof(SqlDatabaseModel)}] {nameof(SaveModifiedDateTime)}: Exception={ex.Message}");
            }
        }

        private static void SetDacpacLocation()
        {
#if DEBUG
            const string environment = "debug";
#else
            const string environment = "release";
#endif
            _dacpacFileLocation = Path.Combine(
                Directory.GetCurrentDirectory().Substring(0,
                    Directory.GetCurrentDirectory().IndexOf("src", StringComparison.Ordinal)),
                $"src\\{DatabaseProjectName}\\bin\\{environment}\\{DatabaseProjectName}.dacpac");

            if (!File.Exists(_dacpacFileLocation))
                throw new FileNotFoundException($"DACPAC file not found in: {_dacpacFileLocation}");
        }

        private static void PublishModel()
        {
            var dbPackage = DacPackage.Load(_dacpacFileLocation);
            var services = new DacServices(ConnectionString);

            var policy = Policy
                .Handle<DacServicesException>()
                .WaitAndRetry(Enumerable.Repeat(TimeSpan.FromMilliseconds(250), 40));

            policy.Execute(() =>
            {
                Console.WriteLine($"[{nameof(SqlDatabaseModel)}] {nameof(PublishModel)} attempted");
                services.Deploy(dbPackage, "model", upgradeExisting: true);
            });
        }
    }
}