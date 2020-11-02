using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.EarningsResilienceCheck;
using SFA.DAS.EmployerIncentives.Data.EarningsResilienceCheck;
using SFA.DAS.EmployerIncentives.Domain.EarningsResilienceCheck.Events;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using SFA.DAS.EmployerIncentives.Queries.EarningsResilienceCheck;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.EarningsResilienceCheck.Handlers
{
    [TestFixture]
    public class WhenHandlingEarningsResilienceCheckCommand
    {
        private EarningsResilienceCheckHandler _sut;
        private Mock<IEarningsResilienceCheckRepository> _repository;
        private Mock<IDomainEventDispatcher> _eventDispatcher;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _repository = new Mock<IEarningsResilienceCheckRepository>();
            _eventDispatcher = new Mock<IDomainEventDispatcher>();
            _sut = new EarningsResilienceCheckHandler(_repository.Object, _eventDispatcher.Object);
            _fixture = new Fixture();
        }

        [Test]
        public async Task Then_a_single_apprenticeship_is_processed_for_the_eligibility_check()
        {
            //Arrange
            var command = new EarningsResilienceCheckCommand();

            var applicationId = Guid.NewGuid();
            var applicationIds = new List<Guid> { applicationId };
            _repository.Setup(x => x.GetApplicationsWithoutEarningsCalculations()).ReturnsAsync(applicationIds);
            var applicationDetails = _fixture.Create<IncentiveApplicationModel>();                  
            _repository.Setup(x => x.GetApplicationDetail(applicationId)).ReturnsAsync(applicationDetails);

            //Act
            await _sut.Handle(command);

            //Assert
            _eventDispatcher.Verify(x => x.Send(It.Is<EarningsCalculationRequired>(x => x.Model == applicationDetails), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Then_multiple_apprenticeships_are_processed_for_the_eligibility_check()
        {
            //Arrange
            var command = new EarningsResilienceCheckCommand();

            var applicationId = Guid.NewGuid();
            var applicationIds = new List<Guid> { applicationId };
            _repository.Setup(x => x.GetApplicationsWithoutEarningsCalculations()).ReturnsAsync(applicationIds);
            var applicationDetails = _fixture.Create<IncentiveApplicationModel>();
            applicationDetails.ApprenticeshipModels = new Collection<ApprenticeshipModel>(_fixture.CreateMany<ApprenticeshipModel>(10).ToList());
            _repository.Setup(x => x.GetApplicationDetail(applicationId)).ReturnsAsync(applicationDetails);

            //Act
            await _sut.Handle(command);

            //Assert
            _eventDispatcher.Verify(x => x.Send(It.Is<EarningsCalculationRequired>(x => x.Model == applicationDetails), It.IsAny<CancellationToken>()), Times.Exactly(1));

        }

        [Test]
        public async Task Then_multiple_applications_and_apprenticeships_are_processed_for_the_eligibility_check()
        {
            //Arrange
            var command = new EarningsResilienceCheckCommand();

            var applicationId1 = Guid.NewGuid();
            var applicationId2 = Guid.NewGuid();
            var applicationIds = new List<Guid> { applicationId1, applicationId2 };
            _repository.Setup(x => x.GetApplicationsWithoutEarningsCalculations()).ReturnsAsync(applicationIds);
            var applicationDetails1 = _fixture.Create<IncentiveApplicationModel>();
            applicationDetails1.ApprenticeshipModels = new Collection<ApprenticeshipModel>(_fixture.CreateMany<ApprenticeshipModel>(3).ToList());
            _repository.Setup(x => x.GetApplicationDetail(applicationId1)).ReturnsAsync(applicationDetails1);
            var applicationDetails2 = _fixture.Create<IncentiveApplicationModel>();
            applicationDetails2.ApprenticeshipModels = new Collection<ApprenticeshipModel>(_fixture.CreateMany<ApprenticeshipModel>(4).ToList());
            _repository.Setup(x => x.GetApplicationDetail(applicationId2)).ReturnsAsync(applicationDetails2);

            //Act
            await _sut.Handle(command);

            //Assert
            _eventDispatcher.Verify(x => x.Send(It.IsAny<EarningsCalculationRequired>(), It.IsAny<CancellationToken>()), Times.Exactly(2));

        }
    }
}

