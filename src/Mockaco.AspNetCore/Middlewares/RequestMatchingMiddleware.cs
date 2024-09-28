using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Mockaco
{
    internal class RequestMatchingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestMatchingMiddleware> _logger;

        public RequestMatchingMiddleware(RequestDelegate next, ILogger<RequestMatchingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(
            HttpContext httpContext,
            IMockacoContext mockacoContext,
            IScriptContext scriptContext,
            IMockProvider mockProvider,
            ITemplateTransformer templateTransformer,
            IEnumerable<IRequestMatcher> requestMatchers,
            IMemoryCache cache,
            IOptions<MockacoOptions> options
            )
        {
            await LogHttpContext(httpContext);

            await AttachRequestToScriptContext(httpContext, mockacoContext, scriptContext);

            if (mockacoContext.Errors.Any())
            {
                return;
            }

            Mock bestMatch = null;
            var bestScore = -1;

            foreach (var mock in mockProvider.GetMocks())
            {
                if (await requestMatchers.AllAsync(e => e.IsMatch(httpContext.Request, mock)))
                {
                    var score = ScoreRouteTemplate(mock.Route);

                    if (score > bestScore)
                    {
                        bestMatch = mock;
                        bestScore = score;
                    }
                }
                else
                {
                    _logger.LogDebug("Incoming request didn't match {mock}", mock);
                }
            }

            if (bestMatch != null)
            {
                cache.Set($"{nameof(RequestMatchingMiddleware)} {httpContext.Request.Path.Value}", new
                {
                    Route = httpContext.Request.Path.Value,
                    Timestamp = $"{DateTime.Now:t}",
                    Headers = LoadHeaders(httpContext, options.Value.VerificationIgnoredHeaders),
                    Body = await httpContext.Request.ReadBodyStream()
                }, DateTime.Now.AddMinutes(options.Value.MatchedRoutesCacheDuration));

                _logger.LogInformation("Incoming request matched {mock}", bestMatch);

                await scriptContext.AttachRouteParameters(httpContext.Request, bestMatch);

                var template = await templateTransformer.TransformAndSetVariables(bestMatch.RawTemplate, scriptContext);

                mockacoContext.Mock = bestMatch;
                mockacoContext.TransformedTemplate = template;

                await _next(httpContext);

                return;
            }

            _logger.LogInformation("Incoming request didn't match any mock");

            mockacoContext.Errors.Add(new Error("Incoming request didn't match any mock"));
        }

        internal static int ScoreRouteTemplate(string route)
        {
            // Split the route into segments
            var segments = route.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

            var score = 0;
            foreach (var segment in segments)
            {
                if (segment.StartsWith("{") && segment.EndsWith("}"))
                {
                    //Wildcards get the lowest score
                    score++;
                }
                else
                {
                    //Give more weight to static segments
                    score += 2;
                }

                //Give more weight to segments with multiple parameters
                score *= 2;
            }

            //Give more weight to longer routes
            return score * segments.Length;
        }

        //TODO Remove redundant code
        private async Task AttachRequestToScriptContext(HttpContext httpContext, IMockacoContext mockacoContext, IScriptContext scriptContext)
        {
            try
            {
                await scriptContext.AttachRequest(httpContext.Request);
            }
            catch (Exception ex)
            {
                mockacoContext.Errors.Add(new Error("An error occurred while reading request", ex));

                _logger.LogWarning(ex, "An error occurred while reading request");

                return;
            }
        }

        private async Task LogHttpContext(HttpContext httpContext)
        {
            _logger.LogInformation("Incoming request from {remoteIp}", httpContext.Connection.RemoteIpAddress);

            _logger.LogDebug("Headers: {headers}", httpContext.Request.Headers.ToJson());

            var body = await httpContext.Request.ReadBodyStream();

            if (string.IsNullOrEmpty(body))
            {
                _logger.LogDebug("Body is not present");
            }
            else
            {
                _logger.LogDebug("Body: {body}", body);
            }
        }

        private static IEnumerable<object> LoadHeaders(HttpContext httpContext, IEnumerable<string> verificationIgnoredHeaders)
        {            
            return from header in httpContext.Request.Headers.ToList()
                   where !verificationIgnoredHeaders.Any(opt => opt == header.Key)
                   select new { header.Key, Value = header.Value[0] };
        }
    }
}