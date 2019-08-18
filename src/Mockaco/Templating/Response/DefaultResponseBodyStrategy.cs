namespace Mockaco
{
    public class DefaultResponseBodyStrategy : IResponseBodyStrategy
    {
        public bool CanHandle(ResponseTemplate responseTemplate)
        {
            return true;
        }

        public string GetResponseBodyFromTemplate(ResponseTemplate responseTemplate)
        {
            return responseTemplate.Body?.ToString();
        }
    }
}
