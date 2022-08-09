using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.EmploymentCheck;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.EmploymentCheck
{
    [TestFixture]
    public class WhenHandlingRefreshEmploymentCheckCommand
    {
        private RefreshEmploymentCheckCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockIncentiveDomainRespository;
        private Mock<IDateTimeService> _mockDateTimeService;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockDateTimeService = new Mock<IDateTimeService>();
            _mockDateTimeService.Setup(m => m.Now()).Returns(DateTime.Now);
            _mockDateTimeService.Setup(m => m.UtcNow()).Returns(DateTime.UtcNow);


            _mockIncentiveDomainRespository = new Mock<IApprenticeshipIncentiveDomainRepository>();

            _sut = new RefreshEmploymentCheckCommandHandler(_mockIncentiveDomainRespository.Object, _mockDateTimeService.Object);
        }

        [Test]
        public async Task Then_a_refresh_of_the_employment_check_is_requested_if_the_apprentices_have_a_learning_record()
        {
            // Arrange
            var model = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(x => x.EmploymentCheckModels, _fixture.CreateMany<EmploymentCheckModel>(2).ToList())
                .With(x => x.StartDate, new DateTime(2021, 10, 01))
                .With(x => x.Phase, new IncentivePhase(Phase.Phase2))
                .Create();

            var serviceRequest = _fixture.Create<ServiceRequest>();

            var incentive = Domain.ApprenticeshipIncentives.ApprenticeshipIncentive.Get(model.Id, model);
            
            _mockIncentiveDomainRespository
                .Setup(x => x.FindByUlnWithinAccountLegalEntity(
                    model.Apprenticeship.UniqueLearnerNumber, 
                    model.Account.AccountLegalEntityId))
                .ReturnsAsync(incentive);

            // Act
            await _sut.Handle(new RefreshEmploymentCheckCommand(
                model.Account.AccountLegalEntityId,
                model.Apprenticeship.UniqueLearnerNumber,
                serviceRequest.TaskId,
                serviceRequest.DecisionReference,
                serviceRequest.Created
                ));

            // Assert
            var createdEvent = incentive.FlushEvents().OfType<EmploymentChecksCreated>().Single();
            createdEvent.Should().NotBeNull();
            createdEvent.ApprenticeshipIncentiveId.Should().Be(model.Id);
            createdEvent.ServiceRequest.TaskId.Should().Be(serviceRequest.TaskId);
            createdEvent.ServiceRequest.DecisionReference.Should().Be(serviceRequest.DecisionReference);
            createdEvent.ServiceRequest.Created.Should().Be(serviceRequest.Created);

            _mockIncentiveDomainRespository.Verify(m => m.Save(incentive), Times.Once);
        }

        [Test]
        public async Task Then_a_refresh_of_the_employment_check_is_not_requested_if_the_apprentice_does_not_exist()
        {
            var model = _fixture.Build<ApprenticeshipIncentiveModel>()
                 .With(x => x.EmploymentCheckModels, _fixture.CreateMany<EmploymentCheckModel>(2).ToList())
                 .With(x => x.StartDate, new DateTime(2021, 10, 01))
                 .With(x => x.Phase, new IncentivePhase(Phase.Phase2))
                 .Create();

            var serviceRequest = _fixture.Create<ServiceRequest>();

            var incentive = Domain.ApprenticeshipIncentives.ApprenticeshipIncentive.Get(model.Id, model);

            _mockIncentiveDomainRespository
                .Setup(x => x.FindByUlnWithinAccountLegalEntity(
                    model.Apprenticeship.UniqueLearnerNumber,
                    model.Account.AccountLegalEntityId))
                .Returns(Task.FromResult<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>(null));

            // Act
            await _sut.Handle(new RefreshEmploymentCheckCommand(
                model.Account.AccountLegalEntityId,
                model.Apprenticeship.UniqueLearnerNumber,
                serviceRequest.TaskId,
                serviceRequest.DecisionReference,
                serviceRequest.Created
                ));

            // Assert
            var createdEvent = incentive.FlushEvents().SingleOrDefault() as EmploymentChecksCreated;
            createdEvent.Should().BeNull();

            _mockIncentiveDomainRespository.Verify(m => m.Save(incentive), Times.Never);
        }
    }
}
