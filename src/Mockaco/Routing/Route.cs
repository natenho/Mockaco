using System.Diagnostics;

namespace Mockaco.Routing
{
    [DebuggerDisplay("{Method} {Path} ({RawTemplate.Name})")]
    public class Route
    {
        public string Method { get; set; }

        public string Path { get; set; }

        public IRawTemplate RawTemplate { get; set; }

        public bool HasCondition { get; set; }

        public Route(string method, string path, IRawTemplate rawTemplate, bool hasCondition)
        {
            Method = method;
            Path = path;
            RawTemplate = rawTemplate;
            HasCondition = hasCondition;
        }

        public override string ToString()
        {
            return $"{Method} {Path} ({RawTemplate.Name})";
        }
    }
}