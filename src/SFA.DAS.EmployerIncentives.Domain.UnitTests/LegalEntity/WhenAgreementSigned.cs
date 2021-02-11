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

        [Test]
        public void Then_has_signed_incentive_terms_is_true_if_version_greater_than_minimum_required()
        {
            // Arrange
            var model = _fixture.Build<LegalEntityModel>().With(x => x.HasSignedAgreementTerms, false).Create();
            var legalEntity = LegalEntity.Create(model);

            // Act
            legalEntity.SignedAgreement(5, 4);

            // Assert
            legalEntity.HasSignedAgreementTerms.Should().BeTrue();
        }

        [Test]
        public void Then_has_signed_incentive_terms_is_true_if_version_equal_to_the_minimum_required()
        {
            // Arrange
            var model = _fixture.Build<LegalEntityModel>().With(x => x.HasSignedAgreementTerms, false).Create();
            var legalEntity = LegalEntity.Create(model);

            // Act
            legalEntity.SignedAgreement(4, 4);

            // Assert
            legalEntity.HasSignedAgreementTerms.Should().BeTrue();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Then_has_signed_incentive_terms_is_not_changed_if_version_less_than_minimum_required(bool originalValue)
        {
            // Arrange
            var model = _fixture.Build<LegalEntityModel>().With(x => x.HasSignedAgreementTerms, originalValue).Create();
            var legalEntity = LegalEntity.Create(model);

            // Act
            legalEntity.SignedAgreement(3, 4);

            // Assert
            legalEntity.HasSignedAgreementTerms.Should().Be(originalValue);
        }

        [TestCase(null, 2, 1, 2)]
        [TestCase(null, 2, 3, null)]
        [TestCase(1, 2, 1, 2)]
        [TestCase(3, 2, 1, 3)]
        public void Then_signed_version_is_set(int? currentVersion, int signedVersion, int minimumRequiredVersion, int? expectedVersion)
        {
            // Arrange
            var model = _fixture.Build<LegalEntityModel>()
                .With(x => x.HasSignedAgreementTerms, currentVersion != null)
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
