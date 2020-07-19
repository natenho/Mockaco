namespace Mockaco
{
    public class DefaultResponseBodyStrategy : StringResponseBodyStrategy
    {
        public override bool CanHandle(ResponseTemplate responseTemplate)
        {
            return true;
        }

        public override string GetResponseBodyStringFromTemplate(ResponseTemplate responseTemplate)
        {
            return responseTemplate.Body?.ToString();
        }
    }
}
