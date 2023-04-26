using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PaymentProcess;
using SFA.DAS.EmployerIncentives.Data.Reports;
using SFA.DAS.EmployerIncentives.Data.Reports.Metrics;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.EmployerIncentives.Reports;
using SFA.DAS.EmployerIncentives.Reports.Excel;
using SFA.DAS.Notifications.Messages.Commands;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.PaymentProcess
{
    public class WhenHandlingSendMetricsReportEmailCommand
    {
        private SendMetricsReportEmailCommandHandler _sut;
        private Mock<ICommandPublisher> _mockCommandPublisher;
        private Mock<IOptions<EmailTemplateSettings>> _mockTemplateSettings;
        private EmailTemplateSettings _templates;
        private Mock<IReportsDataRepository> _mockReportsDataRepository;
        private Mock<IReportsRepository> _mockReportsRepository;
        private IConfiguration _configuration;
        private Mock<IExcelReportGenerator<MetricsReport>> _mockExcelReportGenerator;
        private MetricsReport _metricsReport;
        private Domain.ValueObjects.CollectionPeriod _collectionPeriod;
        private string _emailAddress;
        private double _validRecordsAmount;
        private double _invalidRecordsAmount;

        [SetUp]
        public void Arrange()
        {
            _validRecordsAmount = 4.54 * 1000000;
            _invalidRecordsAmount = 72.67 * 1000000;

            _metricsReport = new MetricsReport
            {
                ValidationSummary = new PeriodValidationSummary
                {
                    ValidRecords = new PeriodValidationSummary.ValidationSummaryRecord { PeriodAmount = _validRecordsAmount },
                    InvalidRecords = new PeriodValidationSummary.ValidationSummaryRecord { PeriodAmount = _invalidRecordsAmount }
                },
                CollectionPeriod = new Data.Reports.Metrics.CollectionPeriod { AcademicYear = "2324", Period = 8}
            };

            _mockCommandPublisher = new Mock<ICommandPublisher>();
            _collectionPeriod = new Domain.ValueObjects.CollectionPeriod(8,2324);

            _emailAddress = "some@email.com";

            _mockTemplateSettings = new Mock<IOptions<EmailTemplateSettings>>();
            _templates = new EmailTemplateSettings { MetricsReport = new EmailTemplate { TemplateId = Guid.NewGuid().ToString() } };
            _mockTemplateSettings.Setup(x => x.Value).Returns(_templates);

            _mockReportsDataRepository = new Mock<IReportsDataRepository>();
            _mockReportsRepository = new Mock<IReportsRepository>();
            _mockExcelReportGenerator = new Mock<IExcelReportGenerator<MetricsReport>>();
            _configuration = new ConfigurationBuilder()
                        .AddInMemoryCollection(new Dictionary<string, string>
                        {
                            {"EnvironmentName", "UnitTest" }
                        })
                        .Build();

            _mockReportsRepository
                .Setup(m => m.Get(It.IsAny<ReportsFileInfo>()))
                .ReturnsAsync(new byte[42]);

            _mockReportsDataRepository
                .Setup(m => m.Execute<MetricsReport>())
                .ReturnsAsync(_metricsReport);

            _sut = new SendMetricsReportEmailCommandHandler(
                _mockReportsDataRepository.Object,
                _mockReportsRepository.Object,
                _mockCommandPublisher.Object,
                _mockTemplateSettings.Object,
                _configuration);
        }

        [Test]
        public async Task Then_the_report_is_retrieved_from_the_database()
        {
            //Arrange
            var command = new SendMetricsReportEmailCommand(_collectionPeriod, _emailAddress);

            // Act
            await _sut.Handle(command);

            // Assert
            _mockReportsDataRepository.Verify(m => m.Execute<MetricsReport>(), Times.Once);
        }

        [Test]
        public async Task Then_the_converted_report_is_persisted_to_the_reports_repository_with_the_corrrect_fileInfo()
        {
            //Arrange
            var command = new SendMetricsReportEmailCommand(_collectionPeriod,  _emailAddress);
            var expectedFileInfo = new ReportsFileInfo($"UnitTest Metrics R{_collectionPeriod.PeriodNumber.ToString().PadLeft(2, '0')}_{_collectionPeriod.AcademicYear}", "xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Metrics");

            // Act
            await _sut.Handle(command);

            // Assert
            _mockReportsRepository.Verify(m => m.Get(expectedFileInfo), Times.Once);
        }

        [Test]
        public async Task Then_the_correct_templateId_is_used_when_publishing_the_command()
        {
            //Arrange
            var command = new SendMetricsReportEmailCommand(_collectionPeriod, _emailAddress);
            var expectedTemplateId = _templates.MetricsReport.TemplateId;

            //Act
            await _sut.Handle(command);

            //Assert  
            _mockCommandPublisher.Verify(m => m.Publish(It.Is<SendEmailWithAttachmentsCommand>(m => m.TemplateId == expectedTemplateId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Then_the_correct_email_address_is_used_when_publishing_the_command()
        {
            //Arrange
            var command = new SendMetricsReportEmailCommand(_collectionPeriod, _emailAddress);
            var expectedEmail = _emailAddress;

            //Act
            await _sut.Handle(command);

            //Assert  
            _mockCommandPublisher.Verify(m => m.Publish(It.Is<SendEmailWithAttachmentsCommand>(m => m.RecipientsAddress == expectedEmail), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Then_the_correct_personalisation_tokens_are_used_when_publishing_the_command()
        {
            //Arrange
            var command = new SendMetricsReportEmailCommand(_collectionPeriod, _emailAddress);

            //Act
            await _sut.Handle(command);

            //Assert  
            _mockCommandPublisher.Verify(m => m.Publish(It.Is<SendEmailWithAttachmentsCommand>(m => 
                m.Tokens.ContainsKey("periodName") && m.Tokens["periodName"] == "R08" &&
                m.Tokens.ContainsKey("academicYear") && m.Tokens["academicYear"] == "2324" &&
                m.Tokens.ContainsKey("amountToBePaid") && m.Tokens["amountToBePaid"] == "4.54m" && 
                m.Tokens.ContainsKey("amountFailingValidation") && m.Tokens["amountFailingValidation"] == "72.67m" &&
                m.Tokens.Count == 4), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestCase(8500, 9500, "8.5k", "9.5k")]
        [TestCase(123000, 216570, "123k", "216.57k")]
        [TestCase(1230700, 2167570, "1.23m", "2.17m")]
        [TestCase(14230700, 32167570, "14.23m", "32.17m")]
        public async Task Then_the_amounts_are_formatted_correctly_for_different_monetary_sizes(double validRecordsAmount, double invalidRecordsAmount, string expectedValidRecordsText, string expectedInvalidRecordsText)
        {
            // Arrange
            _metricsReport = new MetricsReport
            {
                ValidationSummary = new PeriodValidationSummary
                {
                    ValidRecords = new PeriodValidationSummary.ValidationSummaryRecord { PeriodAmount = validRecordsAmount },
                    InvalidRecords = new PeriodValidationSummary.ValidationSummaryRecord { PeriodAmount = invalidRecordsAmount }
                },
                CollectionPeriod = new Data.Reports.Metrics.CollectionPeriod { AcademicYear = "2324", Period = 8 }
            };

            _mockReportsDataRepository
                .Setup(m => m.Execute<MetricsReport>())
                .ReturnsAsync(_metricsReport);

            var command = new SendMetricsReportEmailCommand(_collectionPeriod, _emailAddress);

            //Act
            await _sut.Handle(command);

            //Assert  
            _mockCommandPublisher.Verify(m => m.Publish(It.Is<SendEmailWithAttachmentsCommand>(m =>
                m.Tokens.ContainsKey("periodName") && m.Tokens["periodName"] == "R08" &&
                m.Tokens.ContainsKey("academicYear") && m.Tokens["academicYear"] == "2324" &&
                m.Tokens.ContainsKey("amountToBePaid") && m.Tokens["amountToBePaid"] == expectedValidRecordsText &&
                m.Tokens.ContainsKey("amountFailingValidation") && m.Tokens["amountFailingValidation"] == expectedInvalidRecordsText &&
                m.Tokens.Count == 4), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Then_the_correct_attachments_are_used_when_publishing_the_command()
        {
            //Arrange
            var command = new SendMetricsReportEmailCommand(_collectionPeriod, _emailAddress);

            //Act
            await _sut.Handle(command);

            //Assert  
            _mockCommandPublisher.Verify(m => m.Publish(It.Is<SendEmailWithAttachmentsCommand>(m =>
                m.AttachmentsDataBus.ContainsKey("metricsReportDownloadLink") &&
                m.AttachmentsDataBus["metricsReportDownloadLink"].Length == 42), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
