using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Data.IncentiveApplication;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.Persistence.ApprenticeshipIncentiveDomainRepository
{
    [TestFixture]
    public class WhenFindingApprenticeshipsIncentivesForLegalEntity
    {
        private Commands.Persistence.ApprenticeshipIncentiveDomainRepository _sut;
        private Mock<IApprenticeshipIncentiveDataRepository> _mockApprenticeshipIncentiveDataRepository;
        private Mock<IPaymentDataRepository> _mockPaymentDataRepository;
        private Mock<IApprenticeshipIncentiveFactory> _mockApprenticeshipIncentiveFactory;
        private Mock<IDomainEventDispatcher> _mockDomainEventDispatcher;
        private long _accountLegalEntityId;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize(new ApprenticeshipIncentiveCustomization());

            _accountLegalEntityId = _fixture.Create<long>();

            _mockApprenticeshipIncentiveDataRepository = new Mock<IApprenticeshipIncentiveDataRepository>();
            _mockPaymentDataRepository = new Mock<IPaymentDataRepository>();
            _mockApprenticeshipIncentiveFactory = new Mock<IApprenticeshipIncentiveFactory>();
            _mockDomainEventDispatcher = new Mock<IDomainEventDispatcher>();

            _sut = new Commands.Persistence.ApprenticeshipIncentiveDomainRepository(
                _mockApprenticeshipIncentiveDataRepository.Object,
                _mockPaymentDataRepository.Object,
                _mockApprenticeshipIncentiveFactory.Object,
                _mockDomainEventDispatcher.Object);
        }

        [Test]
        public async Task Then_apprenticeship_incentives_for_the_account_legal_entity_are_returned()
        {
            // Arrange
            var incentives = _fixture.CreateMany<ApprenticeshipIncentiveModel>(5).ToList();
            _mockApprenticeshipIncentiveDataRepository.Setup(x => x.FindByAccountLegalEntityId(_accountLegalEntityId)).ReturnsAsync(incentives);

            // Act
            var result = await _sut.FindByAccountLegalEntity(_accountLegalEntityId);

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(incentives.Count());
        }

        [Test]
        public async Task Then_no_apprenticeship_incentives_are_returned_for_the_account_legal_entity()
        {
            // Arrange
            var incentives = new List<ApprenticeshipIncentiveModel>();
            _mockApprenticeshipIncentiveDataRepository.Setup(x => x.FindByAccountLegalEntityId(_accountLegalEntityId)).ReturnsAsync(incentives);

            // Act
            var result = await _sut.FindByAccountLegalEntity(_accountLegalEntityId);

            // Assert
            result.Should().BeEmpty();
        }
    }
}
