using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.PendingPaymentQueryRepository
{
    public class WhenGetPendingPaymentsCalled
    {
        private EmployerIncentivesDbContext _context;
        private Fixture _fixture;
        private IQueryRepository<PendingPaymentDto> _sut;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _context = new EmployerIncentivesDbContext(options);

            _sut = new ApprenticeshipIncentives.PendingPaymentQueryRepository(_context);
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
            var accountLegalEntityId = _fixture.Create<long>();
            var allPendingPayments = _fixture.CreateMany<PendingPayment>(10).ToArray();
            
            allPendingPayments[0].AccountLegalEntityId = accountLegalEntityId;
            allPendingPayments[3].AccountLegalEntityId = accountLegalEntityId;

            _context.PendingPayments.AddRange(allPendingPayments);
            _context.SaveChanges();

            // Act
            var actual = (await _sut.GetList(x => x.AccountLegalEntityId == accountLegalEntityId)).ToArray();

            //Assert
            actual.All(x => x.AccountLegalEntityId == accountLegalEntityId).Should().BeTrue();
            actual.Should().BeEquivalentTo(new[] {allPendingPayments[0], allPendingPayments[3]}, opts => opts.ExcludingMissingMembers());
        }
    }
}