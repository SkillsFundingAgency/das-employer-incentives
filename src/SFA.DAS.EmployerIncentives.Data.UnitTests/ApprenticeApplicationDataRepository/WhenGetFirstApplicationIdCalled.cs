using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using Moq;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.ApprenticeApplicationDataRepository
{
    [TestFixture]
    public class WhenGetFirstApplicationIdCalled
    {
        private EmployerIncentivesDbContext _context;
        private Fixture _fixture;
        private Data.ApprenticeApplicationDataRepository _sut;
        private Mock<IDateTimeService> _mockDateTimeService;
        private Mock<ICollectionCalendarService> _mockCollectionCalendarService;
        private Mock<IServiceProvider> _mockServiceProvider;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _mockServiceProvider = new Mock<IServiceProvider>();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _context = new EmployerIncentivesDbContext(options, _mockServiceProvider.Object);

            _mockDateTimeService = new Mock<IDateTimeService>();
            _mockCollectionCalendarService = new Mock<ICollectionCalendarService>();

            _sut = new Data.ApprenticeApplicationDataRepository(new Lazy<EmployerIncentivesDbContext>(_context), _mockDateTimeService.Object, _mockCollectionCalendarService.Object);
        }

        [TearDown]
        public void CleanUp()
        {
            _context.Dispose();
        }

        [Test]
        public async Task Then_first_id_is_returned_when_has_submitted_applications()
        {
            // Arrange
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();
            var account = _fixture.Build<Models.Account>().With(x => x.Id, accountId).With(x => x.AccountLegalEntityId, accountLegalEntityId).Create();

            var allApplications = _fixture.CreateMany<Models.IncentiveApplication>(5).ToArray();
            foreach (var application in allApplications)
            {
                application.AccountId = accountId;
                application.AccountLegalEntityId = accountLegalEntityId;
                application.Status = IncentiveApplicationStatus.Submitted;
            }

            _context.Accounts.Add(account);
            _context.Applications.AddRange(allApplications);

            var expectedId = allApplications.OrderBy(x => x.DateSubmitted).First().Id;

            _context.SaveChanges();

            // Act
            var result = await _sut.GetFirstSubmittedApplicationId(accountLegalEntityId);

            // Assert
            result.Should().Be(expectedId);
        }

        [Test]
        public async Task Then_null_is_returned_when_has_no_submitted_applications()
        {
            // Arrange
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();
            var account = _fixture.Build<Models.Account>().With(x => x.Id, accountId).With(x => x.AccountLegalEntityId, accountLegalEntityId).Create();

            _context.Accounts.Add(account);

            _context.SaveChanges();

            // Act
            var result = await _sut.GetFirstSubmittedApplicationId(accountLegalEntityId);

            // Assert
            result.Should().Be(Guid.Empty);
        }
    }
}
