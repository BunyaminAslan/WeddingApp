using Serilog.Context;
using System.Text.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace WeddingApp.UI.Logging
{
    public class RequestTracingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestTracingMiddleware> _logger;

        public RequestTracingMiddleware(RequestDelegate next, ILogger<RequestTracingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Request.EnableBuffering();

            var traceId = Guid.NewGuid().ToString();
            context.Items["TraceId"] = traceId;
            context.Response.Headers["X-Trace-Id"] = traceId;

            string body = "";
            if (context.Request.ContentLength > 0 && context.Request.Body.CanRead)
            {
                context.Request.Body.Position = 0;
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            var requestInfo = new
            {
                TraceId = traceId,
                Timestamp = DateTime.UtcNow,
                Method = context.Request.Method,
                Path = context.Request.Path,
                Query = context.Request.QueryString.ToString(),
                Headers = context.Request.Headers
                    .Where(h => h.Key != "Authorization")
                    .ToDictionary(h => h.Key, h => h.Value.ToString()),
                Body = TryParseJson(MaskProperties(body))
            };

            var requestJson = JsonSerializer.Serialize(requestInfo, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            _logger.LogInformation("HTTP Request:\n{Request}", requestJson);

            // ✅ RESPONSE BAŞLIĞI BURADA DEĞİŞTİRİLİYOR
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            using (LogContext.PushProperty("TraceId", traceId))
            {
                await _next(context);
            }

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            await responseBody.CopyToAsync(originalBodyStream);

            var responseLog = new
            {
                TraceId = traceId,
                Timestamp = DateTime.UtcNow,
                StatusCode = context.Response.StatusCode,
                Body = TryParseJson(responseText)
            };

            var responseJson = JsonSerializer.Serialize(responseLog, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            _logger.LogInformation("HTTP Response:\n{Response}", responseJson);
        }

        private object TryParseJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonSerializer.Deserialize<JsonElement>(json);
            }
            catch
            {
                return json;
            }
        }

        private string MaskProperties(string maskData)
        {
            if (string.IsNullOrWhiteSpace(maskData))
                return "";

            if(maskData.Length > 300) return "Data Uzun, boyutu :" + maskData.Length.ToString();

            return Regex.Replace(
                maskData,
                "(\"password\"\\s*:\\s*\")(.+?)(\")",
                "$1******$3",
                RegexOptions.IgnoreCase
            );
        }
    }
}
