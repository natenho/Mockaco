using Microsoft.Extensions.Logging;
using Mono.TextTemplating;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Mockaco
{
    public class TemplateTransformer : ITemplateTransformer
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

            return JsonConvert.DeserializeObject<Template>(transformedTemplate);           
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

                        object expressionResult;

                        try
                        {
                            expressionResult = await Run(tokeniser.Value, scriptContext);
                        }
                        catch (Exception)
                        {
                            expressionResult = string.Empty;
                        }

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
            object result = null;

            try
            {
                result = await _scriptRunnerFactory.Invoke<IScriptContext, object>(scriptContext, code);

                _logger.LogDebug("Processed script {code} with result {result}", code, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Processed script {code} with error", code);
            }

            return result;
        }

    }
}