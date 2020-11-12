using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.AzureFunctions
{
    public class FunctionsController
    {
        private const int StartupTimeoutSeconds = 20;
        private static Process _functionHostProcess;
        private readonly List<FunctionOutputBufferHandler> _output = new List<FunctionOutputBufferHandler>();
        private readonly object _sync = new object();

        public async Task StartFunctionsHost(int port, string functionsHostPath, string functionsBinFolder)
        {
            Console.WriteLine($"Starting a function instance for project {functionsBinFolder} on port {port}");
            Console.WriteLine("\tStarting process");

            var bufferHandler =
                StartFunctionHostProcess(
                    port,
                    "csharp",
                    functionsHostPath,
                    functionsBinFolder);

            lock (_sync)
            {
                _output.Add(bufferHandler);
            }

            Console.WriteLine("\tProcess started; waiting for initialisation to complete");

            await Task.WhenAny(
                bufferHandler.JobHostStarted,
                bufferHandler.ExitCode,
                Task.Delay(TimeSpan.FromSeconds(StartupTimeoutSeconds))).ConfigureAwait(false);

            if (bufferHandler.ExitCode.IsCompleted)
            {
                var exitCode = await bufferHandler.ExitCode.ConfigureAwait(false);
                throw new Exception($"Function host process terminated unexpectedly with exit code {exitCode}. \r\n {bufferHandler.StandardErrorText}");
            }

            if (!bufferHandler.JobHostStarted.IsCompleted)
            {
                throw new Exception("Timed out while starting functions instance.");
            }

            Console.WriteLine();
            Console.WriteLine("\tStarted");
        }

        private static FunctionOutputBufferHandler StartFunctionHostProcess(int port, string provider, string functionsHostPath, string functionsBinFolder)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = functionsHostPath,
                Arguments = $"host start --port {port} --{provider} --pause-on-error",
                WorkingDirectory = functionsBinFolder,
                UseShellExecute = true,

                //CreateNoWindow = false,
                //RedirectStandardError = true,
                //RedirectStandardOutput = true,
                //RedirectStandardInput = true,
            };

            var processHandler = new FunctionOutputBufferHandler(startInfo);
            _functionHostProcess = processHandler.Process;
            processHandler.Start();

            return processHandler;
        }

        public void Dispose()
        {
            lock (this._sync)
            {
                WriteAllToConsoleAndClear(_output);
            }

            if (!_functionHostProcess.HasExited)
            {
                _functionHostProcess.Kill(true);
            }

            _functionHostProcess.CloseMainWindow();
            _functionHostProcess.Dispose();
        }

        public static void WriteAllToConsoleAndClear(IEnumerable<IProcessOutput> outputs)
        {
            foreach (var output in outputs)
            {
                Console.WriteLine(output.StandardOutputText);
                Console.WriteLine(output.StandardErrorText);
            }
        }

    }
}