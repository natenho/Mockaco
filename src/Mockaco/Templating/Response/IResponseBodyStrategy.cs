namespace Mockaco
{
    public interface IResponseBodyStrategy
    {
        bool CanHandle(ResponseTemplate responseTemplate);

        string GetResponse(ResponseTemplate responseTemplate);
    }
}