using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Mockaco.Controllers
{
    [Route("/verify")]
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
            var requestHappened = _cache.TryGetValue(route, out var mock);
            if (requestHappened)
            {
                return Ok(mock);
            }

            return NotFound();
        }
    }
}
