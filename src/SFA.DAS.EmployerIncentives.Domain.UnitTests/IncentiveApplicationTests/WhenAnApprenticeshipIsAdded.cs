using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;

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

            _sut = new IncentiveApplicationFactory().CreateNew(_fixture.Create<Guid>(), _fixture.Create<long>(), _fixture.Create<long>());
        }

        [Test]
        public void Then_the_apprenticeship_is_added()
        {
            // Arrange
            var apprenticeship = Apprenticeship.Create(_fixture.Create<ApprenticeshipModel>());

            // Act
            _sut.AddApprenticeship(apprenticeship);

            // Assert
            _sut.Apprenticeships.Single().Id.Should().Be(apprenticeship.Id);
        }

        [Test]
        public void Then_the_apprenticeship_model_is_set()
        {
            // Arrange
            var model = _fixture.Create<ApprenticeshipModel>();
            var apprenticeship = Apprenticeship.Create(model);

            // Act
            _sut.AddApprenticeship(apprenticeship);

            // Assert
            _sut.GetModel().ApprenticeshipModels.Single().Should().Be(model);
        }
    }
}
