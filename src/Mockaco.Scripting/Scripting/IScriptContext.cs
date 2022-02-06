using Bogus;

namespace Mockaco
{
    public interface IScriptContext
    {
        IGlobalVariableStorage Global { get; }

        Faker Faker { get; set; }

        ScriptContextRequest Request { get; set; }

        ScriptContextResponse Response { get; set; }
    }
}