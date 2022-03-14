using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests.ValidationOverrideTests
{
    internal class WhenRemoveValidationOverride
    {
        private Fixture _fixture;
        private ApprenticeshipIncentive _sut;
        private ApprenticeshipIncentiveModel _sutModel;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sutModel = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .Without(i => i.ValidationOverrideModels)
                .Create();
        }        

        [Test]
        public void Then_an_existing_ValidationOverride_is_removed_when_it_already_exist()
        {
            // Arrange
            var existingOverride = _fixture
                  .Build<ValidationOverrideModel>()
                  .With(p => p.ApprenticeshipIncentiveId, _sutModel.Id)
                  .Create();

            _sutModel.ValidationOverrideModels.Add(existingOverride);

            _sut = Sut(_sutModel);
            var validationOverrideStep = new ValidationOverrideStep(existingOverride.Step, _fixture.Create<DateTime>());

            var serviceRequest = _fixture.Create<ServiceRequest>();

            // Act
            _sut.RemoveValidationOverride(validationOverrideStep, serviceRequest);

            // Assert
            _sut.GetModel().ValidationOverrideModels.Count.Should().Be(0);
        }

        [Test]
        public void Then_an_existing_ValidationOverride_is_not_removed_when_it_does_not_already_exist()
        {
            // Arrange
            var existingOverride = _fixture
                   .Build<ValidationOverrideModel>()
                   .With(p => p.ApprenticeshipIncentiveId, _sutModel.Id)
                   .Create();

            _sutModel.ValidationOverrideModels.Add(existingOverride);

            _sut = Sut(_sutModel);
            var validationOverrideStep = new ValidationOverrideStep(_fixture.Create<string>(), _fixture.Create<DateTime>());

            var serviceRequest = _fixture.Create<ServiceRequest>();

            // Act
            _sut.RemoveValidationOverride(validationOverrideStep, serviceRequest);

            // Assert
            _sut.GetModel().ValidationOverrideModels.Single().Id.Should().Be(existingOverride.Id);
        }

        [Test]
        public void Then_a_ValidationOverrideDeleted_event_is_raised_when_an_existing_ValidationOverride_is_removed()
        {
            // Arrange
            var existingOverride = _fixture
                .Build<ValidationOverrideModel>()
                .With(p => p.ApprenticeshipIncentiveId, _sutModel.Id)
                .Create();

            _sutModel.ValidationOverrideModels.Add(existingOverride);

            _sut = Sut(_sutModel);

            var validationOverrideStep = new ValidationOverrideStep(existingOverride.Step, _fixture.Create<DateTime>());
            var serviceRequest = _fixture.Create<ServiceRequest>();

            // Act
            _sut.RemoveValidationOverride(validationOverrideStep, serviceRequest);

            // Assert
            var raisedEvent = _sut.FlushEvents().Single(e => e is ValidationOverrideDeleted) as ValidationOverrideDeleted;
            raisedEvent.Should().NotBeNull();
            raisedEvent.ValidationOverrideId.Should().Be(existingOverride.Id);
            raisedEvent.ApprenticeshipIncentiveId.Should().Be(existingOverride.ApprenticeshipIncentiveId);
        }

        private ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
 }

