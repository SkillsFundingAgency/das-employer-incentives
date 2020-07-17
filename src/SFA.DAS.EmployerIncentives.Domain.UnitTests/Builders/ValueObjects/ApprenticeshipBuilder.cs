using System;
using System.Collections.Generic;
using System.Text;
using SFA.DAS.EmployerIncentives.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.Builders.ValueObjects
{
    public class ApprenticeshipBuilder
    {
        private long _uniqueLearnerNumber;
        private string _name;
        private DateTime _dateOfBirth;
        private DateTime _startDate;
        private bool _isApproved;

        public ApprenticeshipBuilder()
        {
            _uniqueLearnerNumber = 1234567;
            _name = "Some Learner";
            _dateOfBirth = DateTime.Now.AddYears(-20);
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

        public Apprenticeship Build()
        {
            return new Apprenticeship(_uniqueLearnerNumber, _name, _dateOfBirth, _startDate, _isApproved);
        }
    }
}
