using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
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
                return matchedRecords.OrderBy(m => m.StartDate).First().StartDate;
            }

            return null;
        }

        public static bool InLearningForApprenticeship(this LearnerSubmissionDto learnerData, long apprenticeshipId, PendingPayment pendingPayment)
        {
            // 1. For a given payment due date check whether due date falls in between 
            // the start and end date of a price episode that contains a period with 
            // a matching apprenticeship ID OR where there is no end date for the 
            // price episode with a matching apprenticeship ID, the payment due date 
            // is after the price episode start date
            // 2. If a price episode meeting criteria in step 1 is found, set InLearning
            // to True ELSE set InLearning to False

            if (pendingPayment == null)
            {
                return false;
            }

            var matchedRecords =
               from tr in learnerData.Training
               where tr.Reference == "ZPROG001"
               from pe in tr.PriceEpisodes
               from p in pe.Periods
               where p.ApprenticeshipId == apprenticeshipId
               select new
               {
                   p.ApprenticeshipId,
                   pe.StartDate,
                   pe.EndDate,
                   p.Period
               };

            bool isInLearning = false;
            if (matchedRecords.Any())
            {
                foreach(var matchedRecord in matchedRecords)
                {
                    var endDate = matchedRecord.EndDate ?? pendingPayment.DueDate;
                    if (pendingPayment.DueDate >= matchedRecord.StartDate &&
                       pendingPayment.DueDate <= endDate)
                    {
                        isInLearning = true;
                        break;
                    }
                }
            }

            return isInLearning;
        }
    }
}
