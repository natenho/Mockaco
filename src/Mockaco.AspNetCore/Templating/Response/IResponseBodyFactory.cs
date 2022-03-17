using System.Threading.Tasks;

namespace Mockaco
{
    internal interface IResponseBodyFactory
    {
        Task<byte[]> GetResponseBodyBytesFromTemplate(ResponseTemplate responseTemplate);
    }
}
