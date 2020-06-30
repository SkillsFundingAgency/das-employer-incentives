using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace SFA.DAS.EmployerIncentives.Queries.UnitTests
{
    public class WhenUsingQueryDispatcher
    {
        private IHostBuilder _hostBuilder;

        [SetUp]
        public void Arrange()
        {
            _hostBuilder = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddScoped<IQueryDispatcher, QueryDispatcher>();
                    var commandHandlers = Assembly.GetExecutingAssembly().GetTypes()
                        .Where(t => t.GetInterfaces().Any(i =>
                            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)));

                    foreach (var handler in commandHandlers)
                    {
                        services.AddScoped(
                            handler.GetInterfaces().First(i =>
                                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)), handler);
                    }

                });
        }

        [Test]
        public void QueryDispatcher_CanHandle_SampleQuery()
        {
            var host = _hostBuilder.Build();

            var dispatcher = host.Services.GetService<IQueryDispatcher>();

            dispatcher.Should().NotBeNull();
            dispatcher.Should().BeOfType(typeof(QueryDispatcher));

            var result = dispatcher.SendAsync<SampleQuery, bool>(new SampleQuery());

            result.Should().NotBeNull();
            result.Result.Should().BeTrue();
        }
    }
    public class SampleQuery : IQuery<bool>
    {
    }

    public class SampleQueryHandler : IQueryHandler<SampleQuery, bool>
    {
        public Task<bool> HandleAsync(SampleQuery query)
        {
            return Task.FromResult(true);
        }
    }
}