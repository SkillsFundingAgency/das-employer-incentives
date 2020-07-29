using System;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Domain.Factories;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.IncentiveApplicationTests
{
    public class WhenCreatedApprenticeship
    {
        private IncentiveApplicationFactory _sut;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _sut = new IncentiveApplicationFactory();
            _fixture = new Fixture();
        }

        [Test]
        public void Then_the_id_is_set()
        {
            // Arrange
            var id = _fixture.Create<Guid>();

            // Act
            var apprenticeship = _sut.CreateNewApprenticeship(id, _fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<DateTime>(), _fixture.Create<long>(), _fixture.Create<DateTime>(), _fixture.Create<ApprenticeshipEmployerType>());

            // Assert
            apprenticeship.Id.Should().Be(id);
        }

        [Test]
        public void Then_the_apprenticeship_id_is_set()
        {
            // Arrange
            var apprenticeshipId = _fixture.Create<int>();

            // Act
            var apprenticeship = _sut.CreateNewApprenticeship(_fixture.Create<Guid>(), apprenticeshipId, _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<DateTime>(), _fixture.Create<long>(), _fixture.Create<DateTime>(), _fixture.Create<ApprenticeshipEmployerType>());

            // Assert
            apprenticeship.ApprenticeshipId.Should().Be(apprenticeshipId);
        }

        [Test]
        public void Then_the_first_name_is_set()
        {
            // Arrange
            var firstName = _fixture.Create<string>();

            // Act
            var apprenticeship = _sut.CreateNewApprenticeship(_fixture.Create<Guid>(), _fixture.Create<int>(), firstName, _fixture.Create<string>(),
                _fixture.Create<DateTime>(), _fixture.Create<long>(), _fixture.Create<DateTime>(), _fixture.Create<ApprenticeshipEmployerType>());

            // Assert
            apprenticeship.FirstName.Should().Be(firstName);
        }

        [Test]
        public void Then_the_last_name_is_set()
        {
            // Arrange
            var lastName = _fixture.Create<string>();

            // Act
            var apprenticeship = _sut.CreateNewApprenticeship(_fixture.Create<Guid>(), _fixture.Create<int>(), _fixture.Create<string>(), lastName,
                _fixture.Create<DateTime>(), _fixture.Create<long>(), _fixture.Create<DateTime>(), _fixture.Create<ApprenticeshipEmployerType>());

            // Assert
            apprenticeship.LastName.Should().Be(lastName);
        }

        [Test]
        public void Then_the_date_of_birth_is_set()
        {
            // Arrange
            var dateOfBirth = _fixture.Create<DateTime>();

            // Act
            var apprenticeship = _sut.CreateNewApprenticeship(_fixture.Create<Guid>(), _fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<string>(),
                dateOfBirth, _fixture.Create<long>(), _fixture.Create<DateTime>(), _fixture.Create<ApprenticeshipEmployerType>());

            // Assert
            apprenticeship.DateOfBirth.Should().Be(dateOfBirth);
        }

        [Test]
        public void Then_the_uln_is_set()
        {
            // Arrange
            var uln = _fixture.Create<long>();

            // Act
            var apprenticeship = _sut.CreateNewApprenticeship(_fixture.Create<Guid>(), _fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<DateTime>(), uln, _fixture.Create<DateTime>(), _fixture.Create<ApprenticeshipEmployerType>());

            // Assert
            apprenticeship.Uln.Should().Be(uln);
        }

        [Test]
        public void Then_the_planned_start_date_is_set()
        {
            // Arrange
            var plannedStartDate = _fixture.Create<DateTime>();

            // Act
            var apprenticeship = _sut.CreateNewApprenticeship(_fixture.Create<Guid>(), _fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<DateTime>(), _fixture.Create<long>(), plannedStartDate, _fixture.Create<ApprenticeshipEmployerType>());

            // Assert
            apprenticeship.PlannedStartDate.Should().Be(plannedStartDate);
        }

        [Test]
        public void Then_the_employer_type_on_approval_is_set()
        {
            // Arrange
            var employerType = _fixture.Create<ApprenticeshipEmployerType>();

            // Act
            var apprenticeship = _sut.CreateNewApprenticeship(_fixture.Create<Guid>(), _fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<DateTime>(), _fixture.Create<long>(), _fixture.Create<DateTime>(), employerType);

            // Assert
            apprenticeship.ApprenticeshipEmployerTypeOnApproval.Should().Be(employerType);
        }
    }
}
