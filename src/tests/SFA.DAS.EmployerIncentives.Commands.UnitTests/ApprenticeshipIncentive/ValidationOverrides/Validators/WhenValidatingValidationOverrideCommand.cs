﻿using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.ValidationOverrides;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.ValidationOverrides.Validators
{
    public class WhenValidatingValidationOverrideCommand
    {
        private ValidationOverrideCommandValidator _sut;
        private long _accountLegalEntityId;
        private long _uln;
        private string _serviceRequestTaskId;
        private DateTime? _serviceRequestCreated;
        private string _decisionReference;

        private IEnumerable<ValidationOverrideStep> _validationOverrideSteps;

        private Func<ValidationOverrideCommand> ValidationOverrideCommand => 
            () => new ValidationOverrideCommand(_accountLegalEntityId, _uln, _serviceRequestTaskId, _decisionReference, _serviceRequestCreated, _validationOverrideSteps);

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _uln = _fixture.Create<long>();
            _accountLegalEntityId = _fixture.Create<long>();
            _serviceRequestTaskId = _fixture.Create<string>();
            _decisionReference = _fixture.Create<string>();
            _serviceRequestCreated = _fixture.Create<DateTime>();
            _validationOverrideSteps = new List<ValidationOverrideStep>() 
                {  
                    new ValidationOverrideStep(ValidationStep.IsInLearning.ToString(), DateTime.UtcNow.AddDays(1)),
                    new ValidationOverrideStep(ValidationStep.EmployedAtStartOfApprenticeship.ToString(), DateTime.UtcNow.AddDays(1)),
                };

            _sut = new ValidationOverrideCommandValidator();
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_ULN_has_a_default_value()
        {
            //Arrange
            _uln = default;
            var command = ValidationOverrideCommand();

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_AccountLegalEntityId_has_a_default_value()
        {
            //Arrange
            _accountLegalEntityId = default;
            var command = ValidationOverrideCommand();

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [TestCase("IsInLearning", true)]
        [TestCase("HasNoDataLocks", true)]
        [TestCase("HasDaysInLearning", true)]
        [TestCase("EmployedBeforeSchemeStarted", true)]
        [TestCase("EmployedAtStartOfApprenticeship", true)]        
        [TestCase("Random", false)]
        public async Task Then_the_command_is_invalid_when_the_ValidationType_is_invalid(string validationType, bool isValid)
        {
            //Arrange
            _validationOverrideSteps = new List<ValidationOverrideStep>()
            {
                new ValidationOverrideStep(validationType, DateTime.UtcNow.AddDays(1))
            };

            var command = ValidationOverrideCommand();

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(isValid ? 0 : 1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_ValidationOverrideStep_ExpiryDate_is_before_today()
        {
            //Arrange
            _validationOverrideSteps = new List<ValidationOverrideStep>()
            {
                new ValidationOverrideStep(ValidationStep.IsInLearning.ToString(), DateTime.UtcNow.AddDays(-1))
            };

            var command = ValidationOverrideCommand();

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_valid_when_the_ValidationOverrideStep_ExpiryDate_is_before_today_and_remove_is_true()
        {
            //Arrange
            _validationOverrideSteps = new List<ValidationOverrideStep>()
            {
                new ValidationOverrideStep(ValidationStep.IsInLearning.ToString(), DateTime.UtcNow.AddDays(-1), true)
            };

            var command = ValidationOverrideCommand();

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(0);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_ValidationOverrideSteps_is_null()
        {
            //Arrange
            _validationOverrideSteps = null;
            var command = ValidationOverrideCommand();

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }        
    }
}
