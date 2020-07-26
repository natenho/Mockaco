using FluentAssertions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Mockaco.Tests.Templating
{
    public class TemplateTransformerTest
    {
        [Theory]
        [TextFileData("Templating/Transforms_Plain_Json_Template.json")]
        public async Task Transforms_Plain_Json_Template(string content)
        {
            var templateTransformer = new TemplateTransformer(Moq.Mock.Of<IScriptRunnerFactory>(), Moq.Mock.Of<ILogger<TemplateTransformer>>());

            var rawTemplate = Moq.Mock.Of<IRawTemplate>(t => t.Content == content);
            var scriptContext = Moq.Mock.Of<IScriptContext>();
            Moq.Mock.Get(scriptContext).Setup(m => m.Global).Returns(Moq.Mock.Of<IGlobalVariableStorage>());
            
            var transformedTemplate = await templateTransformer.TransformAndSetVariables(rawTemplate, scriptContext);

            Assert(transformedTemplate);
        }

        [Theory]
        [TextFileData("Templating/Transforms_Scripted_Json_Template.json")]
        public async Task Transforms_Scripted_Json_Template(string content)
        {
            var rawTemplate = Moq.Mock.Of<IRawTemplate>(t => t.Content == content);
            var scriptContext = Moq.Mock.Of<IScriptContext>();
            Moq.Mock.Get(scriptContext).Setup(m => m.Global).Returns(Moq.Mock.Of<IGlobalVariableStorage>());

            var scriptRunnerFactory = Moq.Mock.Of<IScriptRunnerFactory>(f =>
            f.Invoke<IScriptContext, object>(scriptContext, "Request.Route[\"parameter1\"] == \"firstParameter\"") == Task.FromResult((object)true)
            && f.Invoke<IScriptContext, object>(scriptContext, "Faker.Random.Number(1,7)") == Task.FromResult((object)5)
            && f.Invoke<IScriptContext, object>(scriptContext, "JsonConvert.SerializeObject(new DateTime(2012, 04, 23, 18, 25, 43, 511, DateTimeKind.Utc))") == Task.FromResult((object)"\"2012-04-23T18:25:43.511Z\""));

            var templateTransformer = new TemplateTransformer(scriptRunnerFactory, Moq.Mock.Of<ILogger<TemplateTransformer>>());

            var transformedTemplate = await templateTransformer.TransformAndSetVariables(rawTemplate, scriptContext);

            Assert(transformedTemplate);
        }

        private static void Assert(Template transformedTemplate)
        {
            transformedTemplate.Request.Method.Should()
                .Be("POST");

            transformedTemplate.Request.Route.Should()
                .Be("this/is/the/{parameter1}/route/{parameter2}");

            transformedTemplate.Request.Condition.Should()
                .BeTrue();

            transformedTemplate.Response.Delay.Should()
                .HaveValue("Response.Delay is set");

            transformedTemplate.Response.Indented.Should()
                .HaveValue("Response.Indented is set");

            transformedTemplate.Response.Status.Should()
                .Be(HttpStatusCode.Created);

            transformedTemplate.Response.Headers.Should()
                .HaveCount(2);

            transformedTemplate.Response.Body["id"].ToString().Should()
                .Be("1");

            transformedTemplate.Response.Body["message"].ToString().Should()
                .Be("Hello world");

            transformedTemplate.Response.Body["createdAt"].Type.Should()
               .Be(JTokenType.Date);

            transformedTemplate.Response.Body["createdAt"].Value<DateTime>().Should()
               .Be(new DateTime(2012, 04, 23, 18, 25, 43, 511, DateTimeKind.Utc));

            transformedTemplate.Callback.Method.Should()
                .Be("PUT");

            transformedTemplate.Callback.Url.Should()
                .Be("http://callback-address/route/to/call");

            transformedTemplate.Callback.Delay.Should()
                .HaveValue("Callback.Delay is set");

            transformedTemplate.Callback.Timeout.Should()
                .HaveValue("Callback.Timeout is set");

            transformedTemplate.Callback.Indented.Should()
                .HaveValue("Callback.Indented is set");

            transformedTemplate.Callback.Headers.Should()
                .HaveCount(2);

            transformedTemplate.Callback.Body["key"].ToString().Should()
               .Be("2");

            transformedTemplate.Callback.Body["key"].Type.Should()
               .Be(JTokenType.Integer);

            transformedTemplate.Callback.Body["topic"].ToString().Should()
                .Be("Hello callback");

            transformedTemplate.Callback.Body["updatedAt"].Type.Should()
               .Be(JTokenType.Date);

            transformedTemplate.Callback.Body["updatedAt"].Value<DateTime>().Should()
               .Be(new DateTime(2003, 02, 01, 19, 00, 00, DateTimeKind.Utc));
        }

    }
}
