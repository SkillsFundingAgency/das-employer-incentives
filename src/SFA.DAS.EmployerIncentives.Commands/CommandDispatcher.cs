using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace SFA.DAS.EmployerIncentives.Commands
{
    public class CommandDispatcher : ICommandDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public CommandDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Send<T>(T command, CancellationToken cancellationToken = default) where T : ICommand
        {
            var service = _serviceProvider.GetService<ICommandHandler<T>>();
            await service.Handle(command);
        }
    }
}