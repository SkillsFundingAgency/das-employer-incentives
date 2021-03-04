using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public interface IPaymentsQueryRepository
    {
        Task<List<PayableLegalEntityDto>> GetPayableLegalEntities(short collectionPeriodYear, byte collectionPeriodNumber);
        Task<List<ClawbackLegalEntityDto>> GetClawbackLegalEntities(short collectionPeriodYear, byte collectionPeriodNumber, bool isSent = false);
        Task<List<PaymentDto>> GetUnpaidPayments(long accountLegalEntity);
        Task<List<PaymentDto>> GetUnpaidClawbacks(long accountLegalEntity);
    }
}
