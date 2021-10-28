using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.EarningsResilienceCheck;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.EarningsResilienceCheck.Handlers
{
    [TestFixture]
    public class WhenHandlingValidateIncompleteEarningsCalculationCommand
    {
        private ValidateIncompleteEarningsCalculationCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _incentiveRepository;
        private Fixture _fixture;
        private Mock<ICommandDispatcher> _commandDispatcher;

        [SetUp]
        public void Arrange()
        {
            _incentiveRepository = new Mock<IApprenticeshipIncentiveDomainRepository>();
            _commandDispatcher = new Mock<ICommandDispatcher>();
            _sut = new ValidateIncompleteEarningsCalculationCommandHandler(_incentiveRepository.Object, _commandDispatcher.Object);
            _fixture = new Fixture();
        }

        [Test]
        public async Task Then_the_earnings_calculated_flag_is_set_if_there_are_pending_payments_and_the_earnings_calculated_flag_not_set() 
        {
            // Arrange
            var validateEarningsCommand = _fixture.Create<ValidateIncompleteEarningsCalculationCommand>();

            var model = new ApprenticeshipIncentiveModel
            {
                PendingPaymentModels = _fixture.CreateMany<PendingPaymentModel>(2).ToList()
            };
            var incentive = Domain.ApprenticeshipIncentives.ApprenticeshipIncentive.Get(Guid.NewGuid(), model);

            _incentiveRepository.Setup(x => x.FindByApprenticeshipId(validateEarningsCommand.IncentiveApplicationApprenticeshipId)).ReturnsAsync(incentive);
        
            // Act
            await _sut.Handle(validateEarningsCommand);

            // Assert
            _commandDispatcher.Verify(x => x.Send(It.Is<CompleteEarningsCalculationCommand>(y => y.IncentiveApplicationApprenticeshipId == validateEarningsCommand.IncentiveApplicationApprenticeshipId), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
