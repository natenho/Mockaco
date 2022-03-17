using System.Diagnostics;

namespace Mockaco
{
    [DebuggerDisplay("{Method} {Route} ({RawTemplate.Name})")]
    public class Mock
    {
        public string Method { get; set; }

        public string Route { get; set; }

        public IRawTemplate RawTemplate { get; set; }

        public bool HasCondition { get; set; }

        public Mock(string method, string route, IRawTemplate rawTemplate, bool hasCondition)
        {
            Method = method;
            Route = route;
            RawTemplate = rawTemplate;
            HasCondition = hasCondition;
        }

        public override string ToString()
        {
            return $"{Method} {Route} ({RawTemplate.Name})";
        }
    }
}