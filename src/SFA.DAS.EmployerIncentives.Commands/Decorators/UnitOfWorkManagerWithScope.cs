using SFA.DAS.UnitOfWork.Managers;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Decorators
{
    public class UnitOfWorkManagerWithScope : IUnitOfWorkManager
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private int counter = 0;
        private bool errorHandled = false;
        public UnitOfWorkManagerWithScope(IUnitOfWorkManager unitOfWorkManager)
        {
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task BeginAsync()
        {
            if (counter == 0)
            {
                await _unitOfWorkManager.BeginAsync();
            }
            counter++;
        }

        public async Task EndAsync(Exception ex = null)
        {
            if (errorHandled) return;

            if (ex != null)
            {
                await _unitOfWorkManager.EndAsync(ex);
                errorHandled = true;
            }
            else
            {
                counter--;
                if (counter == 0)
                {
                    await _unitOfWorkManager.EndAsync();
                }
            }
        }
    }
}
