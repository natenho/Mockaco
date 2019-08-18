using System.Text;
using System.Threading.Tasks;

namespace Mockaco
{
    public abstract class StringResponseBodyStrategy : IResponseBodyStrategy
    {
        public abstract bool CanHandle(ResponseTemplate responseTemplate);

        public Task<byte[]> GetResponseBodyBytesFromTemplate(ResponseTemplate responseTemplate)
        {
            return Task.FromResult(Encoding.UTF8.GetBytes(GetResponseBodyStringFromTemplate(responseTemplate)));
        }

        public abstract string GetResponseBodyStringFromTemplate(ResponseTemplate responseTemplate);
    }
}
