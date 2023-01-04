using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models
{
    public static class ApprenticeApplicationExtensions
    {
        public static bool IsPaymentEstimated(this ApprenticeApplication apprenticeApplication, EarningType earningType, IDateTimeService dateTimeService)
        {
            var paymentDate = apprenticeApplication.FirstPaymentDate;
            if (earningType == EarningType.SecondPayment)
            {
                paymentDate = apprenticeApplication.SecondPaymentDate;
            }

            if (!paymentDate.HasValue)
            {
                return true;
            }

            if (dateTimeService.Now().Day < 27 &&
                paymentDate.Value.Year == dateTimeService.Now().Year &&
                paymentDate.Value.Month == dateTimeService.Now().Month)
            {
                return true;
            }
            return false;
        }

        public static bool IsIncentiveCompleted(this ApprenticeApplication apprenticeApplication, IDateTimeService dateTimeService)
        {
            if (!apprenticeApplication.SecondPaymentDate.HasValue || !apprenticeApplication.LearningStoppedDate.HasValue)
            {
                return false;
            }

            if (apprenticeApplication.LearningStoppedDate > dateTimeService.Now())
            {
                return false;
            }

            if (apprenticeApplication.LearningStoppedDate < apprenticeApplication.SecondPaymentDate)
            {
                return false;
            }

            return true;
        }
    }
}
