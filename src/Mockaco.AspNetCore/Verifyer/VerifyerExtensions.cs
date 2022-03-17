using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Mockaco.Verifyer
{
    internal static class VerifyerExtensions
    {
        public static IResult Verify([FromQuery] string route, [FromServices] IMemoryCache cache)
        {
            var requestHappened = cache.TryGetValue($"{nameof(RequestMatchingMiddleware)} {route}", out var mock);
            if (requestHappened)
            {
                return Results.Ok(mock);
            }

            return Results.NotFound();
        }
    }
}
