// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

using System.Collections.Generic;

namespace Peevo.Samwise
{
    // Sample IDataContext implementation
    public class DataContext : IDataContext
    {
        public string Name {get; internal set; }

        public event System.Action<string, bool, bool> onBoolDataChanged;
        public event System.Action<string, long, long> onIntDataChanged;
        public event System.Action<string, string, string> onSymbolDataChanged;
        public event System.Action<string> onDataClear;
        public event System.Action onClear;
        
        public int BoolVariablesCount => boolVars.Count;
        public int IntVariablesCount => intVars.Count;
        public int SymbolVariablesCount => symbolVars.Count;

        public bool GetValueBool(string name, bool defaultValue = false)
        {
            if (!boolVars.TryGetValue(name, out var value))
                return defaultValue;
            
            return value;
        }

        public long GetValueInt(string name, long defaultValue = 0)
        {
            if (intVars.TryGetValue(name, out var value))
                return value;
            return defaultValue;
        }

        public string GetValueSymbol(string name, string defaultValue = null)
        {
            if (symbolVars.TryGetValue(name, out var value))
                return value;
            return defaultValue;
        }
           
        public void SetValueBool(string name, bool value)
        {
            if (onBoolDataChanged != null)
            {
                boolVars.TryGetValue(name, out var prevValue);
                boolVars[name] = value;
                onBoolDataChanged.Invoke(name, prevValue, value);
            }
            else
                boolVars[name] = value;
        }

        public void SetValueInt(string name, long value)
        {
            if (onIntDataChanged != null)
            {
                intVars.TryGetValue(name, out var prevValue);
                intVars[name] = value;
                onIntDataChanged.Invoke(name, prevValue, value);
            }
            else
                intVars[name] = value;
        }

        public void SetValueSymbol(string name, string value)
        {
            if (onSymbolDataChanged != null)
            {
                symbolVars.TryGetValue(name, out var prevValue);
                symbolVars[name] = value;
                onSymbolDataChanged.Invoke(name, prevValue, value);
            }
            else
                symbolVars[name] = value;
        }

        public void ClearValueBool(string name)
        {
            boolVars.Remove(name);
            onDataClear?.Invoke(name);
        }

        public void ClearValueInt(string name)
        {
            intVars.Remove(name);
            onDataClear?.Invoke(name);
        }

        public void ClearValueSymbol(string name)
        {
            symbolVars.Remove(name);
            onDataClear?.Invoke(name);
        }

        public void Clear()
        {
            boolVars.Clear();
            intVars.Clear();
            symbolVars.Clear();
            onClear?.Invoke();
        }

        public IEnumerable<(string, bool)> GetBoolVariables()
        {
            foreach (var v in boolVars)
            {
                yield return (v.Key, v.Value);
            }
        }

        public IEnumerable<(string, long)> GetIntVariables()
        {
            foreach (var v in intVars)
            {
                yield return (v.Key, v.Value);
            }
        }

        public IEnumerable<(string, string)> GetSymbolVariables()
        {
            foreach (var v in symbolVars)
            {
                yield return (v.Key, v.Value);
            }
        }
        
        Dictionary<string, bool> boolVars = new Dictionary<string, bool>();
        Dictionary<string, long> intVars = new Dictionary<string, long>();
        Dictionary<string, string> symbolVars = new Dictionary<string, string>();
    }
}
