using TriviaApp.API.Middleware;

namespace TriviaApp.API.Extensions;

internal static class RequestResponseLoggingExtensions
{
    internal static void LogRequestStarted(
        this ILogger<RequestResponseLoggingMiddleware> logger,
        string method,
        PathString path,
        QueryString queryString) =>
        logger.LogInformation(
            "HTTP {Method} {Path}{QueryString} started",
            method,
            path,
            queryString);

    internal static void LogRequestStartedWithBody(
        this ILogger<RequestResponseLoggingMiddleware> logger,
        string method,
        PathString path,
        QueryString queryString,
        string requestBody) =>
        logger.LogInformation(
            "HTTP {Method} {Path}{QueryString} started with body {RequestBody}",
            method,
            path,
            queryString,
            requestBody);

    internal static void LogResponse(
        this ILogger<RequestResponseLoggingMiddleware> logger,
        string method,
        PathString path,
        QueryString queryString,
        int statusCode,
        string responseBody) =>
        logger.LogInformation(
            "HTTP {Method} {Path}{QueryString} responded {StatusCode} with body {ResponseBody}",
            method,
            path,
            queryString,
            statusCode,
            responseBody);
}
