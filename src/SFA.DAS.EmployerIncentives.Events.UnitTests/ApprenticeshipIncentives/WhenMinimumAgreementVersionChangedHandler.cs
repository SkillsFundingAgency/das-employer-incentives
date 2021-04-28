using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives;
using SFA.DAS.HashingService;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;

namespace SFA.DAS.EmployerIncentives.Events.UnitTests.ApprenticeshipIncentives
{
    public class WhenMinimumAgreementVersionChangedNotificationHandler
    {
        private MinimumAgreementVersionChangedNotificationHandler _sut;
        private Mock<ICommandPublisher> _mockCommandPublisher;
        
        private Fixture _fixture;
        private MinimumAgreementVersionChanged _event;
        private LegalEntityDto _legalEntityDto;
        private string _hashedAccountId;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _mockCommandPublisher = new Mock<ICommandPublisher>();

            _legalEntityDto = _fixture.Create<LegalEntityDto>();
            _event = _fixture.Create<MinimumAgreementVersionChanged>();

             _legalEntityDto = _fixture.Build<LegalEntityDto>().With(x => x.AccountId, _event.Model.Account.Id).Create();
            var queryRepository = new Mock<IQueryRepository<LegalEntityDto>>();

            var accountLegalEntityId = _event.Model.Account.AccountLegalEntityId;

            // not working 🤷‍♂
            // queryRepository.Setup(x => x.Get(a => a.AccountId == _event.Model.Account.Id
            //&& a.AccountLegalEntityId == _event.Model.Account.AccountLegalEntityId)).ReturnsAsync(_legalEntityDto);

            queryRepository.Setup(x => x.Get(
                It.IsAny<Expression<Func<LegalEntityDto, bool>>>())).ReturnsAsync(_legalEntityDto);

            _hashedAccountId = _fixture.Create<string>();   
            var hashingService = new Mock<IHashingService>();
            hashingService.Setup(x => x.HashValue(_event.Model.Account.Id)).Returns(_hashedAccountId);

            _sut = new MinimumAgreementVersionChangedNotificationHandler(_mockCommandPublisher.Object,
                queryRepository.Object, hashingService.Object);
        }

        [Test]
        public async Task Then_a_NotifyNewAgreementRequiredCommand_is_published_with_correct_data()
        {
            //Act
            await _sut.Handle(_event);

            //Assert
            _mockCommandPublisher.Verify(m => m.Publish(It.Is<NotifyNewAgreementRequiredCommand>(i =>
                i.LegalEntityName == _legalEntityDto.LegalEntityName && i.HashedAccountId == _hashedAccountId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Then_a_NotifyNewAgreementRequiredCommand_is_published_with_correct_EmailTemplateId()
        {
            //Act
            await _sut.Handle(_event);

            //Assert
            NotifyNewAgreementRequiredCommand.TemplateId.Should().Be("NewAgreementVersionNeedsAccepting");
        }
    }
}
