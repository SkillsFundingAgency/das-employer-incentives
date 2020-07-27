using FluentAssertions;
using NUnit.Framework;

namespace SFA.DAS.EmployerIncentives.Queries.UnitTests.Policies
{
    public class WhenPoliciesCreated
    {
        [Test]
        public void ThenDefaultSettingsAreApplied()
        {
            var policies = new Queries.Policies(null);

            policies.QueryRetryPolicy.Should().NotBeNull();
        }
    }
}