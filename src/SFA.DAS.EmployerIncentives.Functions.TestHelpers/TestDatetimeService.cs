using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerIncentives.Functions.TestHelpers
{
    [ExcludeFromCodeCoverage]
    public class TestDateTimeService : IDateTimeService
    {
        private DateTime _dateTimeUtcNow;

        public TestDateTimeService()
        {
            _dateTimeUtcNow = DateTime.UtcNow;
        }

        public void SetUtcNow(DateTime dateTime)
        {
            _dateTimeUtcNow = dateTime;
        }

        public DateTime UtcNow()
        {
            return _dateTimeUtcNow;
        }
    }
}
