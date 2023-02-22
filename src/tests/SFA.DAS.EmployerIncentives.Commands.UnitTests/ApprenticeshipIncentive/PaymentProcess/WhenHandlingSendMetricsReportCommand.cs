using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PaymentProcess;
using SFA.DAS.EmployerIncentives.Data.Reports;
using SFA.DAS.EmployerIncentives.Data.Reports.Metrics;
using SFA.DAS.EmployerIncentives.Reports;
using SFA.DAS.EmployerIncentives.Reports.Excel;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.PaymentProcess
{
    public class WhenHandlingSendMetricsReportCommand
    {
        private SendMetricsReportCommandHandler _sut;
        private Mock<IReportsDataRepository> _mockReportsDataRepository;
        private Mock<IReportsRepository> _mockReportsRepository;
        private Mock<IExcelReportGenerator<MetricsReport>> _mockExcelReportGenerator;
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

            _mockReportsDataRepository
                .Setup(m => m.Execute<MetricsReport>())
                .ReturnsAsync(_metricsReport);

            _sut = new SendMetricsReportCommandHandler(
                _mockReportsDataRepository.Object,
                _mockReportsRepository.Object,
                _mockExcelReportGenerator.Object);
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
            var expectedFileInfo = new ReportsFileInfo($"{_collectionPeriod.AcademicYear}_R{_collectionPeriod.PeriodNumber.ToString().PadLeft(2,'0')}_Metrics", "xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Metrics");

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
    }
}
