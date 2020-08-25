using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.CreateIncentiveApplication;
using SFA.DAS.EmployerIncentives.Commands.SubmitIncentiveApplication;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.SubmitIncentiveApplication.Validators
{
    public class WhenValidatingSubmitIncentiveApplication
    {
        private SubmitIncentiveApplicationCommandValidator _sut;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sut = new SubmitIncentiveApplicationCommandValidator();
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_incentive_application_id_has_a_default_value()
        {
            //Arrange
            var command = new SubmitIncentiveApplicationCommand(default, _fixture.Create<long>(), _fixture.Create<DateTime>(), _fixture.Create<string>(), _fixture.Create<string>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_account_id_has_a_default_value()
        {
            //Arrange
            var command = new SubmitIncentiveApplicationCommand(_fixture.Create<Guid>(), default, _fixture.Create<DateTime>(), _fixture.Create<string>(), _fixture.Create<string>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_submitted_date_has_a_default_value()
        {
            //Arrange
            var command = new SubmitIncentiveApplicationCommand(_fixture.Create<Guid>(), _fixture.Create<long>(), default, _fixture.Create<string>(), _fixture.Create<string>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_submitted_user_email_has_a_default_value()
        {
            //Arrange
            var command = new SubmitIncentiveApplicationCommand(_fixture.Create<Guid>(), _fixture.Create<long>(), _fixture.Create<DateTime>(), default, _fixture.Create<string>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_submitted_user_name_has_a_default_value()
        {
            //Arrange
            var command = new SubmitIncentiveApplicationCommand(_fixture.Create<Guid>(), _fixture.Create<long>(), _fixture.Create<DateTime>(), _fixture.Create<string>(), default);

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
    }
}
