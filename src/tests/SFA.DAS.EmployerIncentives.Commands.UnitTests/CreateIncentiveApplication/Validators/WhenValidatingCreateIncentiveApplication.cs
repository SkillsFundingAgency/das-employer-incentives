using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.CreateIncentiveApplication;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.CreateIncentiveApplication.Validators
{
    public class WhenValidatingCreateIncentiveApplication
    {
        private CreateIncentiveApplicationCommandValidator _sut;
        
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sut = new CreateIncentiveApplicationCommandValidator();
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_incentive_application_id_has_a_default_value()
        {
            //Arrange
            var command = new CreateIncentiveApplicationCommand(default, _fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<IEnumerable<IncentiveApplicationApprenticeship>>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_accountId_has_a_default_value()
        {
            //Arrange
            var command = new CreateIncentiveApplicationCommand(_fixture.Create<Guid>(), default, _fixture.Create<long>(), _fixture.Create<IEnumerable<IncentiveApplicationApprenticeship>>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
        
        [Test]
        public async Task Then_the_command_is_invalid_when_the_accountLegalEntityId_has_a_default_value()
        {
            //Arrange
            var command = new CreateIncentiveApplicationCommand(_fixture.Create<Guid>(), _fixture.Create<long>(), default, _fixture.Create<IEnumerable<IncentiveApplicationApprenticeship>>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
        
        [Test]
        public async Task Then_the_command_is_invalid_when_the_apprenticeships_are_not_provided()
        {
            //Arrange
            var command = new CreateIncentiveApplicationCommand(_fixture.Create<Guid>(), _fixture.Create<long>(), _fixture.Create<long>(), default);

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_there_are_no_apprenticeships()
        {
            //Arrange
            var command = new CreateIncentiveApplicationCommand(_fixture.Create<Guid>(), _fixture.Create<long>(), _fixture.Create<long>(), new List<IncentiveApplicationApprenticeship>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_apprenticeship_apprenticeshipid_has_a_default_value()
        {
            //Arrange
            var apprenticeship = _fixture.Create<IncentiveApplicationApprenticeship>();
            apprenticeship.ApprenticeshipId = default;
            var command = new CreateIncentiveApplicationCommand(_fixture.Create<Guid>(), _fixture.Create<long>(), _fixture.Create<long>(), new List<IncentiveApplicationApprenticeship> { apprenticeship });

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_first_name_is_null()
        {
            //Arrange
            var apprenticeship = _fixture.Create<IncentiveApplicationApprenticeship>();
            apprenticeship.FirstName = null;
            var command = new CreateIncentiveApplicationCommand(_fixture.Create<Guid>(), _fixture.Create<long>(), _fixture.Create<long>(), new List<IncentiveApplicationApprenticeship> { apprenticeship });

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_first_name_is_empty()
        {
            //Arrange
            var apprenticeship = _fixture.Create<IncentiveApplicationApprenticeship>();
            apprenticeship.FirstName = string.Empty;
            var command = new CreateIncentiveApplicationCommand(_fixture.Create<Guid>(), _fixture.Create<long>(), _fixture.Create<long>(), new List<IncentiveApplicationApprenticeship> { apprenticeship });

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_last_name_is_null()
        {
            //Arrange
            var apprenticeship = _fixture.Create<IncentiveApplicationApprenticeship>();
            apprenticeship.LastName = null;
            var command = new CreateIncentiveApplicationCommand(_fixture.Create<Guid>(), _fixture.Create<long>(), _fixture.Create<long>(), new List<IncentiveApplicationApprenticeship> { apprenticeship });

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_last_name_is_empty()
        {
            //Arrange
            var apprenticeship = _fixture.Create<IncentiveApplicationApprenticeship>();
            apprenticeship.LastName = string.Empty;
            var command = new CreateIncentiveApplicationCommand(_fixture.Create<Guid>(), _fixture.Create<long>(), _fixture.Create<long>(), new List<IncentiveApplicationApprenticeship> { apprenticeship });

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_apprenticeship_date_of_birth_has_a_default_value()
        {
            //Arrange
            var apprenticeship = _fixture.Create<IncentiveApplicationApprenticeship>();
            apprenticeship.DateOfBirth = default;
            var command = new CreateIncentiveApplicationCommand(_fixture.Create<Guid>(), _fixture.Create<long>(), _fixture.Create<long>(), new List<IncentiveApplicationApprenticeship> { apprenticeship });

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_apprenticeship_uln_has_a_default_value()
        {
            //Arrange
            var apprenticeship = _fixture.Create<IncentiveApplicationApprenticeship>();
            apprenticeship.ULN = default;
            var command = new CreateIncentiveApplicationCommand(_fixture.Create<Guid>(), _fixture.Create<long>(), _fixture.Create<long>(), new List<IncentiveApplicationApprenticeship> { apprenticeship });

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_apprenticeship_planned_start_date_has_a_default_value()
        {
            //Arrange
            var apprenticeship = _fixture.Create<IncentiveApplicationApprenticeship>();
            apprenticeship.PlannedStartDate = default;
            var command = new CreateIncentiveApplicationCommand(_fixture.Create<Guid>(), _fixture.Create<long>(), _fixture.Create<long>(), new List<IncentiveApplicationApprenticeship> { apprenticeship });

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_provider_ukprn_has_a_default_value()
        {
            //Arrange
            var apprenticeship = _fixture.Create<IncentiveApplicationApprenticeship>();
            apprenticeship.UKPRN = null;
            var apprenticeships = new List<IncentiveApplicationApprenticeship> { apprenticeship };
            var command = new CreateIncentiveApplicationCommand(_fixture.Create<Guid>(), _fixture.Create<long>(), _fixture.Create<long>(), apprenticeships);
            
            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
    }
}
