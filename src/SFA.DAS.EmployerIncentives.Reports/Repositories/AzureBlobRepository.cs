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
            _containerName = string.IsNullOrEmpty(options.Value.ReportsContainerName) ? "Reports" : options.Value.ReportsContainerName;
        }

        public async Task Save(ReportsFileInfo fileInfo, Stream stream)
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                return;
            }

            var container = new BlobContainerClient(_connectionString, _containerName);
            container.CreateIfNotExists();

            var blob = container.GetBlobClient($"{fileInfo.Folder}/{fileInfo.Name}.{fileInfo.Extension}");

            stream.Position = 0;            
            await blob.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = fileInfo.ContentType } });
        }
    }
}
