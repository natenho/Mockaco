using System;

namespace Mockaco.Generation
{
    public class TemplateGenerationContext
    {
        public TemplateGenerationSettings Settings { get; set; }

        public string SourceType { get; set; }

        public Uri SourceUri { get; set; }
    }
}
