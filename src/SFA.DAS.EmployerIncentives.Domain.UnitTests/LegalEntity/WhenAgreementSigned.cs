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
    }
}
