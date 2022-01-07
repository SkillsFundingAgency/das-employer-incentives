using System;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.Factories.IncentiveApplicationFactoryTests
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
            // Act
            var apprenticeship = _sut.CreateApprenticeship(_fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<DateTime>(), _fixture.Create<long>(), _fixture.Create<DateTime>(), _fixture.Create<ApprenticeshipEmployerType>(), _fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<DateTime>());

            // Assert
            apprenticeship.Id.Should().NotBe(Guid.Empty);
        }

        [Test]
        public void Then_the_apprenticeship_id_is_set()
        {
            // Arrange
            var apprenticeshipId = _fixture.Create<long>();

            // Act
            var apprenticeship = _sut.CreateApprenticeship(apprenticeshipId, _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<DateTime>(), _fixture.Create<long>(), _fixture.Create<DateTime>(), _fixture.Create<ApprenticeshipEmployerType>(), _fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<DateTime>());

            // Assert
            apprenticeship.ApprenticeshipId.Should().Be(apprenticeshipId);
        }

        [Test]
        public void Then_the_first_name_is_set()
        {
            // Arrange
            var firstName = _fixture.Create<string>();

            // Act
            var apprenticeship = _sut.CreateApprenticeship(_fixture.Create<long>(), firstName, _fixture.Create<string>(),
                _fixture.Create<DateTime>(), _fixture.Create<long>(), _fixture.Create<DateTime>(), _fixture.Create<ApprenticeshipEmployerType>(), _fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<DateTime>());

            // Assert
            apprenticeship.FirstName.Should().Be(firstName);
        }

        [Test]
        public void Then_the_last_name_is_set()
        {
            // Arrange
            var lastName = _fixture.Create<string>();

            // Act
            var apprenticeship = _sut.CreateApprenticeship(_fixture.Create<long>(), _fixture.Create<string>(), lastName,
                _fixture.Create<DateTime>(), _fixture.Create<long>(), _fixture.Create<DateTime>(), _fixture.Create<ApprenticeshipEmployerType>(), _fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<DateTime>());

            // Assert
            apprenticeship.LastName.Should().Be(lastName);
        }

        [Test]
        public void Then_the_date_of_birth_is_set()
        {
            // Arrange
            var dateOfBirth = _fixture.Create<DateTime>();

            // Act
            var apprenticeship = _sut.CreateApprenticeship(_fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<string>(),
                dateOfBirth, _fixture.Create<long>(), _fixture.Create<DateTime>(), _fixture.Create<ApprenticeshipEmployerType>(), _fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<DateTime>());

            // Assert
            apprenticeship.DateOfBirth.Should().Be(dateOfBirth);
        }

        [Test]
        public void Then_the_uln_is_set()
        {
            // Arrange
            var uln = _fixture.Create<long>();

            // Act
            var apprenticeship = _sut.CreateApprenticeship(_fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<DateTime>(), uln, _fixture.Create<DateTime>(), _fixture.Create<ApprenticeshipEmployerType>(), _fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<DateTime>());

            // Assert
            apprenticeship.ULN.Should().Be(uln);
        }

        [Test]
        public void Then_the_planned_start_date_is_set()
        {
            // Arrange
            var plannedStartDate = _fixture.Create<DateTime>();

            // Act
            var apprenticeship = _sut.CreateApprenticeship(_fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<DateTime>(), _fixture.Create<long>(), plannedStartDate, _fixture.Create<ApprenticeshipEmployerType>(), _fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<DateTime>());

            // Assert
            apprenticeship.PlannedStartDate.Should().Be(plannedStartDate);
        }

        [Test]
        public void Then_the_employer_type_on_approval_is_set()
        {
            // Arrange
            var employerType = _fixture.Create<ApprenticeshipEmployerType>();

            // Act
            var apprenticeship = _sut.CreateApprenticeship(_fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<DateTime>(), _fixture.Create<long>(), _fixture.Create<DateTime>(), employerType, _fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<DateTime>());

            // Assert
            apprenticeship.ApprenticeshipEmployerTypeOnApproval.Should().Be(employerType);
        }

        [Test]
        public void Then_the_course_name_is_set()
        {
            // Arrange
            var courseName = _fixture.Create<string>();

            // Act
            var apprenticeship = _sut.CreateApprenticeship(_fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<DateTime>(), _fixture.Create<long>(), _fixture.Create<DateTime>(), _fixture.Create<ApprenticeshipEmployerType>(), _fixture.Create<long>(), courseName, _fixture.Create<DateTime>());

            // Assert
            apprenticeship.CourseName.Should().Be(courseName);
        }

        [Test]
        public void Then_the_employment_start_date_is_set()
        {
            // Arrange
            var startDate = _fixture.Create<DateTime>();

            // Act
            var apprenticeship = _sut.CreateApprenticeship(_fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<DateTime>(), _fixture.Create<long>(), _fixture.Create<DateTime>(), _fixture.Create<ApprenticeshipEmployerType>(), _fixture.Create<long>(), _fixture.Create<string>(), startDate);

            // Assert
            apprenticeship.GetModel().EmploymentStartDate.Should().Be(startDate);
        }

        [TestCase(null, "2021-10-01", false)]
        [TestCase("2021-10-01", "2021-09-30", false)]
        [TestCase("2021-10-01", "2021-10-01", true)]
        [TestCase("2022-01-31", "2021-10-01", true)]
        [TestCase("2022-02-01", "2021-10-01", false)]
        [TestCase("2022-01-31", "2021-04-01", false)]
        public void Then_StartDatesAreEligible_for_a_phase_3_application_is_set_based_on_the_employment_start_date_and_commitment_start_date(DateTime? employmentStartDate, DateTime commitmentStartDate, bool eligibility)
        {
            // Arrange

            // Act
            var apprenticeship = _sut.CreateApprenticeship(_fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<DateTime>(), _fixture.Create<long>(), commitmentStartDate, _fixture.Create<ApprenticeshipEmployerType>(), _fixture.Create<long>(), _fixture.Create<string>(), employmentStartDate);

            // Assert
            apprenticeship.StartDatesAreEligible.Should().Be(eligibility);
        }
        
        [TestCase("2021-10-01")]
        [TestCase("2022-01-31")]
        public void Then_the_apprenticeship_phase_is_set_to_phase_3(DateTime startDate)
        {
            // Arrange

            // Act
            var apprenticeship = _sut.CreateApprenticeship(_fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<DateTime>(), _fixture.Create<long>(), _fixture.Create<DateTime>(), _fixture.Create<ApprenticeshipEmployerType>(), _fixture.Create<long>(), _fixture.Create<string>(), startDate);

            // Assert
            apprenticeship.Phase.Should().Be(Phase.Phase3);
        }
    }
}
