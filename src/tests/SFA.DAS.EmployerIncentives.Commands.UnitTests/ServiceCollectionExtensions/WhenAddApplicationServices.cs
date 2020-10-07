using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands;
using SFA.DAS.EmployerIncentives.Commands.Decorators;
using SFA.DAS.EmployerIncentives.Commands.RemoveLegalEntity;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Events;
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
                    c.AddDbContext<EmployerIncentivesDbContext>();
                });
        }

        [Test]
        public void Then_the_AddLegalEntityCommand_handler_is_configured()
        {
            //Act
            var host = _hostBuilder.ConfigureServices(c => c.AddCommandServices().AddPersistenceServices().AddEventServices()).Build();

            //Assert
            HandlerShouldBeSetUp<RemoveLegalEntityCommand, RemoveLegalEntityCommandHandler>(host);
        }

        //[Test]
        //public void Then_the_RemoveLegalEntityCommand_handler_is_configured()
        //{
        //    //Act
        //    var host = _hostBuilder.ConfigureServices(c => c.AddCommandServices().AddPersistenceServices().AddEventServices()).Build();

        //    //Assert
        //    HandlerShouldBeSetUp<AddLegalEntityCommand, AddLegalEntityCommandHandler>(host);
        //}

        private void HandlerShouldBeSetUp<TCommand, IHandler>(IHost host) where TCommand : ICommand where IHandler : ICommandHandler<TCommand>
        {
            var outerHandler = host.Services.GetService<ICommandHandler<TCommand>>();

            outerHandler.Should().NotBeNull();
            outerHandler.Should().BeOfType(typeof(CommandHandlerWithLogging<TCommand>));

            var validatorHandler = GetChildHandler<CommandHandlerWithValidator<TCommand>, TCommand>(outerHandler);
            validatorHandler.Should().NotBeNull();

            var retryHandler = GetChildHandler<CommandHandlerWithRetry<TCommand>, TCommand>(validatorHandler);
            retryHandler.Should().NotBeNull();

            var lockHandler = GetChildHandler<CommandHandlerWithDistributedLock<TCommand>, TCommand>(retryHandler);
            lockHandler.Should().NotBeNull();

            var handler = GetChildHandler<IHandler, TCommand>(lockHandler);
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
