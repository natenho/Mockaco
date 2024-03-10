using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Mockaco.Tests.Templating.Scripting
{
    public class ScriptRunnerFactoryTest
    {
        [Theory]
        [InlineData(@"JsonConvert.SerializeObject(new DateTime(2012, 04, 23, 18, 25, 43, 511, DateTimeKind.Utc))", @"\""2012-04-23T18:25:43\.511Z\""")]
        [InlineData(@"new PhoneNumbers().BrazilianPhoneNumber()", @"[0-9]+")]
        [InlineData(@"new Faker().Random.Guid().ToString()", @"[a-z0-9\-]+")]
        [InlineData(@"new Regex(@"".*"").IsMatch(""abc"").ToString()", @"True")]
        [InlineData(@"new[] {1, 2, 3, 4, 5, 6, 7}.Count().ToString()", @"7")]
        public async Task Can_Run_Scripts_From_Builtin_Namespaces(string input, string regexPattern)
        {
            var mockLogger = Moq.Mock.Of<ILogger<ScriptRunnerFactory>>();
            var mockOptions = Moq.Mock.Of<IOptionsMonitor<MockacoOptions>>(o => o.CurrentValue == new MockacoOptions());

            var runner = new ScriptRunnerFactory(mockLogger, mockOptions);

            var result = await runner.Invoke<string, string>("", input);

            Assert.Matches(regexPattern, result);
        }
    }
}
