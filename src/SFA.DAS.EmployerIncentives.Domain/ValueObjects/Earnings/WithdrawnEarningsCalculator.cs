using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Enums;
using System;
namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects.Earnings
{
    public class WithdrawnEarningsCalculator : EarningsCalculator
    {
        public WithdrawnEarningsCalculator(
            ApprenticeshipIncentiveModel model,
            Incentive incentive,
            CollectionCalendar collectionCalendar,
            Action<IDomainEvent> eventHandler) : base(model, incentive, collectionCalendar, eventHandler)
        {
            if (model.Status != IncentiveStatus.Withdrawn)
            {
                throw new ArgumentException("invalid apprenticeship incentive status");
            }
        }

        public override void Calculate()
        {
            if(Model.PreviousStatus == IncentiveStatus.Active)
            {
                if (HasPaidEarnings())
                {
                    RemoveUnpaidEarnings();
                    ClawbackAllPayments();
                    Model.PausePayments = false;
                }
                else
                {
                    RemoveUnpaidEarnings();
                }
            }
        }

        public override void ReCalculate()
        {
            // do nothing
        }
    }
}
