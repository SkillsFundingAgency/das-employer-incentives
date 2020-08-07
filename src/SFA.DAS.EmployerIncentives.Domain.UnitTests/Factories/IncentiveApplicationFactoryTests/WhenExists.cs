using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.Factories.IncentiveApplicationFactoryTests
{
    public class WhenExists
    {
        private IncentiveApplicationFactory _sut;
        private Fixture _fixture;
        private IncentiveApplicationModel _model;
        private Guid _id;

        [SetUp]
        public void Arrange()
        {
            _sut = new IncentiveApplicationFactory();
            _fixture = new Fixture();
            _id = _fixture.Create<Guid>();
            _model = _fixture.Create<IncentiveApplicationModel>();
        }

        [Test]
        public void Then_the_root_properties_are_mapped()
        {
            // Act
            var application = _sut.GetExisting(_id, _model);

            // Assert
            application.Should().BeEquivalentTo(_model, opt => opt.Excluding(x => x.ApprenticeshipModels));
        }

        [Test]
        public void Then_apprenticeships_are_mapped()
        {
            // Act
            var application = _sut.GetExisting(_id, _model);

            // Assert
            application.Apprenticeships.Should().BeEquivalentTo(_model.ApprenticeshipModels);
        }
    }
}
