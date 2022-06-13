using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.IncentiveApplicationLegalEntityQueryRepository
{
    public class WhenGetIncentiveApplicationLegalEntityCalled
    {
        private EmployerIncentivesDbContext _context;
        private Fixture _fixture;
        private IQueryRepository<DataTransferObjects.Queries.IncentiveApplicationLegalEntity> _sut;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _context = new EmployerIncentivesDbContext(options);

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
            var allApplications = _fixture.Build<Models.IncentiveApplication>().With(x => x.AccountLegalEntityId, account.AccountLegalEntityId).CreateMany<Models.IncentiveApplication>(10).ToArray();
            var applicationId = Guid.NewGuid();

            allApplications[1].Id = applicationId;

            _context.Accounts.Add(account);
            _context.Applications.AddRange(allApplications);
            _context.SaveChanges();

            // Act
            var actual = await _sut.Get(x => x.ApplicationId == applicationId);

            //Assert
            actual.ApplicationId.Should().Be(applicationId);
            actual.ApplicationStatus.Should().Be(allApplications[1].Status);
            actual.LegalEntityId.Should().Be(account.LegalEntityId);
            actual.VrfCaseId.Should().Be(account.VrfCaseId);
        }

        [Test]
        public async Task Then_null_is_returned_when_the_application_is_not_found()
        {
            // Arrange
            var allApplications = _fixture.CreateMany<Models.IncentiveApplication>(10).ToArray();
            var applicationId = Guid.NewGuid();

            _context.Applications.AddRange(allApplications);
            _context.SaveChanges();

            // Act
            var actual = await _sut.Get(x => x.ApplicationId == applicationId);

            //Assert
            actual.Should().BeNull();
        }
    }
}