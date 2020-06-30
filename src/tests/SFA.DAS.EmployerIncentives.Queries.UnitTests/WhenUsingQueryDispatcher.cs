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
        private IQueryDispatcher _dispatcher;

        [SetUp]
        public void Arrange()
        {
            // Arrange
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

            var host = _hostBuilder.Build();

            _dispatcher = host.Services.GetService<IQueryDispatcher>();
        }

        [Test]
        public void Then_Can_Handle_SampleQuery()
        {
            // Act 
            var result = _dispatcher.Send<SampleQuery, bool>(new SampleQuery());

            // Assert
            result.Result.Should().BeTrue();
        }
    }

    public class SampleQuery : IQuery<bool>
    {
    }

    public class SampleQueryHandler : IQueryHandler<SampleQuery, bool>
    {
        public Task<bool> Handle(SampleQuery query)
        {
            return Task.FromResult(true);
        }
    }
}