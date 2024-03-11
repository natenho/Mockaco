using Microsoft.Extensions.Logging;
using Mockaco.Common;
using Mono.TextTemplating;
using Newtonsoft.Json;
using System.Text;

namespace Mockaco
{
    internal class TemplateTransformer : ITemplateTransformer
    {
        private readonly IScriptRunnerFactory _scriptRunnerFactory;
        private readonly ILogger<TemplateTransformer> _logger;

        public TemplateTransformer(IScriptRunnerFactory scriptRunnerFactory, ILogger<TemplateTransformer> logger)
        {
            _scriptRunnerFactory = scriptRunnerFactory;
            _logger = logger;
        }

        public async Task<Template> TransformAndSetVariables(IRawTemplate rawTemplate, IScriptContext scriptContext)
        {
            scriptContext.Global.EnableWriting();

            var transformedTemplate = await Transform(rawTemplate.Content, scriptContext);

            return JsonConvert.DeserializeObject<Template>(transformedTemplate);
        }

        public async Task<Template> Transform(IRawTemplate rawTemplate, IScriptContext scriptContext)
        {
            scriptContext.Global.DisableWriting();

            var transformedTemplate = await Transform(rawTemplate.Content, scriptContext);

            try
            {
                return JsonConvert.DeserializeObject<Template>(transformedTemplate);
            }
            catch (JsonReaderException ex)
            {
                var jsonEx = new InvalidMockException("Generated output is invalid", ex);
                jsonEx.Data.Add("Output", transformedTemplate);
                throw jsonEx;
            }
        }

        private async Task<string> Transform(string input, IScriptContext scriptContext)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            var tokeniser = new Tokeniser("input", input);

            var output = new StringBuilder();

            while (tokeniser.Advance() && tokeniser.State != State.EOF)
            {
                switch (tokeniser.State)
                {
                    case State.Content:
                        output.Append(tokeniser.Value);
                        break;
                    case State.Directive:
                        break;
                    case State.Expression:
                        var expressionResult = await Run(tokeniser.Value, scriptContext);
                        output.Append(expressionResult);
                        break;
                    case State.Block:
                        await Run(tokeniser.Value, scriptContext);
                        break;
                    case State.Helper:
                        break;
                    case State.DirectiveName:
                        break;
                    case State.DirectiveValue:
                        break;
                    case State.Name:
                        break;
                    case State.EOF:
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            return output.ToString();
        }

        private async Task<object> Run(string code, IScriptContext scriptContext)
        {
            try
            {
                var result = await _scriptRunnerFactory.Invoke<IScriptContext, object>(scriptContext, code);

                _logger.LogDebug("Processed script {code} with result {result}", code, result);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Processed script {code} with error", code);
                throw;
            }
        }
    }
}