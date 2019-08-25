using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;

namespace Mockaco
{
    public class RouteMatcher
    {
        public RouteValueDictionary Match(string routeTemplate, string requestPath)
        {
            if(string.IsNullOrWhiteSpace(routeTemplate))
            {
                return null;
            }

            var template = TemplateParser.Parse(routeTemplate);

            var matcher = new TemplateMatcher(template, GetDefaults(template));

            var values = new RouteValueDictionary();

            return matcher.TryMatch(requestPath, values) ? values : null;
        }

        public bool IsMatch(string routeTemplate, string requestPath)
        {
            return Match(routeTemplate, requestPath) != null;
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