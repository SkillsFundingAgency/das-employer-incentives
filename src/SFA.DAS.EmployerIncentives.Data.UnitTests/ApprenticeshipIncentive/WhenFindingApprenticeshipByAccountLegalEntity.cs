using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.ApprenticeshipIncentive
{
    [TestFixture]
    public class WhenFindingApprenticeshipByAccountLegalEntity
    {
        private ApprenticeshipIncentives.ApprenticeshipIncentiveDataRepository _sut;
        private Fixture _fixture;
        private EmployerIncentivesDbContext _dbContext;
        private long _uln;
        private long _accountLegalEntityId;

        [SetUp]
        public async Task Arrange()
        {
            _fixture = new Fixture();
            _uln = _fixture.Create<long>();
            _accountLegalEntityId = _fixture.Create<long>();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options);

            _sut = new ApprenticeshipIncentives.ApprenticeshipIncentiveDataRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
            await AddApprenticeshipIncentiveModel(_accountLegalEntityId, _uln);
        }

        [TearDown]
        public void CleanUp()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task Then_when_the_apprenticeship_incentive_exists_it_is_returned_from_the_database()
        {
            // Act
            var matchingIncentives = await _sut.FindByAccountLegalEntityId(_accountLegalEntityId);

            // Assert
            matchingIncentives.Should().NotBeNull();
            matchingIncentives.Count.Should().Be(1);
            matchingIncentives.First().Apprenticeship.UniqueLearnerNumber.Should().Be(_uln);
            matchingIncentives.First().Account.AccountLegalEntityId.Should().Be(_accountLegalEntityId);
        }

        [Test]
        public async Task Then_when_the_apprenticeship_incentive_does_not_exist_it_returns_an_empty_list()
        {
            // Act
            var matchingIncentives = await _sut.FindByAccountLegalEntityId(_accountLegalEntityId - 1);

            // Assert
            matchingIncentives.Count.Should().Be(0);
        }

        [Test]
        public async Task Then_any_withdrawn_applications_are_not_included()
        {
            // Act
            var withdrawnUln = _fixture.Create<long>();
            await AddApprenticeshipIncentiveModel(_accountLegalEntityId, withdrawnUln, Enums.IncentiveStatus.Withdrawn);

            var matchingIncentives = await _sut.FindByAccountLegalEntityId(_accountLegalEntityId);

            // Assert
            matchingIncentives.Should().NotBeNull();
            matchingIncentives.FirstOrDefault(x => x.Apprenticeship?.UniqueLearnerNumber == withdrawnUln).Should().BeNull();
            matchingIncentives.Count.Should().Be(1);
            matchingIncentives.First().Apprenticeship.UniqueLearnerNumber.Should().Be(_uln);
            matchingIncentives.First().Account.AccountLegalEntityId.Should().Be(_accountLegalEntityId);
        }

        private async Task AddApprenticeshipIncentiveModel(long accountLegalEntityId, long uln, Enums.IncentiveStatus status = Enums.IncentiveStatus.Active)
        {
            var account = new Domain.ApprenticeshipIncentives.ValueTypes.Account(_fixture.Create<long>(), accountLegalEntityId);
            var apprenticeship = new Apprenticeship(_fixture.Create<long>(), _fixture.Create<string>(),
                _fixture.Create<string>(), _fixture.Create<DateTime>(), uln,
                _fixture.Create<ApprenticeshipEmployerType>(), _fixture.Create<string>(), _fixture.Create<DateTime>(), _fixture.Create<Provider>());

            var incentive = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(x => x.Apprenticeship, apprenticeship)
                .With(x => x.Account, account)
                .With(x => x.Status, status)
                .Create();

            await _sut.Add(incentive);
            await _dbContext.SaveChangesAsync();
        }
    }
}
