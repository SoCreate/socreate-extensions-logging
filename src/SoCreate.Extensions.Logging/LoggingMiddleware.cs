using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Core;
using Serilog.Events;
using SoCreate.Extensions.Logging.Options;

namespace SoCreate.Extensions.Logging;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;
    private readonly LoggingLevelSwitch _loggingLevelSwitch;
    private readonly bool _showTestOutput;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger,
        LoggingLevelSwitch loggingLevelSwitch, IOptions<LoggingMiddlewareOptions> logOptions)
    {
        _next = next;
        _logger = logger;
        _loggingLevelSwitch = loggingLevelSwitch;
        _showTestOutput = logOptions.Value.ShowTestOutput;
    }

    public async Task Invoke(HttpContext context)
    {
        // Only log response/requests when in debug mode or lower
        if (_loggingLevelSwitch.MinimumLevel <= LogEventLevel.Debug || _showTestOutput)
        {
            //First, get the incoming request
            var request = await FormatRequest(context.Request);
            _logger.LogDebug(request);
            //Copy a pointer to the original response body stream
            var originalBodyStream = context.Response.Body;

            //Create a new memory stream...
            using (var responseBody = new MemoryStream())
            {
                //...and use that for the temporary response body
                context.Response.Body = responseBody;

                //Continue down the Middleware pipeline, eventually returning to this class
                await _next(context);

                //Format the response from the server
                var response = await FormatResponse(context.Response);
                _logger.LogDebug(response);

                //Copy the contents of the new memory stream (which contains the response) to the original stream, which is then returned to the client.
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
        else
        {
            await _next(context);
        }
    }

    private async Task<string> FormatRequest(HttpRequest request)
    {
        //This line allows us to set the reader for the request back at the beginning of its stream.
        request.EnableBuffering();
        var buffer = new byte[Convert.ToInt32(request.ContentLength)];
        await request.Body.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
        var bodyAsText = Encoding.UTF8.GetString(buffer);

        //..and finally, assign the read body back to the request body, which is allowed because of EnableRewind()
        request.Body.Position = 0;

        return $"REQUEST: {request.Scheme} {request.Host}{request.Path} {request.QueryString} {bodyAsText}";
    }

    private async Task<string> FormatResponse(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        string text = await new StreamReader(response.Body).ReadToEndAsync();

        //We need to reset the reader for the response so that the client can read it.
        response.Body.Seek(0, SeekOrigin.Begin);

        return $"RESPONSE: {response.StatusCode}: {text}";
    }
}