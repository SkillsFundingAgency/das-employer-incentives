using System;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.IncentiveApplicationTests
{
    public class WhenCreated
    {
        private IncentiveApplicationFactory _sut;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _sut = new IncentiveApplicationFactory();
            _fixture = new Fixture();
        }

        [Test]
        public void Then_the_id_is_set()
        {
            // Arrange
            var id = _fixture.Create<Guid>();

            // Act
            var application = _sut.CreateNew(id, _fixture.Create<long>(), _fixture.Create<long>());

            // Assert
            application.Id.Should().Be(id);
        }

        [Test]
        public void Then_the_account_id_is_set()
        {
            // Arrange
            var accountId = _fixture.Create<long>();

            // Act
            var application = _sut.CreateNew(_fixture.Create<Guid>(), accountId, _fixture.Create<long>());

            // Assert
            application.AccountId.Should().Be(accountId);
        }

        [Test]
        public void Then_the_account_legal_entity_id_is_set()
        {
            // Arrange
            var accountLegalEntityId = _fixture.Create<long>();

            // Act
            var application = _sut.CreateNew(_fixture.Create<Guid>(), _fixture.Create<long>(), accountLegalEntityId);

            // Assert
            application.AccountLegalEntityId.Should().Be(accountLegalEntityId);
        }

        [Test]
        public void Then_the_status_is_set_to_in_progress()
        {
            // Act
            var application = _sut.CreateNew(_fixture.Create<Guid>(), _fixture.Create<long>(), _fixture.Create<long>());

            // Assert
            application.Status.Should().Be(IncentiveApplicationStatus.InProgress);
        }
    }
}
