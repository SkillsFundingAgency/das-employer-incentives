using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.VendorBlock;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.VendorBlock.Validators
{
    [TestFixture]
    public class WhenValidatingBlockAccountLegalEntityForPaymentsCommand
    {
        private BlockAccountLegalEntityForPaymentsCommandValidator _sut;
        private DateTime _vendorBlockEndDate;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _vendorBlockEndDate = DateTime.Now.AddMonths(1);
            _sut = new BlockAccountLegalEntityForPaymentsCommandValidator();
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_vendorId_has_a_default_value()
        {
            //Arrange
            var command = new BlockAccountLegalEntityForPaymentsCommand(default, _vendorBlockEndDate, _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<DateTime>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_vendorBlockEndDate_has_a_default_value()
        {
            //Arrange
            var command = new BlockAccountLegalEntityForPaymentsCommand(_fixture.Create<string>(), default, _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<DateTime>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_vendorBlockEndDate_is_in_the_past()
        {
            //Arrange
            var command = new BlockAccountLegalEntityForPaymentsCommand(_fixture.Create<string>(), DateTime.Now.AddSeconds(-1), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<DateTime>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_serviceRequestTaskId_has_a_default_value()
        {
            //Arrange
            var command = new BlockAccountLegalEntityForPaymentsCommand(_fixture.Create<string>(), _vendorBlockEndDate, default, _fixture.Create<string>(), _fixture.Create<DateTime>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_serviceRequestDecisionReference_has_a_default_value()
        {
            //Arrange
            var command = new BlockAccountLegalEntityForPaymentsCommand(_fixture.Create<string>(), _vendorBlockEndDate, _fixture.Create<string>(), default, _fixture.Create<DateTime>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
    }
}
