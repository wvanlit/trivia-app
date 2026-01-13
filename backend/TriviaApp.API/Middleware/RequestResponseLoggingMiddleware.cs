using System.Diagnostics.CodeAnalysis;
using System.Text;
using TriviaApp.API.Extensions;

namespace TriviaApp.API.Middleware;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by the middleware pipeline.")]
internal sealed class RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
{
    private static readonly PathString ApiPath = new("/api");
    private const int ResponseBodyLimit = 4 * 1024;
    private const int RequestBodyLimit = 4 * 1024;
    private readonly RequestDelegate _next = next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments(ApiPath, StringComparison.InvariantCultureIgnoreCase))
        {
            await _next(context);
            return;
        }

        var method = context.Request.Method;
        var path = context.Request.Path;
        var queryString = context.Request.QueryString;
        var originalBody = context.Response.Body;

        await using var responseBody = new MemoryStream();

        context.Response.Body = responseBody;

        if (HttpMethods.IsPost(method))
        {
            var requestBody = await ReadRequestBody(context.Request);
            _logger.LogRequestStartedWithBody(method, path, queryString, requestBody);
        }
        else
        {
            _logger.LogRequestStarted(method, path, queryString);
        }

        try
        {
            await _next(context);
        }
        finally
        {
            responseBody.Seek(0, SeekOrigin.Begin);

            var bodyText = await ReadResponseBody(responseBody);
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBody);
            context.Response.Body = originalBody;

            _logger.LogResponse(
                method,
                path,
                queryString,
                context.Response.StatusCode,
                bodyText);
        }
    }

    private static async Task<string> ReadResponseBody(Stream responseBody)
    {
        using var reader = new StreamReader(
            responseBody,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();

        return body.Length <= ResponseBodyLimit ? body : body[..ResponseBodyLimit];
    }

    private static async Task<string> ReadRequestBody(HttpRequest request)
    {
        request.EnableBuffering();

        using var reader = new StreamReader(
            request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        return body.Length <= RequestBodyLimit ? body : body[..RequestBodyLimit];
    }
}
