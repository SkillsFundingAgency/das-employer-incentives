using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.EarningsResilienceCheck;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.EarningsResilienceCheck.Handlers
{
    [TestFixture]
    public class WhenHandlingIncompleteEarningsCalculationCheckCommand
    {
        private IncompleteEarningsCalculationCheckCommandHandler _sut;
        private Mock<IIncentiveApplicationDomainRepository> _applicationRepository;
        private Fixture _fixture;
        private Mock<ICommandDispatcher> _commandDispatcher;

        [SetUp]
        public void Arrange()
        {
            _applicationRepository = new Mock<IIncentiveApplicationDomainRepository>();
            _commandDispatcher = new Mock<ICommandDispatcher>();
            _sut = new IncompleteEarningsCalculationCheckCommandHandler(_applicationRepository.Object, _commandDispatcher.Object);
            _fixture = new Fixture();
        }

        [Test]
        public async Task Then_earnings_are_validated_for_any_apprentices_that_have_earnings_calculated_set_to_false()
        {
            // Arrange
            var apprentices = new List<Apprenticeship> 
            {
                Apprenticeship.Create(_fixture.Build<ApprenticeshipModel>().With(x => x.EarningsCalculated, false).Create()),
                Apprenticeship.Create(_fixture.Build<ApprenticeshipModel>().With(x => x.EarningsCalculated, false).Create()),
                Apprenticeship.Create(_fixture.Build<ApprenticeshipModel>().With(x => x.EarningsCalculated, false).Create())
            };
            var application = _fixture.Build<IncentiveApplicationModel>().With(x => x.Status, IncentiveApplicationStatus.Submitted).Create();
            var applications = new List<IncentiveApplication> { IncentiveApplication.Get(application.Id, application) };
            applications[0].SetApprenticeships(apprentices);

            _applicationRepository.Setup(x => x.FindIncentiveApplicationsWithoutEarningsCalculations()).ReturnsAsync(applications);

            // Act
            await _sut.Handle(new IncompleteEarningsCalculationCheckCommand());

            // Assert
            _commandDispatcher.Verify(x => x.Send(It.IsAny<ValidateIncompleteEarningsCalculationCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
