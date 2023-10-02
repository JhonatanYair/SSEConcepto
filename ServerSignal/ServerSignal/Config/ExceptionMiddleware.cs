using Newtonsoft.Json;

namespace ServerSignal.Config
{

    public class ErrorDetails
    {

        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; internal set; }

        public List<string> Errors { get; set; }

    }

    public class CustomException : Exception
    {
        public int StatusCode { get; set; }

        public CustomException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }

    public class ExceptionMiddleware
    {

        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ExceptionMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<ExceptionMiddleware>();
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            httpContext.Request.EnableBuffering();
            var path = httpContext.Request.Path;
            var headers = httpContext.Request.Headers;
            var headersString = string.Join("; ", headers.Select(header => $"{header.Key}: {string.Join(", ", header.Value)}"));
            string requestBody;

            using (var reader = new StreamReader(httpContext.Request.Body, leaveOpen: true))
            {
                requestBody = await reader.ReadToEndAsync();
                httpContext.Request.Body.Position = 0;
            }

            _logger.LogInformation($"Ruta: {path}");
            _logger.LogInformation($"JSON: {requestBody}");

            _logger.LogInformation($"Headers: {headersString}");

            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Oooops! Algo salió mal: {ex.Message}");
                await HandleGlobalExceptionAsync(httpContext, ex);
            }
        }

        private static Task HandleGlobalExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = GetStatusCode(exception);
            return context.Response.WriteAsync(JsonConvert.SerializeObject(new ErrorDetails()
            {
                StatusCode = context.Response.StatusCode,
                Message = $"Algo salió mal. Error!: {exception.Message}",
                StackTrace = exception.StackTrace
            }));
        }

        private static int GetStatusCode(Exception exception)
        {
            if (exception is CustomException customException)
            {
                return customException.StatusCode;
            }
            else if (exception is HttpRequestException)
            {
                return StatusCodes.Status503ServiceUnavailable;
            }
            else
            {
                return StatusCodes.Status500InternalServerError;
            }
        }

    }

}
