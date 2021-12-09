using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Map;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Enums;

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
            var incentive = apprenticeshipIncentive.Map();
            await _dbContext.AddAsync(incentive);
        }

        public async Task<List<ApprenticeshipIncentiveModel>> FindApprenticeshipIncentivesWithoutPendingPayments(bool includeStopped = false, bool includeWithdrawn = false)
        {
            var collectionPeriods = _dbContext.CollectionPeriods.AsEnumerable();

            var queryResults = _dbContext.ApprenticeshipIncentives.Where(x => x.PendingPayments.Count == 0);
            if(!includeStopped)
                queryResults = queryResults.Where(x => x.Status != IncentiveStatus.Stopped);

            if(!includeWithdrawn)
                queryResults = queryResults.Where(x => x.Status != IncentiveStatus.Withdrawn);

            var results = new List<ApprenticeshipIncentiveModel>();
            foreach (var incentive in queryResults)
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
               .Include(x => x.Payments)
               .Include(x => x.ClawbackPayments)
               .Include(x => x.BreakInLearnings)
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

        public async Task<ApprenticeshipIncentiveModel> FindApprenticeshipIncentiveByEmploymentCheckId(Guid correlationId)
        {
            var employmentCheck = await _dbContext.EmploymentChecks.SingleOrDefaultAsync(c => c.CorrelationId == correlationId);
            if(employmentCheck == null)
            {
                return null;
            }

            return await Get(employmentCheck.ApprenticeshipIncentiveId);
        }

        public async Task<List<ApprenticeshipIncentiveModel>> FindIncentivesWithLearningFound()
        {
            var apprenticeships = (from incentive in _dbContext.ApprenticeshipIncentives.Include(x => x.EmploymentChecks)
                    join learner in _dbContext.Learners.Where(x => x.LearningFound.HasValue && x.LearningFound.Value) 
                        on incentive.Id equals learner.ApprenticeshipIncentiveId
                        where incentive.Status != IncentiveStatus.Withdrawn
                        select new {Incentive = incentive}
                );

            var collectionPeriods = await _dbContext.CollectionPeriods.ToListAsync();

            return apprenticeships.Select(x => x.Incentive.Map(collectionPeriods)).ToList();
        }

        public async Task<ApprenticeshipIncentiveModel> Get(Guid id)
        {
            var apprenticeshipIncentive = await _dbContext.ApprenticeshipIncentives
                .Include(x => x.PendingPayments)
                .ThenInclude(x => x.ValidationResults)
                .Include(x => x.Payments)
                .Include(x => x.ClawbackPayments)
                .Include(x => x.BreakInLearnings)
                .Include(x => x.EmploymentChecks)
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

            foreach (var pendingPayment in existingIncentive.PendingPayments)
            {
                foreach (var validationResult in pendingPayment.ValidationResults)
                {
                    _dbContext.Remove(validationResult);
                }
                _dbContext.Remove(pendingPayment);
            }
        }

        private void UpdateApprenticeshipIncentive(ApprenticeshipIncentive updatedIncentive, ApprenticeshipIncentive existingIncentive)
        {
            _dbContext.Entry(existingIncentive).CurrentValues.SetValues(updatedIncentive);

            RemoveDeletedClawbacks(updatedIncentive, existingIncentive);
            RemoveDeletedPayments(updatedIncentive, existingIncentive);
            RemoveDeletedPendingPayments(updatedIncentive, existingIncentive);
            RemoveDeletedBreaksInLearning(updatedIncentive, existingIncentive);
            RemoveDeletedEmploymentChecks(updatedIncentive, existingIncentive);

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

            foreach (var clawback in updatedIncentive.ClawbackPayments)
            {
                var existingClawback = existingIncentive.ClawbackPayments.SingleOrDefault(p => p.Id == clawback.Id);

                if (existingClawback != null)
                {
                    _dbContext.Entry(existingClawback).CurrentValues.SetValues(clawback);
                }
                else
                {
                    _dbContext.ClawbackPayments.Add(clawback);
                }
            }

            foreach (var breakInLearning in updatedIncentive.BreakInLearnings)
            {
                var existingBreakInLearning = existingIncentive.BreakInLearnings.SingleOrDefault(p => p.StartDate == breakInLearning.StartDate);

                if (existingBreakInLearning != null)
                {
                    breakInLearning.CreatedDate = existingBreakInLearning.CreatedDate;
                    _dbContext.Entry(existingBreakInLearning).CurrentValues.SetValues(breakInLearning);

                    if (_dbContext.Entry(existingBreakInLearning).State == EntityState.Modified)
                    {
                        existingBreakInLearning.UpdatedDate = DateTime.Now;
                    }
                }
                else
                {
                    breakInLearning.CreatedDate = DateTime.Now;
                    _dbContext.BreakInLearnings.Add(breakInLearning);
                }
            }

            foreach (var employmentCheck in updatedIncentive.EmploymentChecks)
            {
                var existingEmploymentCheck = existingIncentive.EmploymentChecks.SingleOrDefault(p => p.Id == employmentCheck.Id);

                if (existingEmploymentCheck != null)
                {
                    _dbContext.Entry(existingEmploymentCheck).CurrentValues.SetValues(employmentCheck);
                    
                    if (_dbContext.Entry(existingEmploymentCheck).State == EntityState.Modified)
                    {
                        existingEmploymentCheck.UpdatedDateTime = DateTime.Now;
                    }                    
                }
                else
                {
                    employmentCheck.CreatedDateTime = DateTime.Now;
                    _dbContext.EmploymentChecks.Add(employmentCheck);
                }
            }
        }

        private void RemoveDeletedBreaksInLearning(ApprenticeshipIncentive updatedIncentive, ApprenticeshipIncentive existingIncentive)
        {
            foreach (var breakInLearning in existingIncentive.BreakInLearnings)
            {
                if (updatedIncentive.BreakInLearnings.All(c => c.StartDate != breakInLearning.StartDate))
                {
                    _dbContext.BreakInLearnings.Remove(breakInLearning);
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
            foreach (var existingPendingPayment in existingIncentive.PendingPayments)
            {
                if (updatedIncentive.PendingPayments.All(c => c.Id != existingPendingPayment.Id))
                {
                    _dbContext.PendingPayments.Remove(existingPendingPayment);
                }
            }
        }

        private void RemoveDeletedPayments(ApprenticeshipIncentive updatedIncentive, ApprenticeshipIncentive existingIncentive)
        {
            foreach (var existingPayment in existingIncentive.Payments)
            {
                if (!updatedIncentive.Payments.Any(c => c.Id == existingPayment.Id))
                {
                    _dbContext.Payments.Remove(existingPayment);
                }
            }
        }

        private void RemoveDeletedClawbacks(ApprenticeshipIncentive updatedIncentive, ApprenticeshipIncentive existingIncentive)
        {
            foreach (var existingPayment in existingIncentive.ClawbackPayments)
            {
                if (!updatedIncentive.ClawbackPayments.Any(c => c.Id == existingPayment.Id))
                {
                    _dbContext.ClawbackPayments.Remove(existingPayment);
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

        private void RemoveDeletedEmploymentChecks(ApprenticeshipIncentive updatedIncentive, ApprenticeshipIncentive existingIncentive)
        {
            foreach (var existingEmploymentCheck in existingIncentive.EmploymentChecks)
            {
                if (!updatedIncentive.EmploymentChecks.Any(c => c.Id == existingEmploymentCheck.Id))
                {
                    _dbContext.EmploymentChecks.Remove(existingEmploymentCheck);
                }
            }
        }       
    }
}
