using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
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
            result.EligibilityStartDate.Should().Be(new DateTime(2021, 4, 1));
            result.EligibilityEndDate.Should().Be(new DateTime(2022, 1, 31));
        }
    }
}
