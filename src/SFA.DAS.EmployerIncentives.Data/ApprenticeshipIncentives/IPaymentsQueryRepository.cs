using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public interface IPaymentsQueryRepository
    {
        Task<List<PayableLegalEntity>> GetPayableLegalEntities(short collectionPeriodYear, byte collectionPeriodNumber);
        Task<List<ClawbackLegalEntity>> GetClawbackLegalEntities(short collectionPeriodYear, byte collectionPeriodNumber, bool isSent = false);
        Task<List<Payment>> GetUnpaidPayments(long accountLegalEntity);
        Task<List<Payment>> GetUnpaidClawbacks(long accountLegalEntity);
    }
}
