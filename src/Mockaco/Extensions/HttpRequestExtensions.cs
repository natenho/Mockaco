/*
The MIT License (MIT)

Copyright (c) 2015 Microsoft

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using Microsoft.Net.Http.Headers;
using Mockaco;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http
{
    /// <summary>
    /// Set of extension methods for Microsoft.AspNetCore.Http.HttpRequest
    /// </summary>
    public static class HttpRequestExtensions
    {
        private const string UnknownHostName = "UNKNOWN-HOST";

        /// <summary>
        /// Gets http request Uri from request object
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/></param>
        /// <returns>A New Uri object representing request Uri</returns>
        public static Uri GetUri(this HttpRequest request)
        {
            if (null == request)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.Scheme))
            {
                throw new ArgumentException("Http request Scheme is not specified");
            }

            string hostName = request.Host.HasValue ? request.Host.ToString() : UnknownHostName;

            var builder = new StringBuilder();

            builder.Append(request.Scheme)
                .Append("://")
                .Append(hostName);

            if (request.Path.HasValue)
            {
                builder.Append(request.Path.Value);
            }

            if (request.QueryString.HasValue)
            {
                builder.Append(request.QueryString);
            }

            return new Uri(builder.ToString());
        }

        public static Routing.RouteValueDictionary GetRouteData(this HttpRequest request, Mock mock)
        {
            var routeMatcher = new RouteMatcher();

            return routeMatcher.Match(mock.Route, request.Path);
        }

        public static bool HasXmlContentType(this HttpRequest request)
        {
            MediaTypeHeaderValue.TryParse(request.ContentType, out var parsedValue);

            return parsedValue?.MediaType.Equals(HttpContentTypes.ApplicationXml, StringComparison.OrdinalIgnoreCase) == true
                || parsedValue?.MediaType.Equals(HttpContentTypes.TextXml, StringComparison.OrdinalIgnoreCase) == true;
        }

        public static async Task<string> ReadBodyStream(this HttpRequest httpRequest)
        {
            httpRequest.EnableBuffering();

            var encoding = GetEncodingFromContentType(httpRequest.ContentType) ?? Encoding.UTF8;
            var reader = new StreamReader(httpRequest.Body, encoding); 

            var body = await reader.ReadToEndAsync();

            if (httpRequest.Body.CanSeek)
            {
                httpRequest.Body.Seek(0, SeekOrigin.Begin);
            }

            return body;
        }

        public static IEnumerable<string> GetAcceptLanguageValues(this HttpRequest httpRequest)
        {
            var acceptLanguages = httpRequest.GetTypedHeaders().AcceptLanguage;

            if(acceptLanguages == default)
            {
                return Enumerable.Empty<string>();
            }

            return acceptLanguages?.Select(l => l.Value.ToString());
        }

        private static Encoding GetEncodingFromContentType(string contentType)
        {
            // although the value is well parsed, the encoding is null when it is not informed
            if (MediaTypeHeaderValue.TryParse(contentType, out var parsedValue))
                return parsedValue.Encoding;

            return null;
        }
    }
}