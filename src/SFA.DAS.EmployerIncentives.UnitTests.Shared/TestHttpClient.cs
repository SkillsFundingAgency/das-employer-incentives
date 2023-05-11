using Microsoft.AspNetCore.Http;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.UnitTests.Shared
{
    public class TestHttpClient : HttpClient
    {
        public readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;

        public TestHttpClient()
            : this(new Mock<HttpMessageHandler>(), new Uri("https://localhost:5001"))
        {
        }

        public TestHttpClient(Uri baseAddress)
            : this(new Mock<HttpMessageHandler>(), baseAddress)
        {
        }

        private TestHttpClient(Mock<HttpMessageHandler> mockHttpMessageHandler, Uri baseAddress) : base(mockHttpMessageHandler.Object)
        {
            _mockHttpMessageHandler = mockHttpMessageHandler;
            BaseAddress = baseAddress;
        }

        public void VerifyContentType(string contentType)
        {
            _mockHttpMessageHandler
             .Protected()
             .Verify("SendAsync", Times.AtLeastOnce(),
             ItExpr.Is<HttpRequestMessage>(r =>
             r.Content.Headers.ContentType.MediaType == contentType
             ), ItExpr.IsAny<CancellationToken>());
        }

        public void VerifyPostAsAsync<T>(string relativePath, T value, Times times)
        {
            _mockHttpMessageHandler
             .Protected()
             .Verify("SendAsync", times,
             ItExpr.Is<HttpRequestMessage>(r =>             
             r.Method == HttpMethod.Post &&
             r.RequestUri.AbsoluteUri == $"{BaseAddress.AbsoluteUri}{relativePath}" &&
             r.Content.ReadAsStringAsync().Result == JsonConvert.SerializeObject(value)
             ), ItExpr.IsAny<CancellationToken>());
        }

        public void VerifyPostAsAsync(string relativePath,Times times)
        {
            _mockHttpMessageHandler
             .Protected()
             .Verify("SendAsync", times,
             ItExpr.Is<HttpRequestMessage>(r =>
             r.Method == HttpMethod.Post &&
             r.RequestUri.AbsoluteUri == $"{BaseAddress.AbsoluteUri}{relativePath}"
             ), ItExpr.IsAny<CancellationToken>());
        }

        public void VerifyPutAsAsync<T>(string relativePath, T value, Times times)
        {
            _mockHttpMessageHandler
             .Protected()
             .Verify("SendAsync", times,
             ItExpr.Is<HttpRequestMessage>(r =>
             r.Method == HttpMethod.Put &&
             r.RequestUri.AbsoluteUri == $"{BaseAddress.AbsoluteUri}{relativePath}" &&
             r.Content.ReadAsStringAsync().Result == JsonConvert.SerializeObject(value)
             ), ItExpr.IsAny<CancellationToken>());
        }

        public void VerifyGetAsAsync(string relativePath, Times times)
        {
            _mockHttpMessageHandler
             .Protected()
             .Verify("SendAsync", times,
             ItExpr.Is<HttpRequestMessage>(r =>
             r.Method == HttpMethod.Get &&
             r.RequestUri.AbsoluteUri == $"{BaseAddress.AbsoluteUri}{relativePath}"
             ), ItExpr.IsAny<CancellationToken>());
        }

        public void VerifyDeleteAsync(string relativePath, Times times)
        {
            _mockHttpMessageHandler
             .Protected()
             .Verify("SendAsync", times,
             ItExpr.Is<HttpRequestMessage>(r =>
             r.Method == HttpMethod.Delete &&
             r.RequestUri.AbsoluteUri == $"{BaseAddress.AbsoluteUri}{relativePath}"
             ), ItExpr.IsAny<CancellationToken>());
        }

        public void SetUpGetAsAsync(HttpStatusCode statusCode)
        {
            _mockHttpMessageHandler
                  .Protected()
                  .Setup<Task<HttpResponseMessage>>("SendAsync",
                      ItExpr.IsAny<HttpRequestMessage>(),
                      ItExpr.IsAny<CancellationToken>())
                  .ReturnsAsync(new HttpResponseMessage(statusCode)).Verifiable("");
        }

        public void SetUpGetAsAsync<T>(T response, HttpStatusCode statusCode)
        {
            var content = new StringContent(JsonConvert.SerializeObject(response));

            SetUpGetAsAsync(content, statusCode);
        }

        public void SetUpGetAsAsync(StringContent content, HttpStatusCode statusCode)
        {
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(statusCode) { Content = content }).Verifiable("");
        }

        public void SetUpPostAsAsync(HttpStatusCode statusCode)
        {
            _mockHttpMessageHandler
                  .Protected()
                  .Setup<Task<HttpResponseMessage>>("SendAsync",
                      ItExpr.IsAny<HttpRequestMessage>(),
                      ItExpr.IsAny<CancellationToken>())
                  .ReturnsAsync(new HttpResponseMessage(statusCode)).Verifiable("");
        }

        public void SetUpPostAsAsync<T>(T value, HttpStatusCode statusCode)
        {
            _mockHttpMessageHandler
                  .Protected()
                  .Setup<Task<HttpResponseMessage>>("SendAsync",
                      ItExpr.Is<HttpRequestMessage>(r => r.Content.ReadAsStringAsync().Result == JsonConvert.SerializeObject(value)),
                      ItExpr.IsAny<CancellationToken>())
                  .ReturnsAsync(new HttpResponseMessage(statusCode)).Verifiable("");
        }

        public void SetUpDeleteAsAsync(HttpStatusCode statusCode)
        {
            _mockHttpMessageHandler
                 .Protected()
                 .Setup<Task<HttpResponseMessage>>("SendAsync",
                     ItExpr.IsAny<HttpRequestMessage>(),
                     ItExpr.IsAny<CancellationToken>())
                 .ReturnsAsync(new HttpResponseMessage(statusCode)).Verifiable("");
        }

        public void SetUpPutAsAsync(HttpStatusCode statusCode)
        {
            _mockHttpMessageHandler
                  .Protected()
                  .Setup<Task<HttpResponseMessage>>("SendAsync",
                      ItExpr.IsAny<HttpRequestMessage>(),
                      ItExpr.IsAny<CancellationToken>())
                  .ReturnsAsync(new HttpResponseMessage(statusCode)).Verifiable("");
        }

        public void SetUpPutAsAsync<T>(T value, HttpStatusCode statusCode)
        {
            _mockHttpMessageHandler
                  .Protected()
                  .Setup<Task<HttpResponseMessage>>("SendAsync",
                      ItExpr.Is<HttpRequestMessage>(r => r.Content.ReadAsStringAsync().Result == JsonConvert.SerializeObject(value)),
                      ItExpr.IsAny<CancellationToken>())
                  .ReturnsAsync(new HttpResponseMessage(statusCode)).Verifiable("");
        }
        public static HttpRequest CreateHttpRequest(object body)
        {
            var mockRequest = new Mock<HttpRequest>();

            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);

            var json = JsonConvert.SerializeObject(body);

            sw.Write(json);
            sw.Flush();

            ms.Position = 0;

            mockRequest.Setup(x => x.Body).Returns(ms);

            return mockRequest.Object;
        }
    }
}
