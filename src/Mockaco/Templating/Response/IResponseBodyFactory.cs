namespace Mockaco
{
    public interface IResponseBodyStrategyFactory
    {
        string GetResponseBody(ResponseTemplate responseTemplate);        
    }
}
