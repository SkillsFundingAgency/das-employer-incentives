using System;
using SFA.DAS.EmployerIncentives.ValueObjects;

namespace SFA.DAS.EmployerIncentives.UnitTests.Builders.ValueObjects
{
    public class ApprenticeshipBuilder
    {
        private long _uniqueLearnerNumber;
        private DateTime _startDate;
        private bool _isApproved;

        public ApprenticeshipBuilder()
        {
            _uniqueLearnerNumber = 1234567;
            _startDate = DateTime.Now.AddYears(-1);
            _isApproved = true;
        }

        public ApprenticeshipBuilder WithStartDate(DateTime startDate)
        {
            _startDate = startDate;
            return this;
        }

        public ApprenticeshipBuilder WithIsApproved(bool isApproved)
        {
            _isApproved = isApproved;
            return this;
        }

        public ApprenticeshipBuilder WithValidIncentiveProperties()
        {
            _startDate = new DateTime(2021, 8, 1);
            _isApproved = true;
            return this;
        }

        public Apprenticeship Build()
        {
            return new Apprenticeship(_uniqueLearnerNumber, _startDate, _isApproved);
        }
    }
}
