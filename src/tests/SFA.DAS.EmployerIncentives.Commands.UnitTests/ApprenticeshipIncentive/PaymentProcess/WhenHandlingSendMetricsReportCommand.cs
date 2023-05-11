using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PaymentProcess;
using SFA.DAS.EmployerIncentives.Data.Reports;
using SFA.DAS.EmployerIncentives.Data.Reports.Metrics;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Reports;
using SFA.DAS.EmployerIncentives.Reports.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.PaymentProcess
{
    public class WhenHandlingSendMetricsReportCommand
    {
        private SendMetricsReportCommandHandler _sut;
        private Mock<IReportsDataRepository> _mockReportsDataRepository;
        private Mock<IReportsRepository> _mockReportsRepository;
        private IConfiguration _configuration;
        private Mock<IExcelReportGenerator<MetricsReport>> _mockExcelReportGenerator;
        private Mock<IDomainEventDispatcher> _mockDomainEventDispatcher;        
        private MetricsReport _metricsReport;
        private Domain.ValueObjects.CollectionPeriod _collectionPeriod;
        private string _containerName;

        [SetUp]
        public void Arrange()
        {
            _metricsReport = new MetricsReport();
            _containerName = Guid.NewGuid().ToString();
            _collectionPeriod = new Domain.ValueObjects.CollectionPeriod(It.IsAny<byte>(), It.IsAny<short>());

            _mockReportsDataRepository = new Mock<IReportsDataRepository>();
            _mockReportsRepository = new Mock<IReportsRepository>();
            _mockExcelReportGenerator = new Mock<IExcelReportGenerator<MetricsReport>>();
            _mockDomainEventDispatcher = new Mock<IDomainEventDispatcher>();

            _configuration = new ConfigurationBuilder()
                        .AddInMemoryCollection(new Dictionary<string, string>
                        {
                            {"EnvironmentName", "UnitTest" }
                        })
                        .Build();

            _mockReportsDataRepository
                .Setup(m => m.Execute<MetricsReport>())
                .ReturnsAsync(_metricsReport);            

            _sut = new SendMetricsReportCommandHandler(
                _mockReportsDataRepository.Object,
                _mockReportsRepository.Object,
                _mockExcelReportGenerator.Object,
                _configuration,
                _mockDomainEventDispatcher.Object);
        }

        [Test]
        public async Task Then_the_report_is_retrieved_from_the_database()
        {
            //Arrange
            var command = new SendMetricsReportCommand(_collectionPeriod);

            // Act
            await _sut.Handle(command);

            // Assert
            _mockReportsDataRepository.Verify(m => m.Execute<MetricsReport>(), Times.Once);
        }

        [Test]
        public async Task Then_the_report_is_converted_to_an_excel_stream()
        {
            //Arrange
            var command = new SendMetricsReportCommand(_collectionPeriod);

            // Act
            await _sut.Handle(command);

            // Assert
            _mockExcelReportGenerator.Verify(m => m.Create(_metricsReport), Times.Once);
        }

        [Test]
        public async Task Then_the_converted_report_is_persisted_to_the_reports_repository_with_the_corrrect_fileInfo()
        {
            //Arrange
            var command = new SendMetricsReportCommand(_collectionPeriod);
            var expectedFileInfo = new ReportsFileInfo($"UnitTest Metrics R{_collectionPeriod.PeriodNumber.ToString().PadLeft(2,'0')}_{_collectionPeriod.AcademicYear}", "xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Metrics");

            // Act
            await _sut.Handle(command);

            // Assert
            _mockReportsRepository.Verify(m => m.Save(expectedFileInfo, It.IsAny<Stream>()), Times.Once);
        }

        [Test]
        public async Task Then_the_converted_report_is_streamed_to_the_reports_repository()
        {
            //Arrange
            var command = new SendMetricsReportCommand(_collectionPeriod);

            using var ms = new MemoryStream();
            _mockExcelReportGenerator
                .Setup(m => m.Create(_metricsReport))
                .Returns(ms);

            // Act
            await _sut.Handle(command);

            // Assert
            _mockReportsRepository.Verify(m => m.Save(It.IsAny<ReportsFileInfo>(), ms), Times.Once);
        }

        [Test]
        public async Task Then_a_report_generated_event_is_raised()
        {
            //Arrange
            var command = new SendMetricsReportCommand(_collectionPeriod);

            using var ms = new MemoryStream();
            _mockExcelReportGenerator
                .Setup(m => m.Create(_metricsReport))
                .Returns(ms);

            // Act
            await _sut.Handle(command);

            // Assert
            _mockDomainEventDispatcher.Verify(m => m.Send(
                It.Is<MetricsReportGenerated>( e => e.CollectionPeriod == command.CollectionPeriod), It.IsAny<CancellationToken>()),
                Times.Once);
        }

    }
}
