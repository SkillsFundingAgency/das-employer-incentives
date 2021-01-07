using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.EarningsResilienceCheck;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Queries.EarningsResilienceCheck;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.EarningsResilienceCheck.Handlers
{
    [TestFixture]
    public class WhenHandlingEarningsResilienceIncentivesCheckCommand
    {
        private EarningsResilienceIncentivesCheckCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _incentiveRepository;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _incentiveRepository = new Mock<IApprenticeshipIncentiveDomainRepository>();
            _sut = new EarningsResilienceIncentivesCheckCommandHandler(_incentiveRepository.Object);
            _fixture = new Fixture();
        }

        [Test]
        public async Task Then_applications_with_partial_apprenticeships_earnings_calculations_are_processed_for_the_eligibility_check()
        {
            //Arrange
            var command = new EarningsResilienceIncentivesCheckCommand();

            var incentives = new List<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>()
            {
                Domain.ApprenticeshipIncentives.ApprenticeshipIncentive.New(Guid.NewGuid(), Guid.NewGuid(), _fixture.Create<Account>(), _fixture.Create<Domain.ApprenticeshipIncentives.ValueTypes.Apprenticeship>(), _fixture.Create<DateTime>(), _fixture.Create<bool>())
            };
            _incentiveRepository.Setup(x => x.FindIncentivesWithoutPendingPayments()).ReturnsAsync(incentives);

            //Act
            await _sut.Handle(command);

            //Assert
            _incentiveRepository.Verify(x => x.Save(It.Is< Domain.ApprenticeshipIncentives.ApprenticeshipIncentive> (x => x.Id == incentives[0].Id)), Times.Once);

        }
    }
}

