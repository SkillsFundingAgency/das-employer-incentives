using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.EmployerIncentives.Queries.Account;
using SFA.DAS.EmployerIncentives.Queries.Decorators;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SFA.DAS.EmployerIncentives.Queries.Account.GetLegalEntities;

namespace SFA.DAS.EmployerIncentives.Queries.UnitTests.ServiceCollectionExtensions
{
    public class WhenAddQueryServices
    {
        private IHostBuilder _hostBuilder;

        [SetUp]
        public void Arrange()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {"PolicySettings:RetryPolicies:QueryRetryAttempts", "1"},
                {"PolicySettings:RetryPolicies:QueryRetryWaitInMilliSeconds", "100"},
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _hostBuilder = new HostBuilder()
                .ConfigureServices(c =>
                {
                    c.AddOptions();
                    c.Configure<PolicySettings>(configuration.GetSection("PolicySettings"));
                    c.AddDbContext<ApplicationDbContext>();
                });
        }

        [Test]
        public void Then_the_GetLegalEntitiesQueryHandler_handler_is_configured()
        {
            // Act
            var host = _hostBuilder.ConfigureServices(c => c.AddQueryServices()).Build();

            // Assert
            HandlerShouldBeSetUp<GetLegalEntitiesRequest, GetLegalEntitiesQueryHandler, GetLegalEntitiesResponse>(host);
        }

        private static void HandlerShouldBeSetUp<TQuery, THandler, TResult>(IHost host) where TQuery : IQuery where THandler : IQueryHandler<TQuery, TResult>
        {
            var outerHandler = host.Services.GetService<IQueryHandler<TQuery, TResult>>();

            outerHandler.Should().NotBeNull();
            outerHandler.Should().BeOfType(typeof(QueryHandlerWithLogging<TQuery, TResult>));

            var retryHandler = GetChildHandler<QueryHandlerWithRetry<TQuery, TResult>, TQuery, TResult>(outerHandler);
            retryHandler.Should().NotBeNull();

            var handler = GetChildHandler<THandler, TQuery, TResult>(retryHandler);
            handler.Should().NotBeNull();
        }

        private static THandler GetChildHandler<THandler, TQuery, TResult>(IQueryHandler<TQuery, TResult> parent) where TQuery : IQuery
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.NonPublic;

            var properties = parent.GetType().GetFields(flags);

            var temp = properties.FirstOrDefault(p => p.FieldType == typeof(IQueryHandler<TQuery, TResult>));
            return (THandler)temp?.GetValue(parent);
        }
    }
}