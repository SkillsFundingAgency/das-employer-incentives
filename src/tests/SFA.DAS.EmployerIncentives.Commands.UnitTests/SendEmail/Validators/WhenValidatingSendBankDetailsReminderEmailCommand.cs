using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.SendEmail;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.SendEmail.Validators
{
    [TestFixture]
    public class WhenValidatingSendBankDetailsReminderEmailCommand
    {
        private SendBankDetailsReminderEmailCommandValidator _sut;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _sut = new SendBankDetailsReminderEmailCommandValidator();
        }

        [Test]
        public async Task Then_the_command_is_invalid_if_account_id_has_a_default_value()
        {
            // Arrange
            var command = new SendBankDetailsReminderEmailCommand(default, _fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<string>());

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count().Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_if_legal_entity_id_has_a_default_value()
        {
            // Arrange
            var command = new SendBankDetailsReminderEmailCommand(_fixture.Create<long>(), default, _fixture.Create<string>(), _fixture.Create<string>());

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count().Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_if_email_address_has_a_default_value()
        {
            // Arrange
            var command = new SendBankDetailsReminderEmailCommand(_fixture.Create<long>(), _fixture.Create<long>(), default, _fixture.Create<string>());

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count().Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_if_bank_details_url_has_a_default_value()
        {
            // Arrange
            var command = new SendBankDetailsReminderEmailCommand(_fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<string>(), default);

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count().Should().Be(1);
        }
    }
}
