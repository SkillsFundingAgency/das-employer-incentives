using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi
{
    public static class LearnerDataExtensions
    {
        public static DateTime? LearningStartDateForApprenticeship(this LearnerSubmissionDto learnerData, long apprenticeshipId)
        {
            // To determine which price episode to look at requires looking at the Payable Periods within a Price Episode, because Payable Periods are labelled with the Apprenticeship ID.
            // searching for the earliest period with an apprenticeship Id that matches the commitment
            // the apprenticeship StartDate is the Start Date of the price episode to which that period belongs
            // this might be different to the “Start Date” for the “Training”, because the “Training” start date won’t update if there is a change of employer 

            var matchedRecords =
                from tr in learnerData.Training
                where tr.Reference == "ZPROG001"
                from pe in tr.PriceEpisodes
                from p in pe.Periods
                where p.ApprenticeshipId == apprenticeshipId
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


        public static LearningFoundStatus LearningFound(this LearnerSubmissionDto data, long apprenticeshipId)
        {
            if (data.Training == null || !data.Training.Any())
            {
                return new LearningFoundStatus("No learning aims were found");
            }

            if (data.Training.All(t => t.Reference != "ZPROG001"))
            {
                return new LearningFoundStatus("Learning aims were found for the ULN, but none of them had a reference of ZPROG001");
            }

            if (!data.Training.Any(t => t.Reference == "ZPROG001" && t.PriceEpisodes.Count > 0))
            {
                return new LearningFoundStatus("A ZPROG001 aim was found, but it had no price episodes");
            }

            if (!AnyPriceEpisodePeriodsForApprenticeship(data, apprenticeshipId))
            {
                return new LearningFoundStatus("A ZPROG001 aim was found, with price episodes, but with no periods with the apprenticeship id that matches the commitment");
            }

            return new LearningFoundStatus();
        }

        private static bool AnyPriceEpisodePeriodsForApprenticeship(LearnerSubmissionDto data, long apprenticeshipId)
        {
            return data.Training.Where(t => t.Reference == "ZPROG001")
                .SelectMany(t => t.PriceEpisodes)
                .SelectMany(e => e.Periods)
                .Any(p => p.ApprenticeshipId == apprenticeshipId);
        }
    }
}
