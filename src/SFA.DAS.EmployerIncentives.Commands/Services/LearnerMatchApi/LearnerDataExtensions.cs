using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi
{
    public static class LearnerDataExtensions
    {
        private static string PROGRAM_REFERENCE = "ZPROG001";

        public static DateTime? LearningStartDateForApprenticeship(this LearnerSubmissionDto learnerData, Domain.ApprenticeshipIncentives.ApprenticeshipIncentive incentive)
        {
            // To determine which price episode to look at requires looking at the Payable Periods within a Price Episode, because Payable Periods are labelled with the Apprenticeship ID.
            // searching for the earliest period with an apprenticeship Id that matches the commitment
            // the apprenticeship StartDate is the Start Date of the price episode to which that period belongs
            // this might be different to the “Start Date” for the “Training”, because the “Training” start date won’t update if there is a change of employer 

            var matchedRecords =
                from tr in learnerData.Training
                where tr.Reference == PROGRAM_REFERENCE
                from pe in tr.PriceEpisodes
                from p in pe.Periods
                where p.ApprenticeshipId == incentive.Apprenticeship.Id
                     && p.IsPayable
                select new
                {
                    p.ApprenticeshipId,
                    pe.StartDate
                };

            if (matchedRecords.Any())
            {
                return matchedRecords.OrderBy(m => m.StartDate).FirstOrDefault().StartDate;
            }

            return null;
        }

        public static LearningFoundStatus LearningFound(this LearnerSubmissionDto data, Domain.ApprenticeshipIncentives.ApprenticeshipIncentive incentive)
        {
            if (data.Training == null || !data.Training.Any())
            {
                return new LearningFoundStatus("No learning aims were found");
            }

            if (data.Training.All(t => t.Reference != PROGRAM_REFERENCE))
            {
                return new LearningFoundStatus("Learning aims were found for the ULN, but none of them had a reference of ZPROG001");
            }

            if (!data.Training.Any(t => t.Reference == PROGRAM_REFERENCE && t.PriceEpisodes.Count > 0))
            {
                return new LearningFoundStatus("A ZPROG001 aim was found, but it had no price episodes");
            }

            if (!data.PaymentsForApprenticeship(incentive.Apprenticeship.Id).Any())
            {
                return new LearningFoundStatus("A ZPROG001 aim was found, with price episodes, but with no periods with the apprenticeship id that matches the commitment");
            }

            return new LearningFoundStatus();
        }

        // *** Conditions for Provider DataLock ***
        // 1. ZPROG001 reference
        // 2. PriceEpisode.StartDate <= NextPayment.DueDate => PriceEpisode.EndDate
        // 3. Period.Period == NextPayment.CollectionPeriod
        // 4. Period.ApprenticeshipId == ApprenticeshipIncentive.ApprenticeshipId
        // 5. Period.IsPayable == false
        public static bool HasProviderDataLocks(this LearnerSubmissionDto data, Domain.ApprenticeshipIncentives.ApprenticeshipIncentive incentive)
        {
            if (incentive == null) return false;

            var pendingPayments = incentive.PendingPayments.Where(p => p.PaymentMadeDate == null);
            if(!pendingPayments.Any())
            {
                return false;
            }
            var nextPayment = pendingPayments.OrderBy(p => p.DueDate).First();
            if (nextPayment.PaymentYear != null && nextPayment.PeriodNumber.HasValue)
            {
                return data
               .PaymentsForApprenticeship(incentive.Apprenticeship.Id, nextPayment.DueDate)
               .Any(p => p.Period == nextPayment.PeriodNumber && !p.IsPayable);
            }

            return false;
        }

        private static IEnumerable<PeriodDto> PaymentsForApprenticeship(this LearnerSubmissionDto data, long apprenticeshipId)
        {
            return data.Training.Where(t => t.Reference == PROGRAM_REFERENCE)
                .SelectMany(t => t.PriceEpisodes)
                .SelectMany(e => e.Periods)
                .Where(p => p.ApprenticeshipId == apprenticeshipId);
        }

        private static IEnumerable<PeriodDto> PaymentsForApprenticeship(this LearnerSubmissionDto data, long apprenticeshipId, DateTime paymentDueDate)
        {
            return data.Training.Where(t => t.Reference == PROGRAM_REFERENCE)
                .SelectMany(t => t.PriceEpisodes.Where(pe => pe.StartDate <= paymentDueDate
                && (pe.EndDate >= paymentDueDate || pe.EndDate == null)))
                .SelectMany(e => e.Periods)
                .Where(p => p.ApprenticeshipId == apprenticeshipId);
        }
    }
}
