using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public interface IApprenticeshipIncentiveArchiveRepository
    {
        Task Archive(PendingPaymentModel pendingPaymentModel);
        Task Archive(PaymentModel paymentModel);
    }
}