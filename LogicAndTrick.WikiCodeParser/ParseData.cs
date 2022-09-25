using System;
using System.Collections.Generic;

namespace LogicAndTrick.WikiCodeParser
{
    public class ParseData
    {
        private readonly Dictionary<string, object> _values;

        public ParseData()
        {
            _values = new Dictionary<string, object>();
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
