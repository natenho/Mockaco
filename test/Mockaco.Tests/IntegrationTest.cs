using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Mockaco.Tests
{
    public class IntegrationTest
    {
        private readonly IHostBuilder _hostBuilder;

        public IntegrationTest()
        {
            //TODO Setup a dedidcated test mock folder
            _hostBuilder = new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder
                        .UseTestServer()
                        .ConfigureServices(services =>
                        {
                            services.AddMockaco();
                        })
                        .Configure(app =>
                        {
                            app.UseMockaco();
                        });
                });
        }

        [Fact]
        public async Task Should_Return_Ok_For_Hello_Mock()
        {
            using var host = await _hostBuilder.StartAsync();

            var guidRegex = @"[{]?[0-9a-f]{8}-([0-9a-f]{4}-){3}[0-9a-f]{12}[}]?";
            var expectedMessage = "Hello test!";

            var response = await host.GetTestClient().GetAsync("/hello/test");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseBody = await response.Content.ReadAsStringAsync();
            var responseJson = JObject.Parse(responseBody);

            responseJson.Count.Should().Be(3);

            responseJson["id"].ToString().Should().MatchRegex(guidRegex);
            responseJson["message"].ToString().Should().Be(expectedMessage);
            responseJson["createdAt"].Type.Should().Be(JTokenType.Date);
        }

        [Fact]
        public async Task Should_Return_Not_Implemented_For_Non_Matching_Mock()
        {
            using var host = await _hostBuilder.StartAsync();

            var response = await host.GetTestClient().GetAsync("/not-existing-mock-route/foo/bar");

            response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);

            var responseBody = await response.Content.ReadAsStringAsync();
            var responseJson = JArray.Parse(responseBody);

            responseJson.Single()["Message"].ToString()
                .Should().Be("Incoming request didn't match any mock");
        }

        [Fact]
        public async Task Should_Return_Ok_For_Succeeded_Verification()
        {
            using var host = await _hostBuilder.StartAsync();

            var givenRoute = "/hello/test";

            var response = await host.GetTestClient().GetAsync(givenRoute);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            response = await host.GetTestClient().GetAsync($"/_mockaco/verification?route={givenRoute}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Should_Return_NotFound_For_Failed_Verification()
        {
            using var host = await _hostBuilder.StartAsync();

            var givenRoute = "/hello/test";

            var response = await host.GetTestClient().GetAsync(givenRoute);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            response = await host.GetTestClient().GetAsync($"/_mockaco/verification?route=/hello/another-route");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
