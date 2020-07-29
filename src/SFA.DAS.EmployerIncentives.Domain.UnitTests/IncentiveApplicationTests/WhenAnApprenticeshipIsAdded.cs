using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.IncentiveApplicationTests
{
    public class WhenAnApprenticeshipIsAdded
    {
        private IncentiveApplication _sut;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize(new IncentiveApplicationCustomization());

            _sut = new IncentiveApplicationFactory().CreateNew(_fixture.Create<Guid>(), _fixture.Create<long>(), _fixture.Create<long>());
        }

        [Test]
        public void Then_the_apprenticeship_is_added()
        {
            // Arrange
            var apprenticeship = _fixture.Create<Apprenticeship>();

            // Act
            _sut.AddApprenticeship(apprenticeship);

            // Assert
            _sut.Apprenticeships.Single().Should().Be(apprenticeship);
        }

        [Test]
        public void Then_the_apprenticeship_model_is_set()
        {
            // Arrange
            var apprenticeship = _fixture.Create<Apprenticeship>();

            // Act
            _sut.AddApprenticeship(apprenticeship);

            // Assert
            _sut.GetModel().ApprenticeshipModels.Single().Should().Be(apprenticeship.GetModel());
        }
    }
}
