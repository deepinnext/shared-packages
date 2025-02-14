using System;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Retry;
using Polly.Timeout;

namespace Deepin.Infrastructure.Extensions;

public static class HttpClientExtensions
{
    private static readonly int DefaultTimeout = 30;
    private static readonly IEnumerable<TimeSpan> Delay = Backoff.LinearBackoff(TimeSpan.FromMilliseconds(200), 3);
    private static readonly AsyncRetryPolicy<HttpResponseMessage> RetryPolicy = HttpPolicyExtensions
        .HandleTransientHttpError()
        .Or<TimeoutRejectedException>()
        .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
        .WaitAndRetryAsync(Delay);
    private static readonly AsyncTimeoutPolicy<HttpResponseMessage> TimeoutPolicy =
        Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(DefaultTimeout));

    public static IHttpClientBuilder AddDefaultPolicies(this IHttpClientBuilder builder)
    {
        builder
        .AddPolicyHandler(RetryPolicy)
        .AddPolicyHandler(TimeoutPolicy)
        .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();
        return builder;
    }
}
