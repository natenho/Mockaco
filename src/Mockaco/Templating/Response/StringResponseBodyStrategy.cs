using System.Text;
using System.Threading.Tasks;

namespace Mockaco
{
    public abstract class StringResponseBodyStrategy : IResponseBodyStrategy
    {
        public abstract bool CanHandle(ResponseTemplate responseTemplate);

        public Task<byte[]> GetResponseBodyBytesFromTemplate(ResponseTemplate responseTemplate)
        {
            var responseBodyStringFromTemplate = GetResponseBodyStringFromTemplate(responseTemplate);

            return Task.FromResult(responseBodyStringFromTemplate == default ? default : Encoding.UTF8.GetBytes(responseBodyStringFromTemplate));
        }

        public abstract string GetResponseBodyStringFromTemplate(ResponseTemplate responseTemplate);
    }
}
