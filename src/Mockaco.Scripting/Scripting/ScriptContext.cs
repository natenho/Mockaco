using Bogus;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mockaco
{
    public class ScriptContext : IScriptContext
    {
        public IGlobalVariableStorage Global { get; }

        public Faker Faker { get; set; }

        public ScriptContextRequest Request { get; set; }

        public ScriptContextResponse Response { get; set; }

        public ScriptContext(IFakerFactory fakerFactory, IGlobalVariableStorage globalVarialeStorage)
        {
            Faker = fakerFactory?.GetDefaultFaker();
            Global = globalVarialeStorage;

            Request = new ScriptContextRequest(
                default,
                new StringDictionary(),
                new StringDictionary(),
                new StringDictionary(),
                new JObject());

            Response = new ScriptContextResponse(new StringDictionary(), new JObject());
        }
    }
}