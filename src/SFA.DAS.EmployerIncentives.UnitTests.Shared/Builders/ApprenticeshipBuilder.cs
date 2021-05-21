using AutoFixture;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;

namespace SFA.DAS.EmployerIncentives.UnitTests.Shared.Builders
{
    internal class ApprenticeshipBuilder
    {
        private readonly Fixture _fixture;
        private Apprenticeship _apprenticeship;

        public ApprenticeshipBuilder()
        {
            _fixture = new Fixture();
            _apprenticeship = new Apprenticeship(
            _fixture.Create<long>(),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<DateTime>(),
            _fixture.Create<long>(),
            _fixture.Create<ApprenticeshipEmployerType>(),
            _fixture.Create<string>(),
            _fixture.Create<DateTime>());
        }

        public ApprenticeshipBuilder WithDateOfBirth(DateTime dateOfBirth)
        {
            _apprenticeship = new Apprenticeship(
                _apprenticeship.Id,
                _apprenticeship.FirstName,
                _apprenticeship.LastName,
                dateOfBirth,
                _apprenticeship.UniqueLearnerNumber,
                _apprenticeship.EmployerType,
                _apprenticeship.CourseName,
                _apprenticeship.EmploymentStartDate);
            return this;
        }

        public Apprenticeship Build()
        {
            return _apprenticeship;
        }
    }
}
