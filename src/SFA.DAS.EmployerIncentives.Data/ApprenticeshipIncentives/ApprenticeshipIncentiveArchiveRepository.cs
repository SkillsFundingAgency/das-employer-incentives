using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Map;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public class ApprenticeshipIncentiveArchiveRepository : IApprenticeshipIncentiveArchiveRepository
    {
        private readonly Lazy<EmployerIncentivesDbContext> _lazyContext;
        private EmployerIncentivesDbContext _dbContext => _lazyContext.Value;

        public ApprenticeshipIncentiveArchiveRepository(Lazy<EmployerIncentivesDbContext> dbContext)
        {
            _lazyContext = dbContext;
        }

        public async Task Archive(PendingPaymentModel pendingPaymentModel)
        {
            await _dbContext.AddAsync(pendingPaymentModel.Map());
            pendingPaymentModel.PendingPaymentValidationResultModels.ToList().ForEach(async v =>
            {
                await _dbContext.AddAsync(v.ArchiveMap(pendingPaymentModel.Id));
            });
        }

        public async Task Archive(PaymentModel paymentModel)
        {
            await _dbContext.AddAsync(paymentModel.Map());
        }

        public async Task Archive(EmploymentCheckModel employmentCheckModel)
        {
            await _dbContext.AddAsync(employmentCheckModel.Map());
        }

        public async Task<PendingPaymentModel> GetArchivedPendingPayment(Guid pendingPaymentId)
        {
            var archivedPendingPayment = _dbContext.ArchivedPendingPayments.Where(x => x.PendingPaymentId == pendingPaymentId)
                .OrderByDescending(y => y.ArchiveDateUTC).FirstOrDefault();

            if (archivedPendingPayment == null)
            {
                return null;
            }

            return await Task.FromResult(archivedPendingPayment.Map());
        }
    }
}
