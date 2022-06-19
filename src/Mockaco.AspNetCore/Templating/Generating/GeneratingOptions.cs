using System;
using System.IO;

namespace Mockaco.Templating.Generating
{
    internal class GeneratingOptions
    {
        public string Source { get; set; }

        public Uri SourceUri
        {
            get
            {
                if (Uri.TryCreate(Source, UriKind.Absolute, out Uri uri))
                {
                    return uri;
                }
                else if (Path.IsPathFullyQualified(Source))
                {
                    return new Uri(new Uri(Uri.UriSchemeFile), Source);
                }
                else
                {
                    throw new ArgumentException("Provided Source cannot be converted to Uri");
                }
            }
        }

        public string Provider { get; set; }
    }
}