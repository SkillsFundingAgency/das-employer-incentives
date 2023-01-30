using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.IncentiveApplicationLegalEntityQueryRepository
{
    public class WhenGetIncentiveApplicationLegalEntitiesCalled
    {
        private EmployerIncentivesDbContext _context;
        private Fixture _fixture;
        private IQueryRepository<DataTransferObjects.Queries.IncentiveApplicationLegalEntity> _sut;
        private Mock<IServiceProvider> _mockServiceProvider;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _mockServiceProvider = new Mock<IServiceProvider>();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _context = new EmployerIncentivesDbContext(options, _mockServiceProvider.Object);

            _sut = new IncentiveApplicationLegalEntity.IncentiveApplicationLegalEntityQueryRepository(new Lazy<EmployerIncentivesDbContext>(_context));
        }

        [TearDown]
        public void CleanUp()
        {
            _context.Dispose();
        }

        [Test]
        public async Task Then_data_is_fetched_from_database()
        {
            // Arrange
            var account = _fixture.Create<Models.Account>();
            var allApplications = _fixture.Build<Models.IncentiveApplication>().With(x => x.AccountLegalEntityId, account.AccountLegalEntityId).CreateMany(10).ToArray();
            
            _context.Accounts.Add(account);
            _context.Applications.AddRange(allApplications);
            _context.SaveChanges();

            // Act
            var actual = (await _sut.GetList(x => x.LegalEntityId == account.LegalEntityId)).ToArray();

            //Assert
            actual.Should().BeEquivalentTo(allApplications, opts => opts.ExcludingMissingMembers());
        }
    }
}