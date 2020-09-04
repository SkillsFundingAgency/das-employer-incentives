using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.Factories.ApprenticeshipIncentiveTests
{
    public class WhenExists
    {
        private ApprenticeshipIncentiveFactory _sut;
        private Fixture _fixture;
        private ApprenticeshipIncentiveModel _model;
        private Guid _id;

        [SetUp]
        public void Arrange()
        {
            _sut = new ApprenticeshipIncentiveFactory();
            _fixture = new Fixture();
            _id = _fixture.Create<Guid>();
            _model = _fixture.Create<ApprenticeshipIncentiveModel>();
        }

        [Test]
        public void Then_the_root_properties_are_mapped()
        {
            // Act
            var incentive = _sut.GetExisting(_id, _model);

            // Assert
            incentive.Should().BeEquivalentTo(_model);
        }
    }
}
