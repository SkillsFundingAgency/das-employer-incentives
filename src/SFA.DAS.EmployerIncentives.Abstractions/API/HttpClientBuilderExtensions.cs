using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Http.MessageHandlers;
using SFA.DAS.Http.TokenGenerators;
using System;
using System.Net.Http;

namespace SFA.DAS.EmployerIncentives.Abstractions.API
{
    public static class HttpClientBuilderExtensions
    {
        public static IHttpClientBuilder WithDefaultHeaders(this IHttpClientBuilder httpClientBuilder)
        {
            return httpClientBuilder.AddHttpMessageHandler(s => new DefaultHeadersHandler());
        }

        public static IHttpClientBuilder WithLogging(this IHttpClientBuilder httpClientBuilder)
        {
            return httpClientBuilder.AddHttpMessageHandler(s => new LoggingMessageHandler(s.GetService<ILoggerFactory>().CreateLogger<LoggingMessageHandler>()));
        }

        public static IHttpClientBuilder WithHandler(this IHttpClientBuilder httpClientBuilder, Func<IServiceProvider, DelegatingHandler> handler)
        {
            return httpClientBuilder.AddHttpMessageHandler(handler);
        }

        public static IHttpClientBuilder WithBaseUrl<T>(
            this IHttpClientBuilder httpClientBuilder)
            where T : IOptions<ApiBase>
        {
            return httpClientBuilder.ConfigureHttpClient((serviceProvider, client) =>
            {
                var settings = serviceProvider.GetService<T>().Value;
                if (!settings.ApiBaseUrl.EndsWith("/"))
                {
                    settings.ApiBaseUrl += "/";
                }

                client.BaseAddress = new Uri(settings.ApiBaseUrl);
            });
        }

        public static IHttpClientBuilder WithManagedIdentityAuthorisationHeader<T>(
            this IHttpClientBuilder httpClientBuilder)
            where T : IOptions<ManagedIdentityApiBase>
        {
            return httpClientBuilder.AddHttpMessageHandler(s =>
            {
                var settings = s.GetService<T>().Value;

                if (!string.IsNullOrEmpty(settings.Identifier))
                {
                    return new ManagedIdentityHeadersHandler(new ManagedIdentityTokenGenerator(settings));
                }

                return new DummyHandler();
            });
        }

        public static IHttpClientBuilder WithApimAuthorisationHeader<T>(
            this IHttpClientBuilder httpClientBuilder)
            where T : IOptions<ApimBase>
        {
            return httpClientBuilder.AddHttpMessageHandler(serviceProvider =>
            {
                var settings = serviceProvider.GetService<T>().Value;

                return new ApimHeadersHandler(settings);
            });
        }
    }
}
