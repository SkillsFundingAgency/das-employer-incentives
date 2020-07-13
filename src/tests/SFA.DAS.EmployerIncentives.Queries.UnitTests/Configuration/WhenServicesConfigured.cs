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
                {"RetryPolicies:LockedRetryAttempts", "1"},
                {"RetryPolicies:LockedRetryWaitInMilliSeconds", "100"},
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var services = new ServiceCollection();
            services.AddQueryServices();
            services.Configure<RetryPolicies>(configuration.GetSection("RetryPolicies"));

            // Act
            var polly = services.BuildServiceProvider().GetService<Policies>();

            // Assert
            polly.Should().NotBeNull();
            polly.LockRetryPolicy.Should().NotBeNull();
        }
    }
}