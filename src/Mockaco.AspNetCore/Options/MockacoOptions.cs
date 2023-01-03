﻿using System.Net;

namespace Mockaco
{
    public class MockacoOptions
    {
        public HttpStatusCode DefaultHttpStatusCode { get; set; }

        public HttpStatusCode ErrorHttpStatusCode { get; set; }

        public string DefaultHttpContentType { get; set; }

        public List<string> References { get; set; }

        public List<string> Imports { get; set; }

        public int MatchedRoutesCacheDuration { get; set; }

        public string VerificationEndpointPrefix { get; set; }

        public string VerificationEndpointName { get; set; }

        public string AdminApiEndpointPrefix { get; set; }

        public string AdminApiEndpointName { get; set; }
        public string AdminApiSecretKey { get; set; }


        public TemplateFileProviderOptions TemplateFileProvider { get; set; }

        public MockacoOptions()
        {
            DefaultHttpStatusCode = HttpStatusCode.OK;
            ErrorHttpStatusCode = HttpStatusCode.NotImplemented;
            DefaultHttpContentType = HttpContentTypes.ApplicationJson;
            References = new List<string>();
            Imports = new List<string>();
            MatchedRoutesCacheDuration = 60;
            VerificationEndpointPrefix = "_mockaco";
            VerificationEndpointName = "verification";
            TemplateFileProvider = new();
            AdminApiEndpointPrefix = "_mockaco";
            AdminApiEndpointName = "api";
        }
    }
}
