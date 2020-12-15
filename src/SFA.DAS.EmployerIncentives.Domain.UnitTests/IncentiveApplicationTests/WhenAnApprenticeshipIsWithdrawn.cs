using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

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
                    _fixture.Create<ApprenticeshipModel>()
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
            _sut.EmployerWithdrawal(apprenticeship, serviceRequest);

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
            _sut.EmployerWithdrawal(apprenticeship, serviceRequest);

            // Assert
            var raisedEvent = _sut.FlushEvents().Single() as EmployerWithdrawn;
            raisedEvent.AccountId.Should().Be(_sut.AccountId);
            raisedEvent.AccountLegalEntityId.Should().Be(_sut.AccountLegalEntityId);
            raisedEvent.Model.Should().Be(apprenticeship.GetModel());
            raisedEvent.ServiceRequest.Should().Be(serviceRequest);
        }
    }
}
