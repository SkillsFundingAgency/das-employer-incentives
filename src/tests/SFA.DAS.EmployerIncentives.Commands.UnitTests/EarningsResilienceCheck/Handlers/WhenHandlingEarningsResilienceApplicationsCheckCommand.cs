using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.EarningsResilienceCheck;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Queries.EarningsResilienceCheck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.EarningsResilienceCheck.Handlers
{
    [TestFixture]
    public class WhenHandlingEarningsResilienceApplicationsCheckCommand
    {
        private EarningsResilienceApplicationsCheckCommandHandler _sut;
        private Mock<IIncentiveApplicationDomainRepository> _applicationRepository;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _applicationRepository = new Mock<IIncentiveApplicationDomainRepository>();
            _sut = new EarningsResilienceApplicationsCheckCommandHandler(_applicationRepository.Object);
            _fixture = new Fixture();
        }

        [Test]
        public async Task Then_a_single_apprenticeship_is_processed_for_the_eligibility_check()
        {
            //Arrange
            var command = new EarningsResilienceApplicationsCheckCommand();

            var applications = new List<IncentiveApplication> { IncentiveApplication.New(Guid.NewGuid(), _fixture.Create<long>(), _fixture.Create<long>()) }; ;
            var apprenticeships = _fixture.CreateMany<Domain.IncentiveApplications.Apprenticeship>(1).ToList();
            applications[0].SetApprenticeships(apprenticeships);
            _applicationRepository.Setup(x => x.FindIncentiveApplicationsWithoutEarningsCalculations()).ReturnsAsync(applications);
            
            //Act
            await _sut.Handle(command);

            //Assert
            _applicationRepository.Verify(x => x.Save(It.Is<IncentiveApplication>(x => x.Id == applications[0].Id)), Times.Once);
        }

        [Test]
        public async Task Then_multiple_apprenticeships_are_processed_for_the_eligibility_check()
        {
            //Arrange
            var command = new EarningsResilienceApplicationsCheckCommand();

            var applications = new List<IncentiveApplication> { IncentiveApplication.New(Guid.NewGuid(), _fixture.Create<long>(), _fixture.Create<long>()) }; ;
            var apprenticeships = _fixture.CreateMany<Domain.IncentiveApplications.Apprenticeship>(3).ToList();
            applications[0].SetApprenticeships(apprenticeships);
            _applicationRepository.Setup(x => x.FindIncentiveApplicationsWithoutEarningsCalculations()).ReturnsAsync(applications);
            
            //Act
            await _sut.Handle(command);

            //Assert
            _applicationRepository.Verify(x => x.Save(It.Is<IncentiveApplication>(x => x.Id == applications[0].Id)), Times.Once);
        }

        [Test]
        public async Task Then_multiple_applications_and_apprenticeships_are_processed_for_the_eligibility_check()
        {
            //Arrange
            var command = new EarningsResilienceApplicationsCheckCommand();

            var applications = new List<IncentiveApplication>
            {
                IncentiveApplication.New(Guid.NewGuid(), _fixture.Create<long>(), _fixture.Create<long>()),
                IncentiveApplication.New(Guid.NewGuid(), _fixture.Create<long>(), _fixture.Create<long>())
            };

            applications[0].SetApprenticeships(_fixture.CreateMany<Domain.IncentiveApplications.Apprenticeship>(3).ToList());
            applications[1].SetApprenticeships(_fixture.CreateMany<Domain.IncentiveApplications.Apprenticeship>(4).ToList());
            _applicationRepository.Setup(x => x.FindIncentiveApplicationsWithoutEarningsCalculations()).ReturnsAsync(applications);
            
            //Act
            await _sut.Handle(command);

            //Assert
            _applicationRepository.Verify(x => x.Save(It.Is<IncentiveApplication>(x => x.Id == applications[0].Id)), Times.Once);
            _applicationRepository.Verify(x => x.Save(It.Is<IncentiveApplication>(x => x.Id == applications[1].Id)), Times.Once);
        }

    }
}

