using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.UnitOfWork.Managers;

namespace SFA.DAS.EmployerIncentives.Commands.Decorators
{
    public class CommandHandlerWithUnitOfWork<T> : ICommandHandler<T> where T : ICommand
    {
        private readonly ICommandHandler<T> _handler;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public CommandHandlerWithUnitOfWork(
            ICommandHandler<T> handler,         
            IUnitOfWorkManager unitOfWorkManager)
        {
            _handler = handler;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task Handle(T command, CancellationToken cancellationToken = default)
        {
            await _unitOfWorkManager.BeginAsync();

            try
            {
                await _handler.Handle(command, cancellationToken);
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
