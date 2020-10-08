using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public class PaymentLegalEntityQueryRepository : IPaymentLegalEntityQueryRepository
    {
        private readonly EmployerIncentivesDbContext _context;

        public PaymentLegalEntityQueryRepository(EmployerIncentivesDbContext context)
        {
            _context = context;
        }

        public Task<List<PayableLegalEntityDto>> GetList(int collectionPeriodYear, int collectionPeriodMonth)
        {
            var accountLegalEntities = _context.Set<PendingPayment>().Where(x => !x.PaymentMadeDate.HasValue).Distinct() //TODO: Remaining criteria
                .Select(x => x.AccountId).Distinct(); //TODO: Changed to account legal entity id

            return accountLegalEntities.Select(x => new PayableLegalEntityDto {AccountLegalEntityId = x}).ToListAsync();
        }
    }
}
