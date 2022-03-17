using System.Threading.Tasks;

namespace Mockaco
{
    internal interface IResponseBodyStrategy
    {
        bool CanHandle(ResponseTemplate responseTemplate);

        Task<byte[]> GetResponseBodyBytesFromTemplate(ResponseTemplate responseTemplate);
    }
}