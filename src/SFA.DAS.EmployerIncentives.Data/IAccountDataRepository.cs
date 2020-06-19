using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data
{
    public interface IAccountDataRepository
    {
        Task Update(IAccountModel account);
        Task Add(IAccountModel account);
        Task<IAccountModel> Find(long accountId);
    }
}
