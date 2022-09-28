using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SendEmploymentCheckRequests;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services.EmploymentCheckApi;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.SendEmploymentCheckRequests.Handlers
{
    public class WhenSendingEmploymentCheckRequestsCommand
    {
        private SendEmploymentCheckRequestsCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockDomainRepository;
        private Mock<IEmploymentCheckService> _mockEmploymentCheckService;
        private Mock<IOptions<ApplicationSettings>> _mockApplicationSettings;
        private Mock<IDateTimeService> _mockDateTimeService;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize(new ApprenticeshipIncentiveCustomization());
            _fixture.Customize<LearnerModel>(c => c.Without(x => x.LearningPeriods));

            _mockDateTimeService = new Mock<IDateTimeService>();
            _mockDateTimeService.Setup(m => m.Now()).Returns(DateTime.Now);
            _mockDateTimeService.Setup(m => m.UtcNow()).Returns(DateTime.UtcNow);

            _mockDomainRepository = new Mock<IApprenticeshipIncentiveDomainRepository>();
            _mockEmploymentCheckService = new Mock<IEmploymentCheckService>();
            _mockApplicationSettings = new Mock<IOptions<ApplicationSettings>>();

            _sut = new SendEmploymentCheckRequestsCommandHandler(_mockDomainRepository.Object, _mockEmploymentCheckService.Object);
        }

        [Test]
        public async Task Then_the_employment_check_is_sent_and_the_correlation_id_is_saved()
        {
            // Arrange            
            var apprenticeshipIncentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();
            var command = new SendEmploymentCheckRequestsCommand(apprenticeshipIncentive.Id, EmploymentCheckType.EmployedAtStartOfApprenticeship);
            apprenticeshipIncentive.SetStartDate(new DateTime(2021, 9, 1));
            var learner = new LearnerFactory().GetExisting(_fixture.Create<LearnerModel>());
            learner.SubmissionData.SetLearningData(new LearningData(true));
            apprenticeshipIncentive.RefreshLearner(learner, _mockDateTimeService.Object);

            _mockDomainRepository.Setup(x => x.Find(command.ApprenticeshipIncentiveId)).ReturnsAsync(apprenticeshipIncentive);
            var correlationId = Guid.NewGuid();
            _mockEmploymentCheckService.Setup(x => x.RegisterEmploymentCheck(It.Is<Domain.ApprenticeshipIncentives.EmploymentCheck>(y => y.CheckType == EmploymentCheckType.EmployedAtStartOfApprenticeship), apprenticeshipIncentive)).ReturnsAsync(correlationId);

            // Act
            await _sut.Handle(command);

            // Assert
            _mockEmploymentCheckService.Verify(x => x.RegisterEmploymentCheck(It.Is<Domain.ApprenticeshipIncentives.EmploymentCheck>(y => y.CheckType == EmploymentCheckType.EmployedAtStartOfApprenticeship), apprenticeshipIncentive), Times.Once);
            apprenticeshipIncentive.EmploymentChecks.Single(x => x.CheckType == EmploymentCheckType.EmployedAtStartOfApprenticeship).CorrelationId.Should().Be(correlationId);
            _mockDomainRepository.Verify(x => x.Save(apprenticeshipIncentive));
        }

        [Test]
        public async Task Then_the_employment_checks_are_not_sent_if_the_feature_toggle_switch_is_off()
        {
            // Arrange
            var command = _fixture.Create<SendEmploymentCheckRequestsCommand>();
            
            var configuration = new ApplicationSettings { EmploymentCheckEnabled = false };
            _mockApplicationSettings.Setup(x => x.Value).Returns(configuration);

            var handler = new SendEmploymentCheckRequestsCommandHandlerWithEmploymentCheckToggle(
                new SendEmploymentCheckRequestsCommandHandler(_mockDomainRepository.Object, _mockEmploymentCheckService.Object),
                _mockApplicationSettings.Object);

            // Act
            await handler.Handle(command);

            // Assert
            _mockDomainRepository.Verify(x => x.Find(It.IsAny<Guid>()), Times.Never);
            _mockEmploymentCheckService.Verify(x => x.RegisterEmploymentCheck(It.IsAny<Domain.ApprenticeshipIncentives.EmploymentCheck>(), It.IsAny<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>()), Times.Never);
            _mockDomainRepository.Verify(x => x.Save(It.IsAny<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>()), Times.Never());
        }
    }
}
