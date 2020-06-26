using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Application.Commands;
using SFA.DAS.EmployerIncentives.Application.Commands.AddLegalEntity;
using SFA.DAS.EmployerIncentives.Application.Commands.RemoveLegalEntity;
using SFA.DAS.EmployerIncentives.Application.Decorators;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using System.Linq;
using System.Reflection;

namespace SFA.DAS.EmployerIncentives.Application.UnitTests.ServiceCollectionExtensionTests
{
    public class WhenAddApplicationServices
    {
        private IHostBuilder _hostBuilder;

        [SetUp]
        public void Arrange()
        {
            _hostBuilder = new HostBuilder()
                .ConfigureServices(c =>
                {
                    c.AddOptions();
                    c.Configure<ApplicationSettings>(o =>
                    {
                        o.DistributedLockStorage = "UseDevelopmentStorage=true";
                    });
                });
        }

        [Test]
        public void Then_the_AddLegalEntityCommand_handler_is_configured()
        {
            //Act
            var host = _hostBuilder.ConfigureServices(c => c.AddApplicationServices()).Build();

            //Assert
            var loggingHandler = host.Services.GetService<ICommandHandler<AddLegalEntityCommand>>();
            loggingHandler.Should().NotBeNull();
            loggingHandler.Should().BeOfType(typeof(CommandHandlerWithLogging<AddLegalEntityCommand>));

            var validatorHandler = GetChildHandler<CommandHandlerWithValidator<AddLegalEntityCommand>, AddLegalEntityCommand>(loggingHandler);
            validatorHandler.Should().NotBeNull();
            
            var retryHandler = GetChildHandler<CommandHandlerWithRetry<AddLegalEntityCommand>, AddLegalEntityCommand>(validatorHandler);
            retryHandler.Should().NotBeNull();

            var lockHandler = GetChildHandler<CommandHandlerWithDistributedLock<AddLegalEntityCommand>, AddLegalEntityCommand>(retryHandler);
            lockHandler.Should().NotBeNull();

            var handler = GetChildHandler<AddLegalEntityCommandHandler, AddLegalEntityCommand>(lockHandler);
            handler.Should().NotBeNull();
        }

        [Test]
        public void Then_the_RemoveLegalEntityCommand_handler_is_configured()
        {
            //Act
            var host = _hostBuilder.ConfigureServices(c => c.AddApplicationServices()).Build();

            //Assert
            var loggingHandler = host.Services.GetService<ICommandHandler<RemoveLegalEntityCommand>>();
            loggingHandler.Should().NotBeNull();
            loggingHandler.Should().BeOfType(typeof(CommandHandlerWithLogging<RemoveLegalEntityCommand>));

            var validatorHandler = GetChildHandler<CommandHandlerWithValidator<RemoveLegalEntityCommand>, RemoveLegalEntityCommand>(loggingHandler);
            validatorHandler.Should().NotBeNull();

            var retryHandler = GetChildHandler<CommandHandlerWithRetry<RemoveLegalEntityCommand>, RemoveLegalEntityCommand>(validatorHandler);
            retryHandler.Should().NotBeNull();

            var lockHandler = GetChildHandler<CommandHandlerWithDistributedLock<RemoveLegalEntityCommand>, RemoveLegalEntityCommand>(retryHandler);
            lockHandler.Should().NotBeNull();

            var handler = GetChildHandler<RemoveLegalEntityCommandHandler, RemoveLegalEntityCommand>(lockHandler);
            handler.Should().NotBeNull();
        }

        private static T GetChildHandler<T, T2>(ICommandHandler<T2> parent) where T2 : ICommand
        {
            var flags = BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.NonPublic;

            var properties = parent.GetType().GetFields(flags);

            var temp = properties.FirstOrDefault(p => p.FieldType.Equals(typeof(ICommandHandler<T2>)));
            return (T)temp.GetValue(parent);
        }
    }
}
