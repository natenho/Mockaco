namespace Mockaco.Templating.Generating.Providers
{
    internal interface IGeneratedTemplateProviderFactory
    {
        IGeneratedTemplateProvider Create(string providerType);
    }
}