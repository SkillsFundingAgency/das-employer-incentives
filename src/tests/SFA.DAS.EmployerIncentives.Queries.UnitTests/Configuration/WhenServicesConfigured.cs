using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Queries.UnitTests.Configuration
{
    public class WhenServicesConfigured
    {
        [Test]
        public void ThenRetryPolicyIsApplied()
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string>
            {
                {"PolicySettings:RetryPolicies:QueryRetryAttempts", "1"},
                {"PolicySettings:RetryPolicies:QueryRetryWaitInMilliSeconds", "100"},
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var services = new ServiceCollection();
            services.AddQueryServices();
            services.Configure<PolicySettings>(configuration.GetSection("PolicySettings"));

            // Act
            var polly = services.BuildServiceProvider().GetService<Policies>();

            // Assert
            polly.Should().NotBeNull();
            polly.QueryRetryPolicy.Should().NotBeNull();
        }
    }
}