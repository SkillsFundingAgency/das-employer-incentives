using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Map;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public class ApprenticeshipIncentiveDataRepository : IApprenticeshipIncentiveDataRepository
    {
        private readonly Lazy<EmployerIncentivesDbContext> _lazyContext;
        private EmployerIncentivesDbContext _dbContext => _lazyContext.Value;

        public ApprenticeshipIncentiveDataRepository(Lazy<EmployerIncentivesDbContext> dbContext)
        {
            _lazyContext = dbContext;
        }

        public async Task Add(ApprenticeshipIncentiveModel apprenticeshipIncentive)
        {
            await _dbContext.AddAsync(apprenticeshipIncentive.Map());
        }

        public async Task<List<ApprenticeshipIncentiveModel>> FindApprenticeshipIncentivesWithoutPendingPayments()
        {
            var collectionPeriods = _dbContext.CollectionPeriods.AsEnumerable();

            var queryResults = _dbContext.ApprenticeshipIncentives.Where(x => x.PendingPayments.Count == 0);
            var results = new List<ApprenticeshipIncentiveModel>();
            foreach(var incentive in queryResults)
            {
                results.Add(incentive.Map(collectionPeriods));
            }

            return await Task.FromResult(results);
        }

        public async Task<ApprenticeshipIncentiveModel> FindByApprenticeshipId(Guid incentiveApplicationApprenticeshipId)
        {
            var collectionPeriods = _dbContext.CollectionPeriods.AsEnumerable();

            var apprenticeshipIncentive = await _dbContext.ApprenticeshipIncentives
               .Include(x => x.PendingPayments).ThenInclude(x => x.ValidationResults)
               .FirstOrDefaultAsync(a => a.IncentiveApplicationApprenticeshipId == incentiveApplicationApprenticeshipId);
            if (apprenticeshipIncentive != null)
            {
                return apprenticeshipIncentive.Map(collectionPeriods);
            }
            return null;
        }

        public async Task<List<ApprenticeshipIncentiveModel>> FindApprenticeshipIncentiveByUlnWithinAccountLegalEntity(long uln, long accountLegalEntityId)
        {
            var apprenticeships = await _dbContext.ApprenticeshipIncentives.Where(a => a.ULN == uln && a.AccountLegalEntityId == accountLegalEntityId).ToListAsync();
            var collectionPeriods = await _dbContext.CollectionPeriods.ToListAsync();

            return apprenticeships.Select(x => x.Map(collectionPeriods)).ToList();
        }

        public async Task<ApprenticeshipIncentiveModel> Get(Guid id)
        {
            var apprenticeshipIncentive = await _dbContext.ApprenticeshipIncentives
                .Include(x => x.PendingPayments)
                .ThenInclude(x => x.ValidationResults)
                .Include(x => x.Payments)
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
            }
        }

        public async Task Delete(ApprenticeshipIncentiveModel apprenticeshipIncentive)
        {
            var deletedIncentive = apprenticeshipIncentive.Map();

            var existingIncentive = await _dbContext.ApprenticeshipIncentives.FirstOrDefaultAsync(x => x.Id == deletedIncentive.Id);

            foreach(var pendingPayment in existingIncentive.PendingPayments)
            {
                foreach (var validationResult in pendingPayment.ValidationResults)
                {
                    _dbContext.Remove(validationResult);
                }
                _dbContext.Remove(pendingPayment);
            }
            _dbContext.Remove(existingIncentive);
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

            foreach (var payment in updatedIncentive.Payments)
            {
                var existingPayment = existingIncentive.Payments.SingleOrDefault(p => p.Id == payment.Id);

                if (existingPayment != null)
                {
                    _dbContext.Entry(existingPayment).CurrentValues.SetValues(payment);
                }
                else
                {
                    _dbContext.Payments.Add(payment);
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
