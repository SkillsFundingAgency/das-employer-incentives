using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PausePayments;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SendEmploymentCheckRequests;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi;
using SFA.DAS.EmployerIncentives.Commands.Services.EmploymentCheckApi;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.SendEmploymentCheckRequests.Handlers
{
    public class WhenSendingEmploymentCheckRequestsCommand
    {
        private SendEmploymentCheckRequestsCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockDomainRepository;
        private Mock<IEmploymentCheckService> _mockEmploymentCheckService;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize(new ApprenticeshipIncentiveCustomization());

            _mockDomainRepository = new Mock<IApprenticeshipIncentiveDomainRepository>();
            _mockEmploymentCheckService = new Mock<IEmploymentCheckService>();

            _sut = new SendEmploymentCheckRequestsCommandHandler(_mockDomainRepository.Object, _mockEmploymentCheckService.Object);
        }

        [Test]
        public async Task Then_the_employment_checks_are_sent_and_the_correlation_ids_are_saved()
        {
            // Arrange
            var command = _fixture.Create<SendEmploymentCheckRequestsCommand>();
            var apprenticeshipIncentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();
            apprenticeshipIncentive.SetStartDate(new DateTime(2021, 10, 1));
            apprenticeshipIncentive.RequestEmploymentChecks(true);

            _mockDomainRepository.Setup(x => x.Find(command.ApprenticeshipIncentiveId)).ReturnsAsync(apprenticeshipIncentive);
            var firstCheckCorrelationId = Guid.NewGuid();
            var secondCheckCorrelationId = Guid.NewGuid();
            _mockEmploymentCheckService.Setup(x => x.RegisterEmploymentCheck(apprenticeshipIncentive.EmploymentChecks.ElementAt(0), apprenticeshipIncentive)).ReturnsAsync(firstCheckCorrelationId);
            _mockEmploymentCheckService.Setup(x => x.RegisterEmploymentCheck(apprenticeshipIncentive.EmploymentChecks.ElementAt(1), apprenticeshipIncentive)).ReturnsAsync(secondCheckCorrelationId);

            // Act
            await _sut.Handle(command);

            // Assert
            _mockEmploymentCheckService.Verify(x => x.RegisterEmploymentCheck(apprenticeshipIncentive.EmploymentChecks.ElementAt(0), apprenticeshipIncentive), Times.Once);
            apprenticeshipIncentive.EmploymentChecks.ElementAt(0).CorrelationId.Should().Be(firstCheckCorrelationId);
            _mockEmploymentCheckService.Verify(x => x.RegisterEmploymentCheck(apprenticeshipIncentive.EmploymentChecks.ElementAt(1), apprenticeshipIncentive), Times.Once);
            apprenticeshipIncentive.EmploymentChecks.ElementAt(1).CorrelationId.Should().Be(secondCheckCorrelationId);
            _mockDomainRepository.Verify(x => x.Save(apprenticeshipIncentive));
        }
    }
}
