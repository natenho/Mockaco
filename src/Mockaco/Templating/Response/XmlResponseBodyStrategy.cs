using System;
using System.Text;
using System.Xml;

namespace Mockaco
{
    public class XmlResponseBodyStrategy : StringResponseBodyStrategy
    {
        public override bool CanHandle(ResponseTemplate responseTemplate)
        {
            responseTemplate.Headers.TryGetValue(HttpHeaders.ContentType, out var contentType);

            return contentType.IsAnyOf(HttpContentTypes.ApplicationXml, HttpContentTypes.TextXml);
        }

        public override string GetResponseBodyStringFromTemplate(ResponseTemplate responseTemplate)
        {
            var settings = new XmlWriterSettings
            {
                Indent = responseTemplate.Indented.GetValueOrDefault(true)
            };

            var stringBuilder = new StringBuilder();
            using (var writer = XmlWriter.Create(stringBuilder, settings))
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(responseTemplate.Body?.ToString());
                xmlDocument.WriteContentTo(writer);
            }

            return stringBuilder.ToString();
        }
    }
}
