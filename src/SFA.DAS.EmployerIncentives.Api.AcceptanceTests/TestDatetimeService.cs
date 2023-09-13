using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests
{
    [ExcludeFromCodeCoverage]
    public class TestDateTimeService : IDateTimeService
    {
        private DateTime _dateTimeNow;
        private DateTime _dateTimeUtcNow;

        public TestDateTimeService()
        {
            _dateTimeNow = DateTime.Now;
            _dateTimeUtcNow = DateTime.UtcNow;
        }

        public void SetUtcNow(DateTime dateTime)
        {
            _dateTimeNow = dateTime;
            _dateTimeUtcNow = dateTime;
        }

        public DateTime Now()
        {
            return _dateTimeNow;
        }

        public DateTime UtcNow()
        {
            return _dateTimeUtcNow;
        }
    }
}
