using System;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests
{
    public class TestDateTimeService : IDateTimeService
    {
        private DateTime _currentDate = DateTime.Now;

        public DateTime Now()
        {
            return _currentDate;
        }

        public void SetCurrentDate(DateTime date)
        {
            _currentDate = date;
        }
    }
}
