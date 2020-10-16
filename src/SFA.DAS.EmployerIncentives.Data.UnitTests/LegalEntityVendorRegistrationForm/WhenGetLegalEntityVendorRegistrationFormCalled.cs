using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Data.LegalEntityVendorRegistrationForm;
using SFA.DAS.EmployerIncentives.Data.Models;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.LegalEntityVendorRegistrationForm
{
    public class WhenGetLegalEntityVendorRegistrationFormCalled
    {
        private EmployerIncentivesDbContext _context;
        private Fixture _fixture;
        private IQueryRepository<LegalEntityVendorRegistrationFormDto> _sut;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _context = new EmployerIncentivesDbContext(options);

            _sut = new LegalEntityVendorRegistrationFormQueryRepository(_context);
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
            var accounts = _fixture.CreateMany<Models.Account>(3);
            
            _context.Accounts.AddRange(accounts);
            _context.SaveChanges();

            // Act
            var actual = await _sut.Get(x => x.LegalEntityId == accounts.First().LegalEntityId);

            //Assert
            actual.Should().BeEquivalentTo(accounts.First(), opts => opts.ExcludingMissingMembers());
        }

        [Test]
        public async Task Then_null_is_returned_when_the_legal_entity_is_not_found()
        {
            // Arrange
            var accounts = _fixture.CreateMany<Models.Account>(3);

            _context.Accounts.AddRange(accounts);
            _context.SaveChanges();

            var legalEntityId = _fixture.Create<long>();

            // Act
            var actual = await _sut.Get(x => x.LegalEntityId == legalEntityId);

            //Assert
            actual.Should().BeNull();
        }
    }
}