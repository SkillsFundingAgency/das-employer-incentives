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
    public class WhenGetPendingPaymentCalled
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

            _sut = new ApprenticeshipIncentives.PendingPaymentQueryRepository(new Lazy<EmployerIncentivesDbContext>(_context));
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
            var pendingPayment = _fixture.Create<PendingPayment>();
            
            _context.PendingPayments.Add(pendingPayment);
            _context.SaveChanges();

            // Act
            var actual = await _sut.Get(x => x.Id == pendingPayment.Id);

            //Assert
            actual.Should().BeEquivalentTo(pendingPayment, opts => opts.ExcludingMissingMembers());
        }

        [Test]
        public async Task Then_null_is_returned_when_the_application_is_not_found()
        {
            // Arrange
            var allPendingPayments = _fixture.CreateMany<PendingPayment>(10).ToArray();
            
            _context.PendingPayments.AddRange(allPendingPayments);
            _context.SaveChanges();

            // Act
            var actual = await _sut.Get(x => x.Id == Guid.NewGuid());

            //Assert
            actual.Should().BeNull();
        }
    }
}