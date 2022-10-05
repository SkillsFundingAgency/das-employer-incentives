using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
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
               .PaymentsForApprenticeshipInAcademicYear(incentive.Apprenticeship.Id, nextPayment.DueDate)
               .Any(p => p.Period == nextPayment.CollectionPeriod?.PeriodNumber 
                         && !p.IsPayable);

            return hasLock;
        }

        public static bool IsInLearning(this LearnerSubmissionDto learnerData, Domain.ApprenticeshipIncentives.ApprenticeshipIncentive incentive)
        {
            if (incentive == null) return false;
            var nextPayment = incentive.NextDuePayment;
            if (nextPayment == null) return false;

            return true;
        }

        public static LearningStoppedStatus IsStopped(this LearnerSubmissionDto learnerData,
            Domain.ApprenticeshipIncentives.ApprenticeshipIncentive incentive,
            Domain.ValueObjects.CollectionCalendar collectionCalendar)
        {
            if (incentive == null) return new LearningStoppedStatus(false);

            var matchedRecords =
                (from tr in learnerData.Training
                    where tr.Reference == PROGRAM_REFERENCE
                    from pe in tr.PriceEpisodes
                    from p in pe.Periods.DefaultIfEmpty()
                    where (p is null || p.ApprenticeshipId == incentive.Apprenticeship.Id)
                    select new
                    {
                        p?.ApprenticeshipId,
                        pe.StartDate,
                        EndDate = pe.EndDate ?? collectionCalendar.GetAcademicYearEndDate(pe.AcademicYear),
                        pe.AcademicYear
                    })
                .OrderByDescending(x => x.StartDate)
                .Select(c => (c.ApprenticeshipId, c.StartDate, c.EndDate, c.AcademicYear))
                .ToArray();

            if (!matchedRecords.Any()) return new LearningStoppedStatus(false);

            var latestPriceEpisode = matchedRecords.First();

            if (HasNoPeriods(latestPriceEpisode) 
                || HasEndedAndNotOnAcademicYearsEve(collectionCalendar, latestPriceEpisode)
                || SubmissionIsInNextAcademicYearToLearnerData(latestPriceEpisode, learnerData, collectionCalendar)
                )
            {
                return new LearningStoppedStatus(true, latestPriceEpisode.EndDate.AddDays(1));
            }

            return new LearningStoppedStatus(false, latestPriceEpisode.StartDate);
        }
        private static bool SubmissionIsInNextAcademicYearToLearnerData((long? ApprenticeshipId, DateTime StartDate, DateTime EndDate, string AcademicYear) latestPriceEpisode, LearnerSubmissionDto learnerData, Domain.ValueObjects.CollectionCalendar collectionCalendar)
        {
            if(collectionCalendar.GetAcademicYearEndDate(learnerData.AcademicYear) > collectionCalendar.GetAcademicYearEndDate(latestPriceEpisode.AcademicYear))
            {
                return true;
            }

            return false;
        }

        private static bool HasEndedAndNotOnAcademicYearsEve(Domain.ValueObjects.CollectionCalendar collectionCalendar,
            (long? ApprenticeshipId, DateTime StartDate, DateTime EndDate, string AcademicYear) episode)
        {
            return episode.EndDate.Date < collectionCalendar.GetActivePeriod().CensusDate
                   && episode.EndDate.Date != collectionCalendar.GetAcademicYearEndDate(episode.AcademicYear);
        }

        private static bool HasNoPeriods((long? ApprenticeshipId, DateTime StartDate, DateTime EndDate, string AcademicYear) episode)
        {
            return episode.ApprenticeshipId == null;
        }

        public static IList<LearningPeriod> LearningPeriods(this LearnerSubmissionDto learnerData, Domain.ApprenticeshipIncentives.ApprenticeshipIncentive incentive, Domain.ValueObjects.CollectionCalendar collectionCalendar)
        {
            if (learnerData == null || learnerData.Training == null) return new List<LearningPeriod>();

            var periods = 
              (from tr in learnerData.Training
               where tr.Reference == PROGRAM_REFERENCE
               from pe in tr.PriceEpisodes
               from p in pe.Periods
               where p.ApprenticeshipId == incentive.Apprenticeship.Id && (pe.EndDate ?? collectionCalendar.GetAcademicYearEndDate(pe.AcademicYear)) >= pe.StartDate
               select new LearningPeriod(pe.StartDate, pe.EndDate ?? collectionCalendar.GetAcademicYearEndDate(pe.AcademicYear)))
              .Distinct()
              .OrderBy(p => p.StartDate)
              .ToList();

            return MergeLearningPeriods(periods);
        }

        private static IList<LearningPeriod> MergeLearningPeriods(IList<LearningPeriod> periods)
        {
            for (var i = 0; i < periods.Count - 1; i++)
            {
                if (PeriodsAreAdjacent(periods[i], periods[i + 1]) ||
                    PeriodsOverlap(periods[i], periods[i + 1]) ||
                    PeriodsAreInclusive(periods[i], periods[i + 1]))
                {
                    var start = MinDate(periods[i].StartDate, periods[i + 1].StartDate);
                    var end = MaxDate(periods[i].EndDate, periods[i + 1].EndDate);
                    periods.Add(new LearningPeriod(start, end));
                    periods.RemoveAt(i);
                    periods.RemoveAt(i);

                    i = -1; // restart
                }
            }

            return periods.OrderBy(x => x.StartDate).ToList();
        }

        private static bool PeriodsAreAdjacent(LearningPeriod firstPeriod, LearningPeriod secondPeriod)
        {
            var gap = Math.Abs((secondPeriod.StartDate - firstPeriod.EndDate).Days);

            return gap <= 1;
        }

        private static bool PeriodsAreInclusive(LearningPeriod firstPeriod, LearningPeriod secondPeriod)
        {
            return ((secondPeriod.StartDate > firstPeriod.StartDate) &&
                    (secondPeriod.EndDate < firstPeriod.EndDate)) ||
                   ((firstPeriod.StartDate > secondPeriod.StartDate) &&
                    (firstPeriod.EndDate < secondPeriod.EndDate));
        }

        private static bool PeriodsOverlap(LearningPeriod firstPeriod, LearningPeriod secondPeriod)
        {
            return ((secondPeriod.StartDate > firstPeriod.StartDate) &&
                    (firstPeriod.EndDate > secondPeriod.StartDate));
        }

        private static DateTime MinDate(DateTime a, DateTime b)
        {
            return new DateTime(Math.Min(a.Ticks, b.Ticks));
        }

        private static DateTime MaxDate(DateTime a, DateTime b)
        {
            return new DateTime(Math.Max(a.Ticks, b.Ticks));
        }

        private static IEnumerable<PeriodDto> PaymentsForApprenticeship(this LearnerSubmissionDto data, long apprenticeshipId)
        {
            return data.Training.Where(t => t.Reference == PROGRAM_REFERENCE)
                .SelectMany(t => t.PriceEpisodes)
                .SelectMany(e => e.Periods)
                .Where(p => p.ApprenticeshipId == apprenticeshipId);
        }

        private static IEnumerable<PeriodDto> PaymentsForApprenticeshipInAcademicYear(this LearnerSubmissionDto data, long apprenticeshipId, DateTime paymentDueDate)
        {
            return data.Training.Where(t => t.Reference == PROGRAM_REFERENCE)
                .SelectMany(t => t.PriceEpisodes.Where(pe => pe.StartDate <= paymentDueDate
                && (pe.EndDate >= paymentDueDate || pe.EndDate == null)))
                .SelectMany(e => e.Periods)
                .Where(p => p.ApprenticeshipId == apprenticeshipId);
        }
       
    }
}
