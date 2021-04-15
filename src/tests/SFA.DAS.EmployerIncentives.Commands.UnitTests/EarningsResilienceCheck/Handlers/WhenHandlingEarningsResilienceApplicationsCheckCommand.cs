using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.EarningsResilienceCheck;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Commands.EarningsResilienceCheck;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.EarningsResilienceCheck.Handlers
{
    [TestFixture]
    public class WhenHandlingEarningsResilienceApplicationsCheckCommand
    {
        private EarningsResilienceApplicationsCheckCommandHandler _sut;
        private Mock<IIncentiveApplicationDomainRepository> _applicationRepository;
        private Fixture _fixture;
        private Mock<IDomainEventDispatcher> _domainEventDispatcher;

        [SetUp]
        public void Arrange()
        {
            _applicationRepository = new Mock<IIncentiveApplicationDomainRepository>();
            _domainEventDispatcher = new Mock<IDomainEventDispatcher>();
            _sut = new EarningsResilienceApplicationsCheckCommandHandler(_applicationRepository.Object, _domainEventDispatcher.Object);
            _fixture = new Fixture();
        }

        [Test]
        public async Task Then_a_single_apprenticeship_is_processed_for_the_eligibility_check()
        {
            //Arrange
            var command = new EarningsResilienceApplicationsCheckCommand();

            var application = _fixture.Build<IncentiveApplicationModel>().With(x => x.Status, IncentiveApplicationStatus.Submitted).Create();
            var applications = new List<IncentiveApplication> { IncentiveApplication.Get(application.Id, application) };
            var apprenticeships = _fixture.CreateMany<Domain.IncentiveApplications.Apprenticeship>(1).ToList();
            applications[0].SetApprenticeships(apprenticeships);
            _applicationRepository.Setup(x => x.FindIncentiveApplicationsWithoutEarningsCalculations()).ReturnsAsync(applications);
            
            //Act
            await _sut.Handle(command);

            //Assert
            _domainEventDispatcher.Verify(x => x.Send<Submitted>(It.Is<Submitted>(x => x.Model == applications[0].GetModel()), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Then_multiple_apprenticeships_are_processed_for_the_eligibility_check()
        {
            //Arrange
            var command = new EarningsResilienceApplicationsCheckCommand();

            var application = _fixture.Build<IncentiveApplicationModel>().With(x => x.Status, IncentiveApplicationStatus.Submitted).Create();
            var applications = new List<IncentiveApplication> { IncentiveApplication.Get(application.Id, application) };
            var apprenticeships = _fixture.CreateMany<Domain.IncentiveApplications.Apprenticeship>(3).ToList();
            applications[0].SetApprenticeships(apprenticeships);
            _applicationRepository.Setup(x => x.FindIncentiveApplicationsWithoutEarningsCalculations()).ReturnsAsync(applications);
            
            //Act
            await _sut.Handle(command);

            //Assert
            _domainEventDispatcher.Verify(x => x.Send<Submitted>(It.Is<Submitted>(x => x.Model == applications[0].GetModel()), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Then_multiple_applications_and_apprenticeships_are_processed_for_the_eligibility_check()
        {
            //Arrange
            var command = new EarningsResilienceApplicationsCheckCommand();

            var application1 = _fixture.Build<IncentiveApplicationModel>().With(x => x.Status, IncentiveApplicationStatus.Submitted).Create();
            var application2 = _fixture.Build<IncentiveApplicationModel>().With(x => x.Status, IncentiveApplicationStatus.Submitted).Create();
            var applications = new List<IncentiveApplication>
            {
                IncentiveApplication.Get(application1.Id, application1),
                IncentiveApplication.Get(application2.Id, application2)
            };

            applications[0].SetApprenticeships(_fixture.CreateMany<Domain.IncentiveApplications.Apprenticeship>(3).ToList());
            applications[1].SetApprenticeships(_fixture.CreateMany<Domain.IncentiveApplications.Apprenticeship>(4).ToList());
            _applicationRepository.Setup(x => x.FindIncentiveApplicationsWithoutEarningsCalculations()).ReturnsAsync(applications);
            
            //Act
            await _sut.Handle(command);

            //Assert
            _domainEventDispatcher.Verify(x => x.Send<Submitted>(It.Is<Submitted>(x => x.Model == applications[0].GetModel()), It.IsAny<CancellationToken>()), Times.Once);
            _domainEventDispatcher.Verify(x => x.Send<Submitted>(It.Is<Submitted>(x => x.Model == applications[1].GetModel()), It.IsAny<CancellationToken>()), Times.Once);
        }

    }
}

