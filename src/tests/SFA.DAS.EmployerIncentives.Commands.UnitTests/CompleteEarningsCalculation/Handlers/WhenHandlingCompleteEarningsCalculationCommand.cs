using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.CompleteEarningsCalculation;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;
using Apprenticeship = SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Apprenticeship;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.CompleteEarningsCalculation.Handlers
{
    public class WhenHandlingCompleteEarningsCalculationCommand
    {
        private CompleteEarningsCalculationCommandHandler _sut;
        private Mock<IQueryRepository<IncentiveApplicationApprenticeship>> _mockQueryRepository;
        private Mock<IIncentiveApplicationDomainRepository> _mockIncentiveApplicationDomainRepository;
        private Fixture _fixture;
        private Guid _apprenticeshipIncentiveId;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize(new IncentiveApplicationCustomization());
            _apprenticeshipIncentiveId = _fixture.Create<Guid>();

            _mockQueryRepository = new Mock<IQueryRepository<IncentiveApplicationApprenticeship>>();
            
            _mockIncentiveApplicationDomainRepository = new Mock<IIncentiveApplicationDomainRepository>();

            _sut = new CompleteEarningsCalculationCommandHandler(_mockQueryRepository.Object, _mockIncentiveApplicationDomainRepository.Object);
        }

        [Test]
        public async Task Then_the_earnings_calculated_flag_is_updated_for_matching_apprenticeship()
        {
            //Arrange
            var incentiveApplication = _fixture.Create<IncentiveApplication>();
            var firstApprenticeship = incentiveApplication.Apprenticeships.First();
            firstApprenticeship.Id = _apprenticeshipIncentiveId;
            var apprenticeshipModel = _fixture.Build<ApprenticeshipModel>().With(x => x.Id, _apprenticeshipIncentiveId)
                .Without(x => x.EarningsCalculated).Create();
            var apprenticeshipModel2 = _fixture.Create<ApprenticeshipModel>();

            var domainIncentiveApplication = _fixture.Create<Domain.IncentiveApplications.IncentiveApplication>();

            domainIncentiveApplication.SetApprenticeships(new List<Apprenticeship> { Apprenticeship.Create(apprenticeshipModel), Apprenticeship.Create(apprenticeshipModel2) });
  
            var command = new CompleteEarningsCalculationCommand(incentiveApplication.AccountId, firstApprenticeship.Id,
                firstApprenticeship.ApprenticeshipId, _apprenticeshipIncentiveId);

            _mockQueryRepository.Setup(x => x.Get(It.IsAny<Expression<Func<Data.Models.IncentiveApplicationApprenticeship, bool>>>()))
                .ReturnsAsync(firstApprenticeship);

            _mockIncentiveApplicationDomainRepository.Setup(x => x
            .Find(firstApprenticeship.IncentiveApplicationId))
                .ReturnsAsync(domainIncentiveApplication);

            // Act
            await _sut.Handle(command);

            domainIncentiveApplication.Apprenticeships.First().EarningsCalculated.Should().BeTrue();
            domainIncentiveApplication.Apprenticeships.Last().EarningsCalculated.Should().BeFalse();
        }
    }
}