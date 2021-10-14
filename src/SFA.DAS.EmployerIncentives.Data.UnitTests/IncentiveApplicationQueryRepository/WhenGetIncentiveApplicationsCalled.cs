using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Data.IncentiveApplication;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.IncentiveApplicationQueryRepository
{
    public class WhenGetIncentiveApplicationsCalled
    {
        private EmployerIncentivesDbContext _context;
        private Fixture _fixture;
        private IIncentiveApplicationQueryRepository _sut;
        private List<IncentivePaymentProfile> _paymentProfiles;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _context = new EmployerIncentivesDbContext(options);

            _paymentProfiles = new List<IncentivePaymentProfile>
            {
                new IncentivePaymentProfile(IncentivePhase.Create(), _fixture.CreateMany<PaymentProfile>(2).ToList())
            };

            _sut = new IncentiveApplication.IncentiveApplicationQueryRepository(new Lazy<EmployerIncentivesDbContext>(_context));
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
            const long accountId = -1;

            allApplications[0].AccountId = accountId;
            allApplications[3].AccountId = accountId;
            foreach (var apprentice in allApplications[0].Apprenticeships)
            {
                apprentice.Phase = Phase.Phase2;
            }
            foreach (var apprentice in allApplications[3].Apprenticeships)
            {
                apprentice.Phase = Phase.Phase2;
            }

            _context.Accounts.Add(account);
            _context.Applications.AddRange(allApplications);
            _context.SaveChanges();

            // Act
            var actual = (await _sut.GetList(_paymentProfiles, x => x.AccountId == accountId)).ToArray();

            //Assert
            actual.All(x => x.AccountId == accountId).Should().BeTrue();
            actual.Should().BeEquivalentTo(new[] {allApplications[0], allApplications[3]}, opts => opts.ExcludingMissingMembers());
        }

        [Test]
        public async Task Then_apprenticeships_are_ordered()
        {
            // Arrange
            var account = _fixture.Create<Models.Account>();
            var application = _fixture.Build<Models.IncentiveApplication>()
                            .With(x => x.AccountId, account.Id)
                            .With(x => x.AccountLegalEntityId, account.AccountLegalEntityId)
                            .Create();

            application.Apprenticeships = new List<Models.IncentiveApplicationApprenticeship>()
            {
                _fixture.Build<Models.IncentiveApplicationApprenticeship>().With(a => a.FirstName, "ZFirst").With(a => a.LastName, "ALast").With(a => a.ULN, 9).With(a => a.Phase, Phase.Phase2).Create(),
                _fixture.Build<Models.IncentiveApplicationApprenticeship>().With(a => a.FirstName, "AFirst").With(a => a.LastName, "ALast").With(a => a.ULN, 1).With(a => a.Phase, Phase.Phase2).Create(),
                _fixture.Build<Models.IncentiveApplicationApprenticeship>().With(a => a.FirstName, "AFirst").With(a => a.LastName, "ALast").With(a => a.ULN, 9).With(a => a.Phase, Phase.Phase2).Create(),
                _fixture.Build<Models.IncentiveApplicationApprenticeship>().With(a => a.FirstName, "AFirst").With(a => a.LastName, "ZLast").With(a => a.ULN, 9).With(a => a.Phase, Phase.Phase2).Create()
            };

            _context.Accounts.Add(account);
            _context.Applications.AddRange(application);
            _context.SaveChanges();

            // Act
            var actual = (await _sut.GetList(_paymentProfiles, x => x.AccountId == account.Id)).Single();

            //Assert
            actual.Apprenticeships.Count().Should().Be(4);

            IncentiveApplicationApprenticeshipDto previous = null;
            foreach (var apprenticeship in actual.Apprenticeships)
            {
                if (previous != null)
                {
                    apprenticeship.FirstName.CompareTo(previous.FirstName).Should().BeGreaterOrEqualTo(0);
                    if (apprenticeship.FirstName.CompareTo(previous.FirstName) == 0)
                    {
                        apprenticeship.LastName.CompareTo(previous.LastName).Should().BeGreaterOrEqualTo(0);
                        if (apprenticeship.LastName.CompareTo(previous.LastName) == 0)
                        {
                            previous.Uln.Should().BeLessOrEqualTo(apprenticeship.Uln);
                        }
                    }
                }

                previous = apprenticeship;
            }            
        }
    }
}