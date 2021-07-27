using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.EmployerIncentives.Domain.Accounts;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.IncentiveApplicationTests
{
    public class WhenAnApprenticeshipIsWithdrawn
    {
        private Fixture _fixture;
        private IncentiveApplication _sut;
        private IncentiveApplicationModel _sutModel;
     
        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sutModel = _fixture
                .Build<IncentiveApplicationModel>()
                .With(i => i.ApprenticeshipModels,
                new List<ApprenticeshipModel>()
                {
                    _fixture
                    .Build<ApprenticeshipModel>()
                    .With(a => a.WithdrawnByEmployer, false)
                    .With(a => a.WithdrawnByCompliance, false)
                    .Create()
                })
                .Create();

            _sut = new IncentiveApplicationFactory().GetExisting(_sutModel.Id, _sutModel);
        }

        [Test]
        public void Then_apprenticeship_is_marked_as_employer_withdrawn_when_an_employer_withdraws_the_application()
        {
            // Arrange
            var serviceRequest = _fixture.Create<ServiceRequest>();
            var apprenticeship = _sut.Apprenticeships.Single();
            apprenticeship.WithdrawnByEmployer.Should().BeFalse();

            // Act
            _sut.EmployerWithdrawal(apprenticeship, _fixture.Create<LegalEntity>(), _fixture.Create<string>(), serviceRequest);

            // Assert
            apprenticeship.WithdrawnByEmployer.Should().BeTrue();
        }

        [Test]
        public void Then_an_EmployerWithdrawn_event_is_raised_when_an_employer_withdraws_the_application()
        {
            // Arrange
            var serviceRequest = _fixture.Create<ServiceRequest>();
            var apprenticeship = _sut.Apprenticeships.Single();

            // Act
            _sut.EmployerWithdrawal(apprenticeship, _fixture.Create<LegalEntity>(), _fixture.Create<string>(), serviceRequest);

            // Assert
            var raisedEvent = _sut.FlushEvents().Single() as EmployerWithdrawn;
            raisedEvent.WithdrawalStatus.Should().Be(IncentiveApplicationStatus.EmployerWithdrawn);
            raisedEvent.AccountId.Should().Be(_sut.AccountId);
            raisedEvent.AccountLegalEntityId.Should().Be(_sut.AccountLegalEntityId);
            raisedEvent.Model.Should().Be(apprenticeship.GetModel());
            raisedEvent.ServiceRequest.Should().Be(serviceRequest);
        }

        [Test]
        public void Then_apprenticeship_is_marked_as_compliance_withdrawn_when_compliance_withdraws_the_application()
        {
            // Arrange
            var serviceRequest = _fixture.Create<ServiceRequest>();
            var apprenticeship = _sut.Apprenticeships.Single();
            apprenticeship.WithdrawnByCompliance.Should().BeFalse();

            // Act
            _sut.ComplianceWithdrawal(apprenticeship, serviceRequest);

            // Assert
            apprenticeship.WithdrawnByCompliance.Should().BeTrue();
        }

        [Test]
        public void Then_a_ComplianceWithdrawn_event_is_raised_when_compliance_withdraws_the_application()
        {
            // Arrange
            var serviceRequest = _fixture.Create<ServiceRequest>();
            var apprenticeship = _sut.Apprenticeships.Single();

            // Act
            _sut.ComplianceWithdrawal(apprenticeship, serviceRequest);

            // Assert
            var raisedEvent = _sut.FlushEvents().Single() as ComplianceWithdrawn;
            raisedEvent.WithdrawalStatus.Should().Be(IncentiveApplicationStatus.ComplianceWithdrawn);
            raisedEvent.AccountId.Should().Be(_sut.AccountId);
            raisedEvent.AccountLegalEntityId.Should().Be(_sut.AccountLegalEntityId);
            raisedEvent.Model.Should().Be(apprenticeship.GetModel());
            raisedEvent.ServiceRequest.Should().Be(serviceRequest);
        }
    }
}
