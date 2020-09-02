using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetApprenticeshipEligibility;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.EligibleApprenticeships
{
    public class WhenGettingEligibleApprentices
    {
        private EligibleApprenticeshipsQueryController _sut;
        private Mock<IQueryDispatcher> _queryDispatcherMock;
        private Fixture _fixture;
        private long _accountId;
        private long _accountLegalEntityId;
        private List<EligibleApprenticeshipCheckDetails> _apprenticeshipDetails;

        [SetUp]
        public void Setup()
        {
            _queryDispatcherMock = new Mock<IQueryDispatcher>();
            _sut = new EligibleApprenticeshipsQueryController(_queryDispatcherMock.Object);
            _fixture = new Fixture();

            _accountId = _fixture.Create<long>();
            _accountLegalEntityId = _fixture.Create<long>();
            _apprenticeshipDetails = _fixture.CreateMany<EligibleApprenticeshipCheckDetails>(10).ToList();
        }

        [Test]
        public async Task Then_a_successful_response_is_returned_for_an_eligible_apprenticeship()
        {
            // Arrange
            var eligible = new List<EligibleApprenticeshipResult>();
            foreach(var apprentice in _apprenticeshipDetails)
            {
                eligible.Add(new EligibleApprenticeshipResult { Eligible = true, Uln = apprentice.Uln });
            }
            var expected = new GetApprenticeshipEligibilityResponse(true); 

            _queryDispatcherMock.Setup(x => x.Send<GetApprenticeshipEligibilityRequest, GetApprenticeshipEligibilityResponse>(
                    It.IsAny<GetApprenticeshipEligibilityRequest>()))
                .ReturnsAsync(expected);

            // Act
            var actual = await _sut.AreApprenticeshipsEligible(_accountId, _accountLegalEntityId, _apprenticeshipDetails) as OkObjectResult;

            // Assert
            actual.Should().NotBeNull();
            _queryDispatcherMock.Verify(x => x.Send<GetApprenticeshipEligibilityRequest, GetApprenticeshipEligibilityResponse>(
                    It.IsAny<GetApprenticeshipEligibilityRequest>()), Times.Exactly(eligible.Count));
            var result = actual.Value as IEnumerable<EligibleApprenticeshipResult>;
            result.Count().Should().Be(eligible.Count());
        }

        [Test]
        public async Task Then_a_successful_response_is_returned_for_a_mix_of_eligible_and_ineligible_apprenticeships()
        {
            // Arrange
            _apprenticeshipDetails = _fixture.CreateMany<EligibleApprenticeshipCheckDetails>(4).ToList();
            var eligible = new List<EligibleApprenticeshipResult>
            {
                new EligibleApprenticeshipResult { Eligible = true, Uln = _apprenticeshipDetails[0].Uln },
                new EligibleApprenticeshipResult { Eligible = false, Uln = _apprenticeshipDetails[1].Uln },
                new EligibleApprenticeshipResult { Eligible = true, Uln = _apprenticeshipDetails[2].Uln },
                new EligibleApprenticeshipResult { Eligible = false, Uln = _apprenticeshipDetails[3].Uln }
            };

            var expected = new GetApprenticeshipEligibilityResponse(true);

            _queryDispatcherMock.Setup(x => x.Send<GetApprenticeshipEligibilityRequest, GetApprenticeshipEligibilityResponse>(
                    It.Is<GetApprenticeshipEligibilityRequest>(x => x.Apprenticeship.UniqueLearnerNumber == _apprenticeshipDetails[0].Uln)))
                .ReturnsAsync(new GetApprenticeshipEligibilityResponse(true));
            _queryDispatcherMock.Setup(x => x.Send<GetApprenticeshipEligibilityRequest, GetApprenticeshipEligibilityResponse>(
                    It.Is<GetApprenticeshipEligibilityRequest>(x => x.Apprenticeship.UniqueLearnerNumber == _apprenticeshipDetails[1].Uln)))
                .ReturnsAsync(new GetApprenticeshipEligibilityResponse(false));
            _queryDispatcherMock.Setup(x => x.Send<GetApprenticeshipEligibilityRequest, GetApprenticeshipEligibilityResponse>(
                    It.Is<GetApprenticeshipEligibilityRequest>(x => x.Apprenticeship.UniqueLearnerNumber == _apprenticeshipDetails[2].Uln)))
                .ReturnsAsync(new GetApprenticeshipEligibilityResponse(true));
            _queryDispatcherMock.Setup(x => x.Send<GetApprenticeshipEligibilityRequest, GetApprenticeshipEligibilityResponse>(
                    It.Is<GetApprenticeshipEligibilityRequest>(x => x.Apprenticeship.UniqueLearnerNumber == _apprenticeshipDetails[3].Uln)))
                .ReturnsAsync(new GetApprenticeshipEligibilityResponse(false));

            // Act
            var actual = await _sut.AreApprenticeshipsEligible(_accountId, _accountLegalEntityId, _apprenticeshipDetails) as OkObjectResult;

            // Assert
            actual.Should().NotBeNull();
            _queryDispatcherMock.Verify(x => x.Send<GetApprenticeshipEligibilityRequest, GetApprenticeshipEligibilityResponse>(
                    It.IsAny<GetApprenticeshipEligibilityRequest>()), Times.Exactly(eligible.Count));
            var result = actual.Value as IEnumerable<EligibleApprenticeshipResult>;
            result.Count().Should().Be(eligible.Count());
        }

    }
}