using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace Mockaco
{
    public class RequestBodyFactory : IRequestBodyFactory
    {
        private readonly IEnumerable<IRequestBodyStrategy> _strategies;

        public RequestBodyFactory(IEnumerable<IRequestBodyStrategy> strategies)
        {
            _strategies = strategies;
        }

        public async Task<JToken> ReadBodyAsJson(HttpRequest httpRequest)
        {
            if (httpRequest.Body?.CanRead == false)
            {
                return new JObject();
            }

            var selectedStrategy = _strategies.FirstOrDefault(strategy => strategy.CanHandle(httpRequest));

            if (selectedStrategy == null)
            {
                return new JObject();
            }

            return await selectedStrategy.ReadBodyAsJson(httpRequest);
        }
    }
}
