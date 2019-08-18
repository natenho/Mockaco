namespace Mockaco
{
    public interface IResponseBodyStrategy
    {
        bool CanHandle(ResponseTemplate responseTemplate);

        string GetResponseBodyFromTemplate(ResponseTemplate responseTemplate);
    }
}