using SFA.DAS.UnitOfWork.Managers;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests
{
    public class TestUnitOfWorkManager : IUnitOfWorkManager
    {
        public Task BeginAsync()
        {
            return Task.CompletedTask;
        }

        public Task EndAsync(Exception ex = null)
        {
            return Task.CompletedTask;
        }
    }
}
