using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.LegalEntityTests
{
    public class WhenAgreementSigned
    {
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
        }

        [TestCase(null, 2, 1, 2)]
        [TestCase(null, 2, 3, null)]
        [TestCase(1, 2, 1, 2)]
        [TestCase(3, 2, 1, 3)]
        public void Then_signed_version_is_set(int? currentVersion, int signedVersion, int minimumRequiredVersion, int? expectedVersion)
        {
            // Arrange
            var model = _fixture.Build<LegalEntityModel>()
                .With(x => x.SignedAgreementVersion, currentVersion)
                .Create();

            var legalEntity = LegalEntity.Create(model);

            // Act
            legalEntity.SignedAgreement(signedVersion, minimumRequiredVersion);

            // Assert
            model.SignedAgreementVersion.Should().Be(expectedVersion);
        }
    }
}
