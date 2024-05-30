using System.Diagnostics;

namespace Gateway;

public class TracingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ActivitySource _activitySource;

    public TracingMiddleware(RequestDelegate next)
    {
        _next = next;
        _activitySource = new ActivitySource("TraceMiddleware");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using var activity = _activitySource.StartActivity("IncomingRequest", ActivityKind.Server);

        activity?.SetTag("http.method", context.Request.Method);
        activity?.SetTag("http.url", context.Request.Path);

        if (context.Request.Body.CanSeek)
        {
            context.Request.Body.Seek(0, SeekOrigin.Begin);
            var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
            context.Request.Body.Seek(0, SeekOrigin.Begin);
            activity?.SetTag("request.body", requestBody);
        }

        await _next(context);
    }
}