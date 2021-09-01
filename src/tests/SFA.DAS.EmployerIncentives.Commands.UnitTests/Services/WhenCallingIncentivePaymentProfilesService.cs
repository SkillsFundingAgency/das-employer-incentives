using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Services;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.Services
{
    [TestFixture]
    public class WhenCallingIncentivePaymentProfilesService
    {
        private IncentivePaymentProfilesService _sut;

        [SetUp]
        public void Arrange()
        {
            _sut = new IncentivePaymentProfilesService();
        }

        [Test]
        public void Then_the_payment_profiles_should_contain_values_for_employer_incentives_phases()
        {
            var result = _sut.Get().ToList();

            result.Count.Should().Be(2);
            var phase1 = result.FirstOrDefault(x => x.IncentivePhase.Identifier == Phase.Phase1);
            phase1.PaymentProfiles.FirstOrDefault(x => x.IncentiveType == IncentiveType.UnderTwentyFiveIncentive).Should().NotBeNull();
            phase1.PaymentProfiles.FirstOrDefault(x => x.IncentiveType == IncentiveType.TwentyFiveOrOverIncentive).Should().NotBeNull();
            var phase2 = result.FirstOrDefault(x => x.IncentivePhase.Identifier == Phase.Phase2);
            phase2.PaymentProfiles.FirstOrDefault(x => x.IncentiveType == IncentiveType.UnderTwentyFiveIncentive).Should().NotBeNull();
            phase2.PaymentProfiles.FirstOrDefault(x => x.IncentiveType == IncentiveType.TwentyFiveOrOverIncentive).Should().NotBeNull();
        }
    }
}
