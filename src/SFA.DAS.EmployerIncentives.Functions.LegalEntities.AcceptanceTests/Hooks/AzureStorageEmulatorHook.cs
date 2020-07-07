using System;
using System.Diagnostics;
using System.IO;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities.AcceptanceTests.Hooks
{
    [Binding]
    public class AzureStorageEmulatorHook
    {
        [BeforeTestRun]
        public static void StartAzureStorageEmulator()
        {
            var azureStorageEmulatorExe =
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory + @"..\..\..\..\start_azure_storage_emulator.cmd");
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = azureStorageEmulatorExe,
                    Arguments = "start",
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };

            process.Start();
        }
    }
}