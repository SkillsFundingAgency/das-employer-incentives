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

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    internal class WhenValidationOverride
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
        public void Then_a_new_ValidationOverride_is_added_when_it_does_not_already_exist()
        {
            // Arrange
            _sut = Sut(_sutModel);
            var id = _sut.Id;
            var validationOverrideStep = _fixture.Create<ValidationOverrideStep>();
            var serviceRequest = _fixture.Create<ServiceRequest>();

            // Act
            _sut.AddValidationOverride(validationOverrideStep, serviceRequest);

            // Assert
            _sut.GetModel().ValidationOverrideModels.Single().ApprenticeshipIncentiveId.Should().Be(id);
            _sut.GetModel().ValidationOverrideModels.Single().Step.Should().Be(validationOverrideStep.ValidationType);
            _sut.GetModel().ValidationOverrideModels.Single().ExpiryDate.Should().Be(validationOverrideStep.ExpiryDate);
        }

        [Test]
        public void Then_a_validationOverrideCreated_event_is_raised_when_a_new_ValidationOverride_is_added()
        {
            // Arrange
            _sut = Sut(_sutModel);
            var id = _sut.Id;
            var validationOverrideStep = _fixture.Create<ValidationOverrideStep>();
            var serviceRequest = _fixture.Create<ServiceRequest>();

            // Act
            _sut.AddValidationOverride(validationOverrideStep, serviceRequest);

            // Assert
            var raisedEvent = _sut.FlushEvents().Single() as ValidationOverrideCreated;
            raisedEvent.ApprenticeshipIncentiveId.Should().Be(id);
            raisedEvent.ValidationOverrideId.Should().NotBe(Guid.Empty);
            raisedEvent.ValidationOverrideStep.Should().Be(validationOverrideStep);
            raisedEvent.ServiceRequest.Should().Be(serviceRequest);
        }

        [Test]
        public void Then_a_ValidationOverrideDeleted_event_is_raised_when_an_existing_ValidationOverride_is_replaced()
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
            _sut.AddValidationOverride(validationOverrideStep, serviceRequest);

            // Assert
            var raisedEvent = _sut.FlushEvents().Single(e => e is ValidationOverrideDeleted) as ValidationOverrideDeleted;
            raisedEvent.Should().NotBeNull();
            raisedEvent.ValidationOverrideId.Should().Be(existingOverride.Id);
            raisedEvent.ApprenticeshipIncentiveId.Should().Be(existingOverride.ApprenticeshipIncentiveId);
        }

        [Test]
        public void Then_a_ValidationOverrideCreated_event_is_raised_when_an_existing_ValidationOverride_is_replaced()
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
            _sut.AddValidationOverride(validationOverrideStep, serviceRequest);

            // Assert
            var raisedEvent = _sut.FlushEvents().Single(e => e is ValidationOverrideCreated) as ValidationOverrideCreated;
            raisedEvent.Should().NotBeNull();
            raisedEvent.ValidationOverrideId.Should().NotBe(existingOverride.Id);
            raisedEvent.ApprenticeshipIncentiveId.Should().Be(existingOverride.ApprenticeshipIncentiveId);
            raisedEvent.ValidationOverrideStep.Should().Be(validationOverrideStep);
            raisedEvent.ServiceRequest.Should().Be(serviceRequest);
        }

        [Test]
        public void Then_an_existing_validation_override_is_replaced_when_the_new_override_is_the_same_type()
        {
            // Arrange
            var existingOverride = _fixture
                .Build<ValidationOverrideModel>()
                .With(p => p.ApprenticeshipIncentiveId, _sutModel.Id)
                .Create();

            _sutModel.ValidationOverrideModels.Add(existingOverride);

            _sut = Sut(_sutModel);
            var id = _sut.Id;
            var validationOverrideStep = new ValidationOverrideStep(existingOverride.Step, _fixture.Create<DateTime>());
            var validationOverrideId = _sutModel.ValidationOverrideModels.Single().Id;
            
            var serviceRequest = _fixture.Create<ServiceRequest>();

            //Act
            _sut.AddValidationOverride(validationOverrideStep, serviceRequest);

            //Assert
            var ValidationOverideModel = _sut.GetModel().ValidationOverrideModels.Single();
            ValidationOverideModel.Step.Should().Be(validationOverrideStep.ValidationType);
            ValidationOverideModel.Id.Should().NotBe(validationOverrideId);
            ValidationOverideModel.ApprenticeshipIncentiveId.Should().Be(id);
            ValidationOverideModel.ExpiryDate.Should().Be(validationOverrideStep.ExpiryDate);

        }
        [Test]
        public void When_existing_ValidationOverride_has_different_validation_type_to_new_validationOverride_both_are_stored()
        {
            // Arrange
            var existingOverride = _fixture
                 .Build<ValidationOverrideModel>()
                 .With(p => p.ApprenticeshipIncentiveId, _sutModel.Id)
                 .Create();

            _sutModel.ValidationOverrideModels.Add(existingOverride);
            
            _sut = Sut(_sutModel);
            var validationOverrideStep = _fixture.Create<ValidationOverrideStep>();

            var serviceRequest = _fixture.Create<ServiceRequest>();

            //Act
            _sut.AddValidationOverride(validationOverrideStep, serviceRequest);

            //Assert
            _sut.GetModel().ValidationOverrideModels.Count.Should().Be(2);

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

