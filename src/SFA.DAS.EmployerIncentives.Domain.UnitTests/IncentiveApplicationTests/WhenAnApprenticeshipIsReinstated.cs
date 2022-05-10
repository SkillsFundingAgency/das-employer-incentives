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
    public class WhenAnApprenticeshipIsReinstated
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
                    .With(a => a.WithdrawnByEmployer, true)
                    .With(a => a.WithdrawnByCompliance, true)
                    .Create()
                })
                .Create();

            _sut = new IncentiveApplicationFactory().GetExisting(_sutModel.Id, _sutModel);
        }

        [Test]
        public void Then_withdrawn_flags_are_removed()
        {
            // Arrange
            var apprenticeship = _sut.Apprenticeships.Single();
            
            // Act
            _sut.ReinstateWithdrawal(apprenticeship);

            // Assert
            apprenticeship.WithdrawnByEmployer.Should().BeFalse();
            apprenticeship.WithdrawnByCompliance.Should().BeFalse();
        }

        [Test]
        public void Then_an_ApplicationReinstated_event_is_raised_when_the_application_is_reinstated()
        {
            // Arrange
            var apprenticeship = _sut.Apprenticeships.Single();

            // Act
            _sut.ReinstateWithdrawal(apprenticeship);

            // Assert
            var raisedEvent = _sut.FlushEvents().Single() as ApplicationReinstated;
            raisedEvent.AccountId.Should().Be(_sut.AccountId);
            raisedEvent.AccountLegalEntityId.Should().Be(_sut.AccountLegalEntityId);
            raisedEvent.Model.Should().Be(apprenticeship.GetModel());
        }
    }
}
