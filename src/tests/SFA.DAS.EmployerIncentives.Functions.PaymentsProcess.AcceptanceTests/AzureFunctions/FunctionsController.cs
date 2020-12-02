using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Services;
using System;
using System.Diagnostics;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.AzureFunctions
{
    public class FunctionsController
    {
        private static Process _functionHostProcess;

        public void StartFunctionsHost(int port, TestSettings settings)
        {
            Console.WriteLine($"Starting a function instance for project {settings.FunctionApplicationPath} on port {port}");
            Console.WriteLine("\tStarting process");

            StartFunctionHostProcess(
                    port,
                    "csharp",
                    settings);

            Console.WriteLine();
            Console.WriteLine("\tStarted");
        }

        private static void StartFunctionHostProcess(int port, string provider, TestSettings settings)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = settings.FunctionHostPath,
                Arguments = $"start --port {port} --{provider}",
                WorkingDirectory = settings.FunctionApplicationPath,
                UseShellExecute = true
            };

            _functionHostProcess = new Process { StartInfo = startInfo };
            var success = _functionHostProcess.Start();

            if (!success) throw new InvalidOperationException("Failed to start Azure Functions host");
        }

        public void Dispose()
        {
            if (_functionHostProcess == null) return;

            if (!_functionHostProcess.HasExited)
            {
                _functionHostProcess.Kill(true);
            }

            _functionHostProcess.CloseMainWindow();
            _functionHostProcess.Dispose();
        }

    }
}