using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.LegalEntityVendorRegistrationForm;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.LegalEntityVendorRegistrationForm
{
    public class WhenGetLegalEntitiesVendorRegistrationFormCalled
    {
        private EmployerIncentivesDbContext _context;
        private Fixture _fixture;
        private IQueryRepository<DataTransferObjects.Queries.LegalEntityVendorRegistrationForm> _sut;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _context = new EmployerIncentivesDbContext(options);

            _sut = new LegalEntityVendorRegistrationFormQueryRepository(new Lazy<EmployerIncentivesDbContext>(_context));
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
            var expectedAccounts = _fixture.Build<Models.Account>().With(x => x.VrfCaseStatus, "To Process").CreateMany(5);
            var notExpectedAccounts = _fixture.Build<Models.Account>().With(x => x.VrfCaseId, (string)null).CreateMany(5);

            _context.Accounts.AddRange(expectedAccounts);
            _context.Accounts.AddRange(notExpectedAccounts);
            _context.SaveChanges();

            // Act
            var actual = await _sut.GetList(x => x.VrfCaseStatus != "Case Request completed" && x.VrfCaseId != null);

            //Assert
            actual.Should().BeEquivalentTo(expectedAccounts, opts => opts.ExcludingMissingMembers());
        }
    }
}