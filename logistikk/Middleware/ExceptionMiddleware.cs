

using System.Net;
using System.Text.Json;
namespace logistikk.Middleware
{
    // Fanger opp alle uventede feil i applikasjonen ett sted
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Send forespørselen videre i pipelinen
                await _next(context);
            }
            catch (Exception ex)
            {
                // Logg den fulle feilen på serveren (kun for oss)
                _logger.LogError(ex, "Uventet feil: {Melding}", ex.Message);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // I utvikling: vis detaljer. I produksjon: skjul dem
                var response = _env.IsDevelopment()
                    ? new ErrorResponse(500, ex.Message, ex.StackTrace?.ToString())
                    : new ErrorResponse(500, "Det oppstod en intern feil", null);

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
            }
        }
    }

    // Standard format for feilmeldinger
    public record ErrorResponse(int StatusCode, string Message, string? Details);
}