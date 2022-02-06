using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mockaco
{
    public interface IMockProvider
    {
        IEnumerable<Mock> GetMocks();

        IEnumerable<(string TemplateName, string ErrorMessage)> GetErrors();

        Task BuildCache();
    }
}