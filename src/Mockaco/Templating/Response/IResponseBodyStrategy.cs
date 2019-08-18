using System.Threading.Tasks;

namespace Mockaco
{
    public interface IResponseBodyStrategy
    {
        bool CanHandle(ResponseTemplate responseTemplate);

        Task<byte[]> GetResponseBodyBytesFromTemplate(ResponseTemplate responseTemplate);
    }
}