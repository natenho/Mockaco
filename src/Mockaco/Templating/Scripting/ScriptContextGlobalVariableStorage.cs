using System.Collections.Concurrent;

namespace Mockaco
{
    public class ScriptContextGlobalVariableStorage : IGlobalVariableStorage
    {
        private static readonly ConcurrentDictionary<string, object> _variables = new ConcurrentDictionary<string, object>();

        private bool _canWrite;

        private readonly object _locker = new object();

        public object this[string name]
        {
            get
            {
                _variables.TryGetValue(name, out var value);

                return value;
            }
            set
            {
                if (_canWrite)
                {
                    _variables.AddOrUpdate(name, value, (name, _) => _variables[name] = value);
                }
            }
        }

        public void Clear()
        {
            _variables.Clear();
        }

        public void DisableWriting()
        {
            lock (_locker)
            {
                _canWrite = false;
            }
        }

        public void EnableWriting()
        {
            lock (_locker)
            {
                _canWrite = true;
            }
        }
    }
}
