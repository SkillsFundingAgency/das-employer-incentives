using Dapper.Contrib.Extensions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Hooks;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Services;
using SFA.DAS.EmployerIncentives.TestHelpers.Services;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.EmployerIncentives.TestHelpers.Types;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests
{
    public class TestContext : IDisposable
    {
        public string InstanceId { get; private set; }

        public string SqlDataSource { get; private set; }

        public DirectoryInfo TestDirectory { get; set; }
        public TestFunction TestFunction { get; set; }        

        public TestData TestData { get; set; }        
        public List<IHook> Hooks { get; set; }
        public SqlDatabase SqlDatabase { get; set; }
        public ApplicationSettings ApplicationSettings { get; set; }        

        public MockApi LearnerMatchApi { get; set; }
        
        public MockApi PaymentsApi { get; set; }
        public Data.ApprenticeshipIncentives.Models.CollectionCalendarPeriod ActivePeriod { get; set; }

        public static HttpRequest TestRequest(string path, object body = null) => CreateTestRequest(path, body);

        public TestContext()
        {
            InstanceId = Guid.NewGuid().ToString();
            TestDirectory = new DirectoryInfo(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent.Parent.FullName, $"TestDirectory/{InstanceId}"));
            if (!TestDirectory.Exists)
            {
                Directory.CreateDirectory(TestDirectory.FullName);
            }
            TestData = new TestData();
            Hooks = new List<IHook>();
            ApplicationSettings = new ApplicationSettings
            {
                DistributedLockStorage = "UseDevelopmentStorage=true",
                NServiceBusConnectionString = "UseLearningEndpoint=true",
                UseLearningEndpointStorageDirectory = Path.Combine(TestDirectory.FullName, ".learningtransport"),
            };
        }
        public void Initialise(SqlServerImageInfo sqlServerImageInfo)
        {
            SqlDataSource = sqlServerImageInfo.DataSource;
        }

        private bool _isDisposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }        

        public async Task SetActiveCollectionCalendarPeriod(CollectionPeriod collectionPeriod)
        {
            await using var dbConnection = new SqlConnection(SqlDatabase.DatabaseInfo.ConnectionString);
            var calendar = await dbConnection.GetAllAsync<Data.ApprenticeshipIncentives.Models.CollectionCalendarPeriod>();
            var currentActivePeriod = calendar.SingleOrDefault(x => x.Active);
            if (currentActivePeriod != null)
            {
                currentActivePeriod.Active = false;
                await dbConnection.UpdateAsync(currentActivePeriod);
            }
            var period = calendar.Single(x => x.AcademicYear == collectionPeriod.Year.ToString() && x.PeriodNumber == collectionPeriod.Period);
            period.Active = true;

            await dbConnection.UpdateAsync(period);
            ActivePeriod = period;
        }

        internal async Task SetActiveCollectionCalendarPeriod(short year, byte number)
        {
            await SetActiveCollectionCalendarPeriod(new CollectionPeriod {Year = year, Period = number});
        }

        public async Task<Data.ApprenticeshipIncentives.Models.CollectionCalendarPeriod> GetCollectionCalendarPeriod(DateTime date)
        {
            await using var dbConnection = new SqlConnection(SqlDatabase.DatabaseInfo.ConnectionString);
            var calendar = await dbConnection.GetAllAsync<Data.ApprenticeshipIncentives.Models.CollectionCalendarPeriod>();

            var period = calendar
                    .Where(d => d.CensusDate >= date)
                    .OrderBy(d => d.CensusDate)
                    .FirstOrDefault();

            return period;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                LearnerMatchApi?.Reset();
                PaymentsApi?.Reset();
            }

            _isDisposed = true;
        }

        private static HttpRequest CreateTestRequest(string path, object body = null)
        {
            if (!path.StartsWith("/"))
            {
                path = string.Concat("/", path);
            }

            var json = body == null ? "{}" : JsonSerializer.Serialize(body);

            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Body = memoryStream;
            request.ContentType = "application/json";
            request.Method = HttpMethod.Get.ToString();
            request.Scheme = "https";
            request.Host = new HostString("localhost:7071");
            request.IsHttps = false;
            request.Query = new QueryCollection();
            request.Path = path;
            request.QueryString = QueryString.Empty;
            request.Protocol = string.Empty;
            request.ContentLength = request.Body.Length;
            request.Form = new FormCollection(new Dictionary<string, StringValues>());


            return request;
        }
    }
}