using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Data.IncentiveApplication;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.Persistence.IncentiveApplicationDomainRepository
{
    [TestFixture]
    public class WhenFindingApplicationsForLegalEntity
    {
        private Commands.Persistence.IncentiveApplicationDomainRepository _sut;
        private Mock<IIncentiveApplicationDataRepository> _mockIncentiveApplicationDataRepository;
        private Mock<IIncentiveApplicationFactory> _mockIncentiveApplicationFactory;
        private Mock<IDomainEventDispatcher> _mockDomainEventDispatcher;
        private long _accountLegalEntityId;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _accountLegalEntityId = _fixture.Create<long>();

            _mockIncentiveApplicationDataRepository = new Mock<IIncentiveApplicationDataRepository>();
            _mockIncentiveApplicationFactory = new Mock<IIncentiveApplicationFactory>();
            _mockDomainEventDispatcher = new Mock<IDomainEventDispatcher>();

            _sut = new Commands.Persistence.IncentiveApplicationDomainRepository(_mockIncentiveApplicationDataRepository.Object, _mockIncentiveApplicationFactory.Object, _mockDomainEventDispatcher.Object);
        }

        [Test]
        public async Task Then_applications_for_the_account_legal_entity_are_returned()
        {
            // Arrange
            var applications = _fixture.CreateMany<IncentiveApplicationModel>(5);
            _mockIncentiveApplicationDataRepository.Setup(x => x.FindApplicationsByAccountLegalEntity(_accountLegalEntityId)).ReturnsAsync(applications);

            // Act
            var result = await _sut.FindByAccountLegalEntity(_accountLegalEntityId);

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(applications.Count());
        }

        [Test]
        public async Task Then_no_applications_are_returned_for_the_account_legal_entity()
        {
            // Arrange
            var applications = new List<IncentiveApplicationModel>();
            _mockIncentiveApplicationDataRepository.Setup(x => x.FindApplicationsByAccountLegalEntity(_accountLegalEntityId)).ReturnsAsync(applications);

            // Act
            var result = await _sut.FindByAccountLegalEntity(_accountLegalEntityId);

            // Assert
            result.Should().BeEmpty();
        }
    }
}
