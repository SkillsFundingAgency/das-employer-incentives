using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.Services
{
    public class DateTimeService : IDateTimeService
    {
        public DateTime Now()
        {
            return DateTime.Now;
        }

        public DateTime UtcNow()
        {
            return DateTime.UtcNow;
        }
    }
}
