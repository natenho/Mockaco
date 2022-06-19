namespace Mockaco.Templating.Generating.Providers
{
    public interface IGeneratedTemplateProviderFactory
    {
        IGeneratedTemplateProvider Create(string providerType);
    }
}