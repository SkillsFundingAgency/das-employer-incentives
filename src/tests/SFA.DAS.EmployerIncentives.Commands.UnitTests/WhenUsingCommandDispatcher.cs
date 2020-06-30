using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests
{
    public class WhenUsingCommandDispatcher
    {
        private IHostBuilder _hostBuilder;

        [SetUp]
        public void Arrange()
        {
            _hostBuilder = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddScoped<ICommandDispatcher, CommandDispatcher>();
                    var commandHandlers = Assembly.GetExecutingAssembly().GetTypes()
                        .Where(t => t.GetInterfaces().Any(i =>
                            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)));

                    foreach (var handler in commandHandlers)
                    {
                        services.AddScoped(
                            handler.GetInterfaces().First(i =>
                                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)), handler);
                    }

                });
        }


        [Test]
        public void CommandDispatcher_Handles_SampleCommand()
        {
            var host = _hostBuilder.Build();

            var dispatcher = host.Services.GetService<ICommandDispatcher>();

            dispatcher.Should().NotBeNull();
            dispatcher.Should().BeOfType(typeof(CommandDispatcher));

            var result = dispatcher.SendAsync(new SampleCommand());

            result.Should().NotBeNull();
            result.Status.Should().Be(TaskStatus.RanToCompletion);
        }

        public class SampleCommand : ICommand
        {

        }

        public class SampleCommandHandler : ICommandHandler<SampleCommand>
        {
            public Task HandleAsync(SampleCommand command)
            {
                return Task.CompletedTask;

            }
        }

    }
}
