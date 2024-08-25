using Microsoft.AspNetCore.Http;

namespace Mockaco.Chaos.Strategies;

internal interface IChaosStrategy
{
    Task Response(HttpResponse httpResponse);
}