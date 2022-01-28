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
        public void No_existing_validationOverride_then_a_new_one_is_stored()
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
        public void Then_a_validationOverrideCreated_event_is_raised()
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
        public void Then_a_validationOverrideDeleted_event_is_raised()
        {
            // Arrange
            _sut = Sut(_sutModel);
            var validationOverrideStep = _fixture.Create<ValidationOverrideStep>();
            var serviceRequest = _fixture.Create<ServiceRequest>();
            // Act
            _sut.AddValidationOverride(validationOverrideStep, serviceRequest);
            // Assert
            _ = _sut.FlushEvents().Single() as ValidationOverrideDeleted;
        }

        [Test]
        public void When_there_is_existing_validation_override_it_is_replaced()
        {
            // Arrange
            var existingOverride = _fixture.Build<ValidationOverrideModel>()
                                                .Create();
            _sutModel.ValidationOverrideModels.Add(existingOverride);
            _sut = Sut(_sutModel);
            var id = _sut.Id;
            var validationOverrideStep = new ValidationOverrideStep(existingOverride.Step, _fixture.Create<DateTime>());
            var validationOverrideID = _sutModel.ValidationOverrideModels.Single().Id;
            
            var serviceRequest = _fixture.Create<ServiceRequest>();

            //Act
            _sut.AddValidationOverride(validationOverrideStep, serviceRequest);

            //Assert
            var ValidationOverideModel = _sut.GetModel().ValidationOverrideModels.Single();
            ValidationOverideModel.Step.Should().Be(validationOverrideStep.ValidationType);
            ValidationOverideModel.Id.Should().NotBe(validationOverrideID);
            ValidationOverideModel.ExpiryDate.Should().Be(validationOverrideStep.ExpiryDate);

        }
        [Test]
        public void When_existing_ValidationOverride_has_different_validation_type_to_new_validationOverride_both_are_stored()
        {
            // Arrange
            var existingOverride = _fixture.Build<ValidationOverrideModel>()
                                                .Create();
            _sutModel.ValidationOverrideModels.Add(existingOverride);
            _sut = Sut(_sutModel);
            var id = _sut.Id;
            var validationOverrideStep = _fixture.Create<ValidationOverrideStep>();
            var validationOverrideID = _sutModel.ValidationOverrideModels.Single().Id;

            var serviceRequest = _fixture.Create<ServiceRequest>();

            //Act
            _sut.AddValidationOverride(validationOverrideStep, serviceRequest);

            //Assert
            var ValidationOverideModel = _sut.GetModel().ValidationOverrideModels.FirstOrDefault();
            ValidationOverideModel.Step.Should().Be(validationOverrideStep.ValidationType);
            ValidationOverideModel.Id.Should().NotBe(validationOverrideID);
            ValidationOverideModel.ExpiryDate.Should().Be(validationOverrideStep.ExpiryDate);

        }
        private ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
 }

