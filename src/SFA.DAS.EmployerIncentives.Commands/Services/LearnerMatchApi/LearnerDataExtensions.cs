using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi
{
    public static class LearnerDataExtensions
    {
        private static string PROGRAM_REFERENCE = "ZPROG001";

        public static DateTime? LearningStartDate(this LearnerSubmissionDto learnerData, Domain.ApprenticeshipIncentives.ApprenticeshipIncentive incentive)
        {
            // To determine which price episode to look at requires looking at the Payable Periods within a Price Episode, because Payable Periods are labelled with the Apprenticeship ID.
            // searching for the earliest period with an apprenticeship Id that matches the commitment
            // the apprenticeship StartDate is the Start Date of the price episode to which that period belongs
            // this might be different to the “Start Date” for the “Training”, because the “Training” start date won’t update if there is a change of employer 

            var matchedRecords =
                (from tr in learnerData.Training
                 where tr.Reference == PROGRAM_REFERENCE
                 from pe in tr.PriceEpisodes
                 from p in pe.Periods
                 where p.ApprenticeshipId == incentive.Apprenticeship.Id
                       && p.IsPayable
                 select new
                 {
                     p.ApprenticeshipId,
                     pe.StartDate
                 }).ToArray();

            if (matchedRecords.Any())
            {
                return matchedRecords.OrderBy(m => m.StartDate).First().StartDate;
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
            var nextPayment = incentive.NextDuePayment;
            if (nextPayment == null) return false;

            var hasLock = data
               .PaymentsForApprenticeship(incentive.Apprenticeship.Id, nextPayment.DueDate)
               .Any(p => p.Period == nextPayment.PeriodNumber && !p.IsPayable);

            return hasLock;
        }

        public static bool IsInLearning(this LearnerSubmissionDto learnerData, Domain.ApprenticeshipIncentives.ApprenticeshipIncentive incentive)
        {
            // 1. For a given payment due date check whether due date falls in between 
            // the start and end date of a price episode that contains a period with 
            // a matching apprenticeship ID OR where there is no end date for the 
            // price episode with a matching apprenticeship ID, the payment due date 
            // is after the price episode start date
            // 2. If a price episode meeting criteria in step 1 is found, set InLearning
            // to True ELSE set InLearning to False

            if (incentive == null) return false;
            var nextPayment = incentive.NextDuePayment;
            if (nextPayment == null) return false;

            var matchedRecords =
               (from tr in learnerData.Training
                where tr.Reference == PROGRAM_REFERENCE
                from pe in tr.PriceEpisodes
                from p in pe.Periods
                where p.ApprenticeshipId == incentive.Apprenticeship.Id
                select new
                {
                    p.ApprenticeshipId,
                    pe.StartDate,
                    pe.EndDate,
                    p.Period
                }).ToArray();

            var isInLearning = false;
            if (matchedRecords.Any())
            {
                foreach (var matchedRecord in matchedRecords)
                {
                    var endDate = matchedRecord.EndDate ?? nextPayment.DueDate;
                    if (nextPayment.DueDate >= matchedRecord.StartDate &&
                        nextPayment.DueDate <= endDate)
                    {
                        isInLearning = true;
                        break;
                    }
                }
            }

            return isInLearning;
        }

        public static int? DaysInLearning(this LearnerSubmissionDto learnerData, Domain.ApprenticeshipIncentives.ApprenticeshipIncentive incentive, CollectionCalendar collectionCalendar)
        {
            // For the most recent price episode(there might only be one), if the price episode has a 
            // null end date the count should be from the price episode start date to the census 
            // date(inclusive) for the active period.Where the price episode has an end date, 
            // the count should be from the price episode start date to the earlier of the end date 
            // or the census date for the active period
            // Sum day counts across price episodes

            var startdate = learnerData.LearningStartDate(incentive);
            if (!startdate.HasValue)
            {
                return null;
            }

            var censusDate = collectionCalendar.GetActivePeriod().CensusDate.Date;

            var matchedRecords =
               (from tr in learnerData.Training
                where tr.Reference == PROGRAM_REFERENCE
                from pe in tr.PriceEpisodes
                from p in pe.Periods
                where p.ApprenticeshipId == incentive.Apprenticeship.Id
                select new
                {
                    p.ApprenticeshipId,
                    pe.StartDate,
                    pe.EndDate,
                }).Distinct().ToArray();

            if (matchedRecords.Any())
            {
                int daysInLearning = 0;

                foreach (var matchedRecord in matchedRecords)
                {
                    if (!matchedRecord.EndDate.HasValue)
                    {
                        daysInLearning += censusDate.Subtract(matchedRecord.StartDate.Date).Days + 1;
                    }
                    else
                    {
                        if (matchedRecord.EndDate.Value.Date < censusDate)
                        {
                            daysInLearning += matchedRecord.EndDate.Value.Date.Subtract(matchedRecord.StartDate.Date).Days + 1;
                        }
                        else
                        {
                            daysInLearning += censusDate.Subtract(matchedRecord.StartDate.Date).Days + 1;
                        }
                    }
                }

                return daysInLearning;
            }

            return null;
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
