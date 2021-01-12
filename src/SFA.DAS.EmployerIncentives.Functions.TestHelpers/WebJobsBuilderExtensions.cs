using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SFA.DAS.EmployerIncentives.Functions.TestHelpers
{
    public static class WebJobsBuilderExtensions
    {
        public static IWebJobsBuilder ConfigureServices(this IWebJobsBuilder builder, Action<IServiceCollection> configure)
        {
            configure(builder.Services);
            return builder;
        }
    }
}
