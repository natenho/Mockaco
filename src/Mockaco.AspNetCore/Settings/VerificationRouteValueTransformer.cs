namespace Mockaco.Settings
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Routing;

    internal class VerificationRouteValueTransformer : DynamicRouteValueTransformer
    {
        public override ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            values["controller"] = "verify";
            values["action"] = "Get";

            return ValueTask.FromResult(values);
        }
    }
}