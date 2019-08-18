using System.Collections.Generic;
using System.Linq;

namespace Mockaco
{
    public class ResponseBodyFactory : IResponseBodyFactory
    {
        private readonly IEnumerable<IResponseBodyStrategy> _strategies;

        public ResponseBodyFactory(IEnumerable<IResponseBodyStrategy> strategies)
        {
            _strategies = strategies;
        }

        public string GetResponseBodyFromTemplate(ResponseTemplate responseTemplate)
        {
            var selectedStrategy = _strategies.FirstOrDefault(_ => _.CanHandle(responseTemplate));

            return selectedStrategy.GetResponseBodyFromTemplate(responseTemplate);
        }
    }
}
