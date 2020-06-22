using SFA.DAS.EmployerIncentives.Application.Commands;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities.AcceptanceTests.Hooks
{
    public class CommandHandlerWithTestHook<T> : ICommandHandler<T> where T : ICommand
    {
        private readonly ICommandHandler<T> _handler;
        private readonly CommandHandlerHooks _hooks;

        public CommandHandlerWithTestHook(
            ICommandHandler<T> handler,
            CommandHandlerHooks hooks)
        {
            _handler = handler;
            _hooks = hooks;
        }

        public async Task Handle(T command)
        {
            try
            {
                _hooks.OnHandlerStart?.Invoke(command);
                await _handler.Handle(command);
            }
            catch (Exception ex)
            {
                _hooks.OnHandlerErrored?.Invoke(ex, (command));
                throw;
            }
            finally
            {
                _hooks.OnHandlerEnd?.Invoke(command);
            }
        }
    }
}
