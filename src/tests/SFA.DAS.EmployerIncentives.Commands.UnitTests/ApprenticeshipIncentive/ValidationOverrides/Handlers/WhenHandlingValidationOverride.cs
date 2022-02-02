using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.ValidationOverrides;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.ValidationOverrides.Handlers
{
    public class WhenHandlingValidationOverride
    {
        private ValidationOverrideCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockDomainRepository;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize(new ApprenticeshipIncentiveCustomization());

            _mockDomainRepository = new Mock<IApprenticeshipIncentiveDomainRepository>();

            _sut = new ValidationOverrideCommandHandler(_mockDomainRepository.Object);
        }
        [Test]
        public async Task Then_the_KeyNotFoundException_is_thrown_when_no_incentive_is_found_for_this_uln_and_accountlegalentityid()
        {
            var command = _fixture.Create<ValidationOverrideCommand>();

            Func<Task> act = async () => await _sut.Handle(command);

            act.Should().Throw<KeyNotFoundException>();
        }
        [Test]
        public async Task Then_the_ValidationOverride_flag_for_this_incentive_is_set_on()
        {
            // Arrange
            var command = CreateValidationOverrideCommandWithActionPause();
            var apprenticeshipIncentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();

            _mockDomainRepository
                .Setup(x => x.FindByUlnWithinAccountLegalEntity(command.ULN, command.AccountLegalEntityId))
                .ReturnsAsync(apprenticeshipIncentive);

            // Act
            await _sut.Handle(command);

            // Assert
            _mockDomainRepository.Verify(x => x.Save(apprenticeshipIncentive), Times.Once);
        }
        private ValidationOverrideCommand CreateValidationOverrideCommandWithActionPause()
        {
            return new ValidationOverrideCommand(
                _fixture.Create<long>(),
                _fixture.Create<long>(),
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<DateTime>(),
                new List<ValidationOverrideStep>()
                {
                new ValidationOverrideStep(_fixture.Create<string>(), _fixture.Create<DateTime>()),
                new ValidationOverrideStep(_fixture.Create<string>(), _fixture.Create<DateTime>())
                }  
                );
        }

    }
}
