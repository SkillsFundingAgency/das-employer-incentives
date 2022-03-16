using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Reports.Respositories
{
    public class AzureBlobRepository : IReportsRepository
    {
        private readonly string _connectionString;
        private readonly string _containerName;

        public AzureBlobRepository(IOptions<ApplicationSettings> options)
        {
            _connectionString = options.Value.ReportsConnectionString;
            _containerName = options.Value.ReportsContainerName;
        }

        public async Task Save(ReportsFileInfo fileInfo, MemoryStream memoryStream)
        {
            var container = new BlobContainerClient(_connectionString, _containerName);
            container.CreateIfNotExists();

            var blob = container.GetBlobClient($"{fileInfo.Folder}/{fileInfo.Name}.{fileInfo.Extension}");

            memoryStream.Position = 0;            
            await blob.UploadAsync(memoryStream, new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = fileInfo.ContentType } });
        }
    }
}
