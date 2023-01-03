using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mockaco.AdminApi
{
    internal class AdminApiExtensions
    {

        internal static IResult Handler([FromQuery] string token,
                                       [FromRoute] string action,
                                       [FromBody] AdminApiRequest model,
                                       [FromServices] IOptionsSnapshot<MockacoOptions> options,
                                       [FromServices] ITemplateProvider templateProvider)
        {
            var _action = action?.ToLower();

            var result = new AdminApiResult()
            {
                Status = true
            };

            try
            {
                if (token == null || !token.Equals(options.Value.AdminApiSecretKey))
                {
                    throw new Exception("Invalid secret key");
                }

                switch (_action)
                {
                    case "list":
                        result.Data = templateProvider.GetTemplates();
                        break;
                    case "remove":
                        templateProvider.Remove(model.Name);
                        break;
                    case "update":
                        templateProvider.Update(model.Name, model.Content);
                        break;
                    default:
                        throw new NotSupportedException($"action {_action} is not supported.");
                }
            }
            catch (Exception ex)
            {
                result = new AdminApiResult()
                {
                    Status = false,
                    Message = ex.Message,
                };
            }


            return Results.Ok(result);
        }

    }
}
