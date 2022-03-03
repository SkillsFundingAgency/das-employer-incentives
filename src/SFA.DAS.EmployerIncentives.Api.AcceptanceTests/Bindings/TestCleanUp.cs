using System;
using System.IO;
using System.Threading;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    public class TestCleanUp
    {
        [BeforeScenario(Order = 1)]
        public void StartUp(TestContext context)
        {
            context.CancellationTokenSource = new CancellationTokenSource();
            context.CancellationToken = context.CancellationTokenSource.Token;
        }

        [AfterScenario(Order = 1)]
        public void Cancel(TestContext context)
        {
            context.CancellationTokenSource.Cancel();
        }

        [AfterScenario(Order = 100)]
        public void CleanUp(TestContext context)
        {
            try
            {
                DeleteDirectory(context.TestDirectory.FullName);
            }
            catch(Exception){}
        }

        private static void DeleteDirectory(string directory)
        {
            string[] files = Directory.GetFiles(directory);
            string[] directories = Directory.GetDirectories(directory);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in directories)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(directory, false);
        }
    }
}