using System.Text.Json;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _log;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> log)
    {
        _next = next;
        _log = log;
    }

    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (InvalidOperationException ex)      // 400 – bad input / business rule
        {
            await Write(ctx, 400, ex.Message);
        }
        catch (KeyNotFoundException ex)           // 404 – nothing found
        {
            await Write(ctx, 404, ex.Message);
        }
        catch (Exception ex)                      // 500 – unhandled
        {
            _log.LogError(ex, "Unhandled exception");
            await Write(ctx, 500, "Unexpected server error.");
        }
    }

    static Task Write(HttpContext ctx, int code, string msg)
    {
        ctx.Response.StatusCode = code;
        ctx.Response.ContentType = "application/json";

        return ctx.Response.WriteAsync(JsonSerializer.Serialize(new { statusCode = code, message = msg }));
    }
}
