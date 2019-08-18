using System.Threading.Tasks;

namespace Mockaco
{
    public interface IResponseBodyFactory
    {
        Task<byte[]> GetResponseBodyBytesFromTemplate(ResponseTemplate responseTemplate);
    }
}
