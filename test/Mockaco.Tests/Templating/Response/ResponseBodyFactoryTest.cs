using FluentAssertions;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Mockaco.Tests.Templating.Response
{
    public class ResponseBodyFactoryTest
    {
        [Fact]
        public async Task Can_Handle_Null_Returned_From_Response_Body_Strategy()
        {
            var responseBodyStrategy = Moq.Mock.Of<IResponseBodyStrategy>(
                s => s.CanHandle(It.IsAny<ResponseTemplate>()) && s.GetResponseBodyBytesFromTemplate(It.IsAny<ResponseTemplate>()) == Task.FromResult<byte[]>(null));

            var factory = new ResponseBodyFactory(new[] { responseBodyStrategy });

            var responseTemplate = new ResponseTemplate { Body = null };

            var bodyBytes = await factory.GetResponseBodyBytesFromTemplate(responseTemplate);

            bodyBytes.Should()
                .BeNull();
        }
    }
}