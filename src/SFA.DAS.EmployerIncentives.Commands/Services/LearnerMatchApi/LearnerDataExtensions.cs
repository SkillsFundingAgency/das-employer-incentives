using System;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi
{
    public static class LearnerDataExtensions
    {
        public static DateTime? LearningStartDateForAppenticeship(this LearnerSubmissionDto learnerData, long apprenticeshipId)
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
    }
}
