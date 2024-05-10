// (c) Copyright 2023 Davide 'PeevishDave' Barbieri

using System;

namespace Peevo.Samwise
{
    public class SymbolValue : ISymbolValue, IEquatable<SymbolValue>
    {
        public readonly string Name;

        public SymbolValue(string name) { Name = name; }
        
        public string EvaluateSymbol(IDialogueContext context)
        {
            return Name;
        }
        public void Traverse(System.Action<IValue> action)
        {
            action.Invoke(this);
        }

        public override string ToString()
        {
            return Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public bool Equals(SymbolValue other)
        {
            if (ReferenceEquals(other, this))
                return true;

            if (ReferenceEquals(other, null))
                return false;

            return Name.Equals(other.Name);
        }

        public override bool Equals(object obj)
        {
            var sobj = obj as SymbolValue;
            if (ReferenceEquals(sobj, null))
                return false;

            return Equals(sobj);
        }

        public static bool operator==(SymbolValue a, SymbolValue b)
        {
            if (ReferenceEquals(a, null))
                return ReferenceEquals(b, null);

            return a.Equals(b);
        }

        public static bool operator!=(SymbolValue a, SymbolValue b)
        {
            if (ReferenceEquals(a, null))
                return !ReferenceEquals(b, null);

            return !a.Equals(b);
        }
    }

    public class SymbolVariableValue : ISymbolValue, IBoolValue
    {
        public string Name;
        public string Context;

        public SymbolVariableValue(string name, string context)
        {
            Name = name;
            Context = context != null ? context : "";
        }

        public bool EvaluateBool(IDialogueContext context)
        {
            var c = context.LookupDataContext(Context);
            if (c == null)
                return false;

            return c.GetValueSymbol(Name) != null;
        }

        public string EvaluateSymbol(IDialogueContext context) 
        {
            return context.LookupDataContext(Context)?.GetValueSymbol(Name);
        }

        public void OnVisited(IDialogueContext context)
        {
        }
        public void Traverse(System.Action<IValue> action)
        {
            action.Invoke(this);
        }

        public override string ToString()
        {
            return Context + Name;
        }
    }
}