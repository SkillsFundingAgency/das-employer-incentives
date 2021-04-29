using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetIncentiveDetails;

namespace SFA.DAS.EmployerIncentives.Queries.UnitTests.NewApprenticeIncentive.Handlers
{
    public class WhenHandlingGetIncentiveDetailsQuery
    {
        private GetIncentiveDetailsQueryHandler _sut;
        
        [SetUp]
        public void Arrange()
        {
            _sut = new GetIncentiveDetailsQueryHandler();
        }

        [Test]
        public async Task Then_the_incentive_eligibility_is_returned()
        {
            //Arrange
            var query = new GetIncentiveDetailsRequest();
            
            //Act
            var result = await _sut.Handle(query, CancellationToken.None);

            //Assert
            result.EligibilityStartDate.Should().Be(Incentive.EligibilityStartDate);
            result.EligibilityEndDate.Should().Be(Incentive.LatestCommitmentStartDate);
        }
    }
}
