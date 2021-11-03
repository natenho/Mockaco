using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Mockaco.Controllers
{
    public class VerifyController : Controller
    {
        private readonly IMemoryCache _cache;

        public VerifyController(IMemoryCache cache)
        {
            _cache = cache;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] string route)
        {
            var requestHappened = _cache.TryGetValue($"{nameof(RequestMatchingMiddleware)} {route}", out var mock);
            if (requestHappened)
            {
                return Ok(mock);
            }

            return NotFound();
        }
    }
}
