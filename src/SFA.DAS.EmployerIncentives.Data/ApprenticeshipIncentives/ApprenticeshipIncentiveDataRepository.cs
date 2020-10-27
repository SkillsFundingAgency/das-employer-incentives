using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Map;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public class ApprenticeshipIncentiveDataRepository : IApprenticeshipIncentiveDataRepository
    {
        private readonly EmployerIncentivesDbContext _dbContext;

        public ApprenticeshipIncentiveDataRepository(EmployerIncentivesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Add(ApprenticeshipIncentiveModel apprenticeshipIncentive)
        {
            await _dbContext.AddAsync(apprenticeshipIncentive.Map());
            await _dbContext.SaveChangesAsync();
        }

        public async Task<ApprenticeshipIncentiveModel> Get(Guid id)
        {
            var apprenticeshipIncentive = await _dbContext.ApprenticeshipIncentives
                .Include(x => x.PendingPayments)
                .ThenInclude(x => x.ValidationResults)
                .FirstOrDefaultAsync(a => a.Id == id);
            return apprenticeshipIncentive?.Map(_dbContext.CollectionPeriods.AsEnumerable());
        }

        public async Task Update(ApprenticeshipIncentiveModel apprenticeshipIncentive)
        {
            var updatedIncentive = apprenticeshipIncentive.Map();

            var existingIncentive = await _dbContext.ApprenticeshipIncentives.FirstOrDefaultAsync(x => x.Id == updatedIncentive.Id);

            if (existingIncentive != null)
            {
                UpdateApprenticeshipIncentive(updatedIncentive, existingIncentive);

                await _dbContext.SaveChangesAsync();
            }
        }                

        private void UpdateApprenticeshipIncentive(ApprenticeshipIncentive updatedIncentive, ApprenticeshipIncentive existingIncentive)
        {
            _dbContext.Entry(existingIncentive).CurrentValues.SetValues(updatedIncentive);

            RemoveDeletedPendingPayments(updatedIncentive, existingIncentive);

            foreach (var pendingPayment in updatedIncentive.PendingPayments)
            {
                var existingPendingPayment = existingIncentive.PendingPayments.SingleOrDefault(p => p.Id == pendingPayment.Id);

                if (existingPendingPayment != null)
                {
                    UpdatePendingPayment(pendingPayment, existingPendingPayment);
                }
                else
                {
                    _dbContext.PendingPayments.Add(pendingPayment);
                }
            }
        }

        private void UpdatePendingPayment(PendingPayment updatedPendingPayment, PendingPayment existingPendingPayment)
        {
            _dbContext.Entry(existingPendingPayment).CurrentValues.SetValues(updatedPendingPayment);

            RemoveDeletedValidationResults(updatedPendingPayment, existingPendingPayment);

            foreach (var validationResult in updatedPendingPayment.ValidationResults)
            {
                var existingValidationResult = existingPendingPayment.ValidationResults.SingleOrDefault(v => v.Id == validationResult.Id);

                if (existingValidationResult != null)
                {
                    _dbContext.Entry(existingValidationResult).CurrentValues.SetValues(validationResult);
                }
                else
                {
                    _dbContext.PendingPaymentValidationResults.Add(validationResult);
                }
            }
        }

        private void RemoveDeletedPendingPayments(ApprenticeshipIncentive updatedIncentive, ApprenticeshipIncentive existingIncentive)
        {
            foreach (var existingPayment in existingIncentive.PendingPayments)
            {
                if (!updatedIncentive.PendingPayments.Any(c => c.Id == existingPayment.Id))
                {
                    _dbContext.PendingPayments.Remove(existingPayment);
                }
            }
        }

        private void RemoveDeletedValidationResults(PendingPayment updatedPendingPayment, PendingPayment existingPendingPayment)
        {
            foreach (var existingValidationResult in existingPendingPayment.ValidationResults)
            {
                if (!updatedPendingPayment.ValidationResults.Any(c => c.Id == existingValidationResult.Id))
                {
                    _dbContext.PendingPaymentValidationResults.Remove(existingValidationResult);
                }
            }
        }
    }
}
