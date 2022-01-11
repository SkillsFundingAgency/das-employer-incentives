using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    [TestFixture]
    public class WhenIsNewAgreementRequired
    {
        private readonly Fixture _fixture = new Fixture();
   
        [Test]
        [TestCase(null, true)]
        [TestCase(1, true)]
        [TestCase(2, true)]
        [TestCase(3, true)]
        [TestCase(4, true)]
        [TestCase(5, true)]
        [TestCase(6, true)]
        [TestCase(7, false)]
        public void Then_new_agreement_required_if_signed_agreement_is_below_required_version_based_on_start_date(int? signed, bool expected)
        {
            // Arrange
            var apprenticeships = new[]
            {
                _fixture.Build<IncentiveApplicationApprenticeshipDto>()
                    .With(dto => dto.Phase, Phase.Phase3)
                    .With(dto => dto.PlannedStartDate, new DateTime(2021, 10, 2))
                    .With(dto => dto.StartDatesAreEligible, true)
                    .Without(dto => dto.EmploymentStartDate)
                    .Create()
            };

            var entity = _fixture.Build<LegalEntityDto>()
                .With(dto => dto.SignedAgreementVersion, signed)
                .Create();

            var application = _fixture.Build<IncentiveApplicationDto>()
                .With(dto => dto.Apprenticeships, apprenticeships)
                .With(dto => dto.LegalEntity, entity)
                .Create();

            // Act
            var actual = Incentive.IsNewAgreementRequired(application);

            // Assert
            actual.Should().Be(expected);
        }

        [Test]
        [TestCase(6, "2021-09-30", true)]
        [TestCase(6, "2021-10-01", true)]
        [TestCase(7, "2021-09-30", false)]
        [TestCase(7, "2021-10-01", false)]
        public void Then_new_agreement_required_if_signed_agreement_is_below_required_version_based_on_employment_date(int signed, string date, bool expected)
        {
            // Arrange
            var apprenticeships = new[]
            {
                _fixture.Build<IncentiveApplicationApprenticeshipDto>()
                    .With(dto => dto.Phase, Phase.Phase3)
                    .With(dto => dto.PlannedStartDate, new DateTime(2021, 10, 2))
                    .With(dto => dto.StartDatesAreEligible, true)
                    .With(dto => dto.EmploymentStartDate, DateTime.Parse(date))
                    .Create()
            };

            var entity = _fixture.Build<LegalEntityDto>()
                .With(dto => dto.SignedAgreementVersion, signed)
                .Create();

            var application = _fixture.Build<IncentiveApplicationDto>()
                .With(dto => dto.Apprenticeships, apprenticeships)
                .With(dto => dto.LegalEntity, entity)
                .Create();

            // Act
            var actual = Incentive.IsNewAgreementRequired(application);

            // Assert
            actual.Should().Be(expected);
        }

    }
}
