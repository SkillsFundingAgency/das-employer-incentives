using System;

namespace SFA.DAS.EmployerIncentives.Domain.Interfaces
{
    public interface IDateTimeService
    {
        DateTime Now();

        DateTime UtcNow();
    }
}
