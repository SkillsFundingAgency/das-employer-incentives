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
        private readonly bool _canConnect;

        public AzureBlobRepository(IOptions<ApplicationSettings> options)
        {
            _connectionString = options.Value.ReportsConnectionString;
            _canConnect = !string.IsNullOrEmpty(_connectionString);
            _containerName = string.IsNullOrEmpty(options.Value.ReportsContainerName) ? "reports" : options.Value.ReportsContainerName.ToLowerInvariant();
        }

        public async Task Save(ReportsFileInfo fileInfo, Stream stream)
        {
            if (!_canConnect)
            {
                return;
            }

            var container = new BlobContainerClient(_connectionString, _containerName);
            container.CreateIfNotExists();

            var blob = container.GetBlobClient($"{fileInfo.Folder}/{fileInfo.Name}.{fileInfo.Extension}");

            using (var ms = new MemoryStream())
            {
                await blob.DownloadToAsync(ms);
                var byteArray = ms.ToArray();
            }

            stream.Position = 0;            
            await blob.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = fileInfo.ContentType } });
        }

        public async Task<byte[]> Get(ReportsFileInfo fileInfo)
        {
            if (!_canConnect)
            {
                return null;
            }

            var container = new BlobContainerClient(_connectionString, _containerName);

            var blob = container.GetBlobClient($"{fileInfo.Folder}/{fileInfo.Name}.{fileInfo.Extension}");

            using var ms = new MemoryStream();
            await blob.DownloadToAsync(ms);
            var byteArray = ms.ToArray();

            return byteArray;
        }
    }
}
