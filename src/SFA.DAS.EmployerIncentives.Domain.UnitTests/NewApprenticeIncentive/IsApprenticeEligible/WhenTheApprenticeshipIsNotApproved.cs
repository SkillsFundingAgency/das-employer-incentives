using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.NewApprenticeIncentive.IsApprenticeEligible
{
    [TestFixture]
    public class WhenTheApprenticeshipIsNotApproved
    {
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void Then_the_apprenticeship_is_not_eligible()
        {
            var apprenticeship = _fixture
                .Build<Apprenticeship>()
                .With(f => f.IsApproved, false)
                .Create();

            var result = Domain.NewApprenticeIncentive.NewApprenticeIncentive.IsApprenticeshipEligible(apprenticeship);

            result.Should().BeFalse();
        }
    }
}
