using Ductus.FluentDocker.Services;
using Ductus.FluentDocker.Services.Extensions;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.TestHelpers.Types;
using System.Data;
using System.Data.SqlClient;

namespace SFA.DAS.EmployerIncentives.TestHelpers.Services
{
    public class SqlServerImage : IDisposable
    {
        const string DatabaseProjectName = "SFA.DAS.EmployerIncentives.Database";
        public const string DockerProjectLocation = "SFA.DAS.EmployerIncentives.TestHelpers";
        public const string DockerImageName = "ei-database/test";

        public SqlServerImageInfo SqlServerImageInfo { get; private set; }

        private IContainerService _dockerContainer;
        private string _dacpacFileLocation = "";
        private string _dockerFileLocation = "";
        private bool isDisposed;

        private SqlServerImage()
        {
            SetDacpacLocation();
            EnsureDockerIsRunning();
            SetDockerFileLocation();
            BuildSourceImage();
            StartContainer();
        }

        public static async Task<SqlServerImage> Create()
        {
            var sqlImage = new SqlServerImage();
            await sqlImage.WaitForServerStartUp();
            return sqlImage;
        }        
    
        private void SetDacpacLocation()
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
                throw new FileNotFoundException($"DACPAC file not found in: {_dacpacFileLocation}.  Rebuid the database project.");
        }

        private static void EnsureDockerIsRunning()
        {
            var hosts = new Hosts().Discover();
            var _docker = hosts.FirstOrDefault(x => x.IsNative) ?? hosts.FirstOrDefault(x => x.Name == "default");

            if (_docker == null)
            {
                throw new ApplicationException("Ensure Docker is running");
            }
        }

        private void SetDockerFileLocation()
        {
            _dockerFileLocation = Path.Combine(
                Directory.GetCurrentDirectory().Substring(0,
                    Directory.GetCurrentDirectory().IndexOf("src", StringComparison.Ordinal)),
                $"src\\{DockerProjectLocation}\\Images\\Dockerfile");

            if (!File.Exists(_dockerFileLocation))
                throw new FileNotFoundException($"Dockerfile not found in: {_dockerFileLocation}. ");
        }

        private void BuildSourceImage()
        {
            var workingDirectory = Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), Path.GetRandomFileName()));

            File.Copy(_dacpacFileLocation, Path.Combine(workingDirectory.FullName, "SFA.DAS.EmployerIncentives.Database.dacpac"));

            var image = new Ductus.FluentDocker.Builders.Builder()
             .DefineImage(DockerImageName)
             .FromFile(_dockerFileLocation)
             .WorkingFolder(workingDirectory.FullName)
             .Build();
            image.Start();
            image.Stop();
        }

        private void StartContainer()
        {
            _dockerContainer =
                new Ductus.FluentDocker.Builders.Builder().UseContainer()
                .UseImage(DockerImageName)
                .WithName($"EITEST_{Guid.NewGuid()}")
                .ExposePort(1433)
                .Build()
                .Start();

            _dockerContainer.StopOnDispose = true;
            _dockerContainer.RemoveOnDispose = true;
                        
            _dockerContainer.State.Should().Be(ServiceRunningState.Running);

            var endpoint = _dockerContainer.WaitForPort("1433/tcp", 30000).ToHostExposedEndpoint("1433/tcp");

            if (endpoint == null) throw new Exception("Unable to get host port for SqlDataSource");

            SqlServerImageInfo = new SqlServerImageInfo("localhost", endpoint.Port);
        }

        private async Task WaitForServerStartUp()
        {
            var timeout = new TimeSpan(0, 0, 60);
            var delayTask = Task.Delay(timeout);
            await Task.WhenAny(IsServerReady(), delayTask);

            if (delayTask.IsCompleted)
            {
                throw new Exception($"Failed to start test db server within {timeout.Seconds} seconds. ");
            }
        }

        private Task IsServerReady()
        {
            bool isReady = false;

            do
            {
                using var dbConn = new SqlConnection(@$"Data Source={SqlServerImageInfo.DataSource};Initial Catalog=master;User ID=sa;Password=Pa55word!;MultipleActiveResultSets=true;Integrated Security=False");
                try
                {
                    dbConn.Open();

                    isReady = true;
                }
                catch (Exception)
                {
                    Task.Delay(2000);
                }
                finally
                {
                    if (dbConn.State == ConnectionState.Open)
                    {
                        dbConn.Close();
                    }

                    dbConn.Dispose();
                }
            } while (!isReady);


            return Task.CompletedTask;
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
                _dockerContainer?.Dispose();
            }

            isDisposed = true;
        }
    }
}
