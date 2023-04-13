using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Hosting;
using SFA.DAS.EmployerIncentives.Functions.TestHelpers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Services
{
    public class PurgeBackgroundJob : BackgroundService
    {
        private readonly IJobHost _jobHost;
        public PurgeBackgroundJob(IJobHost jobHost)
        {
            _jobHost = jobHost;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _jobHost.Purge();
                await Task.Delay(TimeSpan.FromSeconds(10));
            }

            await _jobHost.Terminate();
        }
    }
}
