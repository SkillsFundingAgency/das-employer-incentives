using System;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;
using System.Linq;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Domain.Factories;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.IncentiveApplicationTests
{
    public class WhenRemovingApprenticeshipsWithIneligibleStartDates
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
        public void Then_the_ineligibleApprenticeshipsAreRemoved()
        {
            // Arrange
            var apprenticeships = _fixture.CreateMany<Apprenticeship>(3).ToList();
            var ineligibleApprenticeship = new IncentiveApplicationFactory().CreateApprenticeship(_fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<DateTime>(), _fixture.Create<long>(), _fixture.Create<DateTime>(), _fixture.Create<ApprenticeshipEmployerType>(),
                _fixture.Create<long>(), _fixture.Create<string>(), new DateTime(2021, 03, 01));
            apprenticeships.Add(ineligibleApprenticeship);
            _sut.SetApprenticeships(apprenticeships);

            // Act
            _sut.RemoveApprenticeshipsWithIneligibleStartDates();

            // Assert
            _sut.Apprenticeships.Count.Should().Be(3);
            _sut.GetModel().ApprenticeshipModels.Count.Should().Be(3);
            _sut.Apprenticeships.Should().NotContain(ineligibleApprenticeship);
        }
    }
}
