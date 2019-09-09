using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mockaco
{
    public class ResponseBodyFactory : IResponseBodyFactory
    {
        private readonly IEnumerable<IResponseBodyStrategy> _strategies;

        public ResponseBodyFactory(IEnumerable<IResponseBodyStrategy> strategies)
        {
            _strategies = strategies;
        }

        public Task<byte[]> GetResponseBodyBytesFromTemplate(ResponseTemplate responseTemplate)
        {
            if (responseTemplate == default)
            {
                return Task.FromResult<byte[]>(default);
            }

            var selectedStrategy = _strategies.FirstOrDefault(_ => _.CanHandle(responseTemplate));

            return selectedStrategy != null ? selectedStrategy.GetResponseBodyBytesFromTemplate(responseTemplate) : Task.FromResult<byte[]>(default);
        }
    }
}