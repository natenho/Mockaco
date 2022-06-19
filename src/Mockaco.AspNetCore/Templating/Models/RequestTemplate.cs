namespace Mockaco
{
    internal class RequestTemplate
    {
        public string Method { get; set; }

        public string Route { get; set; }

        public bool? Condition { get; set; }
    }
}