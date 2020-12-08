using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Commands.AccountVrfCaseStatus;
using SFA.DAS.EmployerIncentives.Commands.SendEmail;
using SFA.DAS.EmployerIncentives.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.AccountVrfCaseStatus.Handlers
{
    [TestFixture]
    public class WhenHandlingAccountVrfCaseStatusRemindersCommand
    {
        private AccountVrfCaseStatusRemindersCommandHandler _sut;
        private Mock<IAccountDataRepository> _accountRepository;
        private Mock<IApprenticeApplicationDataRepository> _applicationRepository;
        private Mock<ICommandDispatcher> _commandDispatcher;
        private Fixture _fixture;
        private DateTime _cutOffDate;

        [SetUp]
        public void Arrange()
        {
            _cutOffDate = DateTime.Today.AddDays(-30);

            _accountRepository = new Mock<IAccountDataRepository>();
            _applicationRepository = new Mock<IApprenticeApplicationDataRepository>();
            _commandDispatcher = new Mock<ICommandDispatcher>();
            _fixture = new Fixture();
            _sut = new AccountVrfCaseStatusRemindersCommandHandler(_accountRepository.Object, _applicationRepository.Object, _commandDispatcher.Object);
        }

        [Test]
        public async Task Then_reminder_emails_are_sent_for_any_accounts_with_applications_and_no_vrf_case_status_after_the_cut_off_date()
        {
            // Arrange
            string vrfCaseStatus = null;

            var accounts = _fixture.CreateMany<AccountDto>(10).ToList();
            var applications = _fixture.CreateMany<ApprenticeApplicationDto>(3).ToList();
            for(var i=0; i < 3; i++)
            {
                applications[i].ApplicationDate = _cutOffDate.AddDays(-1 * (i + 1));
                applications[i].Status = "Submitted";
            }

            _accountRepository.Setup(x => x.GetByVrfCaseStatus(vrfCaseStatus)).ReturnsAsync(accounts);
            _applicationRepository.Setup(x => x.GetList(It.IsAny<long>())).ReturnsAsync(applications);

            // Act
            await _sut.Handle(new AccountVrfCaseStatusRemindersCommand(_cutOffDate));

            // Assert
            _commandDispatcher.Verify(x => x.Send(It.IsAny<SendBankDetailsRepeatReminderEmailCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(10));
        }

        [Test]
        public async Task Then_reminder_emails_are_not_sent_for_any_accounts_with_applications_and_no_vrf_case_status_before_the_cut_off_date()
        {
            // Arrange
            string vrfCaseStatus = null;

            var accounts = _fixture.CreateMany<AccountDto>(10).ToList();
            var applications = _fixture.CreateMany<ApprenticeApplicationDto>(3).ToList();
            for (var i = 0; i < 3; i++)
            {
                applications[i].ApplicationDate = _cutOffDate.AddDays(i);
                applications[i].Status = "Submitted";
            }

            _accountRepository.Setup(x => x.GetByVrfCaseStatus(vrfCaseStatus)).ReturnsAsync(accounts);
            _applicationRepository.Setup(x => x.GetList(It.IsAny<long>())).ReturnsAsync(applications);

            // Act
            await _sut.Handle(new AccountVrfCaseStatusRemindersCommand(_cutOffDate));

            // Assert
            _commandDispatcher.Verify(x => x.Send(It.IsAny<SendBankDetailsRepeatReminderEmailCommand>(), It.IsAny<CancellationToken>()), Times.Never);

        }

        [Test]
        public async Task Then_reminder_emails_are_not_sent_for_any_account_with_no_vrf_case_status_and_no_applications()
        {
            // Arrange
            string vrfCaseStatus = null;

            var accounts = _fixture.CreateMany<AccountDto>(10).ToList();
            var applications = new List<ApprenticeApplicationDto>();

            _accountRepository.Setup(x => x.GetByVrfCaseStatus(vrfCaseStatus)).ReturnsAsync(accounts);
            _applicationRepository.Setup(x => x.GetList(It.IsAny<long>())).ReturnsAsync(applications);

            // Act
            await _sut.Handle(new AccountVrfCaseStatusRemindersCommand(_cutOffDate));

            // Assert
            _commandDispatcher.Verify(x => x.Send(It.IsAny<SendBankDetailsRepeatReminderEmailCommand>(), It.IsAny<CancellationToken>()), Times.Never);

        }

        [Test]
        public async Task Then_reminder_emails_are_not_sent_if_no_vrf_status_and_some_applications_before_the_cut_off_date()
        {
            // Arrange
            string vrfCaseStatus = null;

            var accounts = _fixture.CreateMany<AccountDto>(2).ToList();
            var applications1 = _fixture.CreateMany<ApprenticeApplicationDto>(1).ToList();
            var applications2 = _fixture.CreateMany<ApprenticeApplicationDto>(1).ToList();

            applications1[0].AccountId = accounts[0].AccountId;
            applications1[0].Status = "Submitted";
            applications1[0].ApplicationDate = _cutOffDate.AddDays(5);
            applications2[0].AccountId = accounts[1].AccountId;
            applications2[0].Status = "Submitted";
            applications2[0].ApplicationDate = _cutOffDate.AddDays(-1);

            _accountRepository.Setup(x => x.GetByVrfCaseStatus(vrfCaseStatus)).ReturnsAsync(accounts);
            _applicationRepository.Setup(x => x.GetList(It.Is<long>(x => x == accounts[0].AccountId))).ReturnsAsync(applications1);
            _applicationRepository.Setup(x => x.GetList(It.Is<long>(x => x == accounts[1].AccountId))).ReturnsAsync(applications2);

            // Act
            await _sut.Handle(new AccountVrfCaseStatusRemindersCommand(_cutOffDate));

            // Assert
            _commandDispatcher.Verify(x => x.Send(It.IsAny<SendBankDetailsRepeatReminderEmailCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(1));

        }
    }
}
