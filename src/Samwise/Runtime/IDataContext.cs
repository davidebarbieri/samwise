// (c) Copyright 2022 Davide 'PeevishDave' Barbieri
using System.Collections.Generic;

namespace Peevo.Samwise
{
    public interface IDataContext
    {
        string Name { get; }

        event System.Action<string, bool, bool> onBoolDataChanged;
        event System.Action<string, long, long> onIntDataChanged;
        event System.Action<string, string, string> onSymbolDataChanged;
        event System.Action<string> onDataClear;
        event System.Action onClear;
        
        int BoolVariablesCount { get; }
        int IntVariablesCount { get; }
        int SymbolVariablesCount { get; }

        bool GetValueBool(string name);
        void SetValueBool(string name, bool value);
        void ClearValueBool(string name);
        long GetValueInt(string name);
        void SetValueInt(string name, long value);
        void ClearValueInt(string name);
        string GetValueSymbol(string name);
        void SetValueSymbol(string name, string value);
        void ClearValueSymbol(string name);

        IEnumerable<(string, bool)> GetBoolVariables();
        IEnumerable<(string, long)> GetIntVariables();
        IEnumerable<(string, string)> GetSymbolVariables();

        void Clear();
    }
}
