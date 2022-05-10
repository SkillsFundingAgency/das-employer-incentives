using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;
using SFA.DAS.UnitOfWork.Managers;

namespace SFA.DAS.EmployerIncentives.Commands
{
    public class CommandDispatcher : ICommandDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public CommandDispatcher(
            IServiceProvider serviceProvider,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _serviceProvider = serviceProvider;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public Task Send<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand
        {
            var handler = _serviceProvider.GetService<ICommandHandler<TCommand>>();

            if (handler == null)
            {
                throw new CommandDispatcherException($"Unable to dispatch command '{command.GetType().Name}'. No matching handler found.");
            }

            return handler.Handle(command, cancellationToken);
        }

        public async Task SendMany<TCommands>(TCommands commands, CancellationToken cancellationToken = default) where TCommands : IEnumerable<ICommand>
        {
            await _unitOfWorkManager.BeginAsync();

            try
            {
                foreach (var command in commands)
                {
                    if (command is ILockIdentifier)
                    {
                        throw new NotSupportedException("Not currently possible to dispatch multiple commands if any implement the ILockIdentifier interface");
                    }
                    await Send(command as dynamic, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                await _unitOfWorkManager.EndAsync(ex);

                throw;
            }

            await _unitOfWorkManager.EndAsync();
        }
    }
}