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
            var selectedStrategy = _strategies.FirstOrDefault(_ => _.CanHandle(responseTemplate));

            return selectedStrategy.GetResponseBodyBytesFromTemplate(responseTemplate);
        }
    }
}
