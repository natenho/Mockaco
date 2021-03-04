using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace Mockaco
{
    public class XmlRequestBodyStrategy : IRequestBodyStrategy
    {
        public bool CanHandle(HttpRequest httpRequest)
        {
            return httpRequest.HasXmlContentType();
        }

        public async Task<JToken> ReadBodyAsJson(HttpRequest httpRequest)
        {
            var body = await httpRequest.ReadBodyStream();

            if (string.IsNullOrWhiteSpace(body))
            {
                return new JObject();
            }

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(body);

            var json = JsonConvert.SerializeXmlNode(xmlDocument);

            return JToken.Parse(json);
        }
    }
}
