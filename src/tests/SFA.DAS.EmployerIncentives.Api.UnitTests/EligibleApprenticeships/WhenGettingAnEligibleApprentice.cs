using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetApprenticeshipEligibility;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.EligibleApprenticeships
{
    public class WhenGettingAnEligibleApprentice
    {
        private EligibleApprenticeshipsQueryController _sut;
        private Mock<IQueryDispatcher> _queryDispatcherMock;
        
        [SetUp]
        public void Setup()
        {
            _queryDispatcherMock = new Mock<IQueryDispatcher>();
            _sut = new EligibleApprenticeshipsQueryController(_queryDispatcherMock.Object);
        }

        [Test]
        public async Task Then_a_successful_response_is_returned_for_an_eligible_apprenticeship()
        {
            // Arrange
            var expected = new GetApprenticeshipEligibilityResponse(true);
            var uln = 123456;
            var startDate = DateTime.Now;
            var isApproved = true;
            
            _queryDispatcherMock.Setup(x => x.Send<GetApprenticeshipEligibilityRequest, GetApprenticeshipEligibilityResponse>(
                    It.Is<GetApprenticeshipEligibilityRequest>(r => r.Apprenticeship.UniqueLearnerNumber == uln && r.Apprenticeship.StartDate == startDate && r.Apprenticeship.IsApproved == isApproved)))
                .ReturnsAsync(expected);

            // Act
            var actual = await _sut.IsApprenticeshipEligible(uln, startDate, isApproved) as OkResult;

            // Assert
            actual.Should().NotBeNull();
        }

        [Test]
        public async Task Then_error_returned_for_an_ineligible_apprenticeship()
        {
            // Arrange
            var expected = new GetApprenticeshipEligibilityResponse(false);
            var uln = 123456;
            var startDate = DateTime.Now;
            var isApproved = true;

            _queryDispatcherMock.Setup(x => x.Send<GetApprenticeshipEligibilityRequest, GetApprenticeshipEligibilityResponse>(
                    It.Is<GetApprenticeshipEligibilityRequest>(r => r.Apprenticeship.UniqueLearnerNumber == uln && r.Apprenticeship.StartDate == startDate && r.Apprenticeship.IsApproved == isApproved)))
                .ReturnsAsync(expected);

            // Act
            var actual = await _sut.IsApprenticeshipEligible(uln, startDate, isApproved) as NotFoundResult;

            // Assert
            actual.Should().NotBeNull();
        }

    }
}