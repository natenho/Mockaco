using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.AspNetCore.WebUtilities;

namespace Mockaco
{
    public class RouteMatcher
    {
        public RouteValueDictionary Match(string routeTemplate, string requestPath, IQueryCollection query)
        {
            var regex = new Regex(@"(.*)(\?[^{}]*$)");
            var match = regex.Match(routeTemplate);

            if (match.Success)
            {
                var queryString = match.Groups[2].Value;
                routeTemplate = match.Groups[1].Value;

                var queryInTemplate = QueryHelpers.ParseQuery(queryString);

                foreach (var (key, value) in query)
                {
                    if (!queryInTemplate.ContainsKey(key.TrimStart('?')) || queryInTemplate[key.TrimStart('?')] != value)
                    {
                        return null;
                    }
                }
            }

            var template = TemplateParser.Parse(routeTemplate);
            var matcher = new TemplateMatcher(template, GetDefaults(template));
            var values = new RouteValueDictionary();

            return matcher.TryMatch(requestPath, values) ? values : null;
        }

        public bool IsMatch(string routeTemplate, string requestPath, IQueryCollection query)
        {
            return Match(routeTemplate, requestPath, query) != null;
        }

        private RouteValueDictionary GetDefaults(RouteTemplate parsedTemplate)
        {
            var result = new RouteValueDictionary();

            foreach (var parameter in parsedTemplate.Parameters)
            {
                if (parameter.DefaultValue != null)
                {
                    result.Add(parameter.Name, parameter.DefaultValue);
                }
            }

            return result;
        }
    }
}