using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Enums;
using System;
namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects.Earnings
{
    public class ActiveEarningsCalculator : DefaultEarningsCalculator
    {
        public ActiveEarningsCalculator(
            ApprenticeshipIncentiveModel model,
            Incentive incentive,
            CollectionCalendar collectionCalendar,
            Action<IDomainEvent> eventHandler) : base(model, incentive, collectionCalendar, eventHandler)
        {
            if (model.Status != IncentiveStatus.Active)
            {
                throw new ArgumentException("invalid apprenticeship incentive status");
            }
        }
    }
}
