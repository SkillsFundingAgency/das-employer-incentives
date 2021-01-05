using System;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.IncentiveApplicationTests
{
    public class WhenAnApprenticeshipAreSet
    {
        private IncentiveApplication _sut;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize(new IncentiveApplicationCustomization());

            _sut = _fixture.Create<IncentiveApplication>();
        }

        [Test]
        public void Then_the_apprenticeships_are_added()
        {
            // Arrange
            var apprenticeships = _fixture.CreateMany<Apprenticeship>().ToList();

            // Act
            _sut.SetApprenticeships(apprenticeships);

            // Assert
            _sut.Apprenticeships.Should().BeEquivalentTo(apprenticeships);
        }

        [Test]
        public void Then_the_apprenticeships_are_replaced()
        {
            // Arrange
            var originalApprenticeships = _fixture.CreateMany<Apprenticeship>(5).ToList();
            _sut.SetApprenticeships(originalApprenticeships);
            var newApprenticeships = _fixture.CreateMany<Apprenticeship>(2).ToList();

            // Act
            _sut.SetApprenticeships(newApprenticeships);

            // Assert
            _sut.Apprenticeships.Should().BeEquivalentTo(newApprenticeships);
        }

        [Test]
        public void Then_the_apprenticeship_start_dates_are_set_to_the_end_of_the_month()
        {
            // Arrange
            var apprenticeships = _fixture.CreateMany<Apprenticeship>(1).ToList();
            var originalStartDate = apprenticeships.First().PlannedStartDate;

            // Act
            _sut.SetApprenticeships(apprenticeships);

            // Assert
            var endOfStartMonth = new DateTime(originalStartDate.Year, originalStartDate.Month, DateTime.DaysInMonth(originalStartDate.Year, originalStartDate.Month));
            _sut.Apprenticeships.First().PlannedStartDate.Should().Be(endOfStartMonth);
        }
    }
}
