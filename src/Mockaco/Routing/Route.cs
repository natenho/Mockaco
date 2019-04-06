namespace Mockaco.Routing
{
    public class Route
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public IRawTemplate RawTemplate { get; set; }

        public Route(string method, string path, IRawTemplate rawTemplate)
        {
            Method = method;
            Path = path;
            RawTemplate = rawTemplate;
        }
    }
}
