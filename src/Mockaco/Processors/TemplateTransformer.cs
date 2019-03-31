using Microsoft.Extensions.Logging;
using Mono.TextTemplating;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Mockaco.Processors
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

        public async Task<string> Transform(string input, ScriptContext scriptContext)
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

        // TODO Remove repeated code
        private async Task<object> Run(string code, ScriptContext scriptContext)
        {
            object result = null;

            try
            {
                result = await _scriptRunnerFactory.Invoke<ScriptContext, object>(scriptContext, code);

                _logger.LogDebug($"Processed script {code} with result {result}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Processed script {code} with result {ex}");
            }

            return result;
        }
    }
}