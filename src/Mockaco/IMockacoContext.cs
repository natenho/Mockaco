using Mockaco.Routing;

namespace Mockaco
{
    public interface IMockacoContext
    {        
        Template TransformedTemplate { get; set; }
        Route Route { get; set; }
    }
}