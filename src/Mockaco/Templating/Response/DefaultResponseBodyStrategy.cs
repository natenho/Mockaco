namespace Mockaco
{
    public class DefaultResponseBodyStrategy : IResponseBodyStrategy
    {
        public bool CanHandle(ResponseTemplate responseTemplate)
        {
            return true;
        }

        public string GetResponse(ResponseTemplate responseTemplate)
        {
            return responseTemplate.Body?.ToString();
        }
    }
}
