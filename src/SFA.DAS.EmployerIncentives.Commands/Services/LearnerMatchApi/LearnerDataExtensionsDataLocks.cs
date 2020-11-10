using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi
{
    public static class LearnerDataExtensionsDataLocks
    {
        //    Conditions for DataLock:
        // 1. ZPROG001 reference
        // 2. PriceEpisode.StartDate <= NexPayment.DueDate => PriceEpisode.EndDate
        // 3. Period.Period == NexPayment.CollectionPeriod
        // 4. Period.ApprenticeshipId == ApprenticeshipIncentive.ApprenticeshipId
        // 5. Period.IsPayable == false

        public static bool HasProviderDataLocks(this LearnerSubmissionDto learnerData, Learner learner)
        {
            if (learner.NextPendingPayment == null) return false;

            return (from tr in learnerData.Training
                    where tr.Reference == "ZPROG001"
                    from pe in tr.PriceEpisodes
                    from p in pe.Periods
                    where p.ApprenticeshipId == learner.ApprenticeshipId
                          && !p.IsPayable
                          && p.Period == learner.NextPendingPayment.CollectionPeriod
                          && pe.StartDate <= learner.NextPendingPayment.DueDate
                          && (pe.EndDate >= learner.NextPendingPayment.DueDate || pe.EndDate == null)
                    select new { }).Any();
        }
    }
}
