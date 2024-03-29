﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.IncentiveApplication;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.IncentiveApplicationQueryRepository
{
    public class WhenGetIncentiveApplicationCalled
    {
        private EmployerIncentivesDbContext _context;
        private Fixture _fixture;
        private IIncentiveApplicationQueryRepository _sut;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _context = new EmployerIncentivesDbContext(options);
           
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
            var applicationId = Guid.NewGuid();

            allApplications[1].Id = applicationId;
            foreach(var apprentice in allApplications[1].Apprenticeships)
            {
                apprentice.Phase = Phase.Phase2;
            }

            _context.Accounts.Add(account);
            _context.Applications.AddRange(allApplications);
            _context.SaveChanges();

            // Act
            var actual = await _sut.Get(x => x.Id == applicationId);

            //Assert
            actual.Should().BeEquivalentTo(allApplications[1], opts => opts.ExcludingMissingMembers());
            actual.LegalEntityId.Should().Be(account.LegalEntityId);
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
            var actual = await _sut.Get(x => x.Id == applicationId);

            //Assert
            actual.Should().BeNull();
        }

        [Test]
        public async Task Then_bank_details_required_is_false_if_a_non_rejected_status_value_specified_for_vrf_status()
        {
            // Arrange
            var applicationId = Guid.NewGuid();

            var account = _fixture.Create<Models.Account>();
            account.VrfCaseStatus = "To Process";
            
            var application = _fixture.Create<Models.IncentiveApplication>();            
            application.Id = applicationId;
            application.AccountLegalEntityId = account.AccountLegalEntityId;
            foreach (var apprentice in application.Apprenticeships)
            {
                apprentice.Phase = Phase.Phase2;
            }
            _context.Accounts.Add(account);
            _context.Applications.Add(application);
            _context.SaveChanges();

            // Act
            var actual = await _sut.Get(x => x.Id == applicationId);

            //Assert
            actual.BankDetailsRequired.Should().BeFalse();
        }

        [TestCase(LegalEntityVrfCaseStatus.RejectedDataValidation)]
        [TestCase(LegalEntityVrfCaseStatus.RejectedVer1)]
        [TestCase(LegalEntityVrfCaseStatus.RejectedVerification)]
        public async Task Then_bank_details_required_is_true_if_a_rejected_status_value_specified_for_vrf_status(string vrfCaseStatus)
        {
            // Arrange
            var applicationId = Guid.NewGuid();

            var account = _fixture.Create<Models.Account>();
            account.VrfVendorId = "";
            account.VrfCaseStatus = vrfCaseStatus;

            var application = _fixture.Create<Models.IncentiveApplication>();
            application.Id = applicationId;
            application.AccountLegalEntityId = account.AccountLegalEntityId;
            foreach (var apprentice in application.Apprenticeships)
            {
                apprentice.Phase = Phase.Phase2;
            }
            _context.Accounts.Add(account);
            _context.Applications.Add(application);
            _context.SaveChanges();

            // Act
            var actual = await _sut.Get(x => x.Id == applicationId);

            //Assert
            actual.BankDetailsRequired.Should().BeTrue();
        }

        [TestCase("")]
        [TestCase(null)]
        public async Task Then_bank_details_required_is_true_if_no_value_for_vrf_status(string vrfStatus)
        {
            // Arrange
            var applicationId = Guid.NewGuid();

            var account = _fixture.Create<Models.Account>();
            account.VrfVendorId = "";
            account.VrfCaseStatus = vrfStatus;

            var application = _fixture.Create<Models.IncentiveApplication>();

            application.Id = applicationId;
            application.AccountLegalEntityId = account.AccountLegalEntityId;
            foreach (var apprentice in application.Apprenticeships)
            {
                apprentice.Phase = Phase.Phase2;
            }
            _context.Accounts.Add(account);
            _context.Applications.Add(application);
            _context.SaveChanges();

            // Act
            var actual = await _sut.Get(x => x.Id == applicationId);

            //Assert
            actual.BankDetailsRequired.Should().BeTrue();
        }

        [TestCase("", "")]
        [TestCase(null, null)]
        [TestCase("", "000000")]
        public async Task Then_bank_details_required_is_true_if_no_vendor_id_or_vrf_status(string vrfStatus, string vrfVendorId)
        {
            // Arrange
            var applicationId = Guid.NewGuid();

            var account = _fixture.Create<Models.Account>();
            account.VrfCaseStatus = vrfStatus;
            account.VrfVendorId = vrfVendorId;

            var application = _fixture.Create<Models.IncentiveApplication>();

            application.Id = applicationId;
            application.AccountLegalEntityId = account.AccountLegalEntityId;
            foreach (var apprentice in application.Apprenticeships)
            {
                apprentice.Phase = Phase.Phase2;
            }
            _context.Accounts.Add(account);
            _context.Applications.Add(application);
            _context.SaveChanges();

            // Act
            var actual = await _sut.Get(x => x.Id == applicationId);

            //Assert
            actual.BankDetailsRequired.Should().BeTrue();
        }

        [TestCase("ABC123")]
        public async Task Then_bank_details_required_is_false_if_vendor_id_present(string vrfVendorId)
        {
            // Arrange
            var applicationId = Guid.NewGuid();

            var account = _fixture.Create<Models.Account>();
            account.VrfVendorId = vrfVendorId;

            var application = _fixture.Create<Models.IncentiveApplication>();

            application.Id = applicationId;
            application.AccountLegalEntityId = account.AccountLegalEntityId;
            foreach (var apprentice in application.Apprenticeships)
            {
                apprentice.Phase = Phase.Phase2;
            }
            _context.Accounts.Add(account);
            _context.Applications.Add(application);
            _context.SaveChanges();

            // Act
            var actual = await _sut.Get(x => x.Id == applicationId);

            //Assert
            actual.BankDetailsRequired.Should().BeFalse();
        }
    }
}