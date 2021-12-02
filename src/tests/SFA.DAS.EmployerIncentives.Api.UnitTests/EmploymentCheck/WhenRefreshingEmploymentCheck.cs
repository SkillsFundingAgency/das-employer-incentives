using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.EmploymentCheck;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.EmploymentCheck
{
    [TestFixture]
    public class WhenRefreshingEmploymentCheck
    {
        private EmploymentCheckController _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;
        
        [SetUp]
        public void Setup()
        {
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            _sut = new EmploymentCheckController(_mockCommandDispatcher.Object);
        }

        [Test]
        public async Task Then_a_refresh_employment_checks_command_is_dispatched()
        {
            // Arrange / Act
            await _sut.Refresh();

            // Assert
            _mockCommandDispatcher.Verify(m => m.Send(It.IsAny<RefreshEmploymentChecksCommand>(),  It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Then_an_Ok_response_is_returned()
        {
            // Arrange / Act
            var actual = await _sut.Refresh() as OkResult;

            // Assert
            actual.Should().NotBeNull();
        }

    }
}