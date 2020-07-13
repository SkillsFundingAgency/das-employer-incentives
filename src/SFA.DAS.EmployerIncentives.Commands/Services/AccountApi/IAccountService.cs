using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Services.AccountApi
{
    public interface IAccountService
    {
        Task<PagedModel<AccountLegalEntity>> GetAccountLegalEntitiesByPage(int pageNumber, int pageSize = 1000);
        Task<GetLegalEntityResponse> GetLegalEntity(long accountId, long legalentityId);
    }
}
