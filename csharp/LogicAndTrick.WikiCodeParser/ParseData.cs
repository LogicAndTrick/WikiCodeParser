using System;
using System.Collections.Generic;

namespace LogicAndTrick.WikiCodeParser
{
    public class ParseData
    {
        private readonly Dictionary<string, object> _values;
        public int CurrentElementDepth { get; set; }
        public int CurrentTagDepth { get; set; }

        public ParseData()
        {
            _values = new Dictionary<string, object>();
            CurrentElementDepth = 0;
            CurrentTagDepth = 0;
        }

        public T Get<T>(string key, Func<T> defaultValue)
        {
            if (_values.ContainsKey(key) && _values[key] is T val) return val;
            var v = defaultValue();
            _values[key] = v;
            return v;
        }

        public void Set<T>(string key, T value)
        {
            _values[key] = value;
        }
    }
}
