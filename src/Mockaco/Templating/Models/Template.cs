using System.Collections.Generic;
using System.Linq;

namespace Mockaco
{
    public class Template
    {
        public Template()
        {
            Callbacks = Enumerable.Empty<CallbackTemplate>();
        }

        public RequestTemplate Request { get; set; }

        public ResponseTemplate Response { get; set; }

        public CallbackTemplate Callback
        {
            get => Callbacks.Count() == 1 ? Callbacks.First() : default;
            set => Callbacks = new[] { value };
        }

        public IEnumerable<CallbackTemplate> Callbacks { get; set; }
    }
}