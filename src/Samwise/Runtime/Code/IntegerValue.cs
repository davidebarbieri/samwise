// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public class IntegerVariableValue : IIntegerValue
    {
        public string Name;
        public string Context;

        public IntegerVariableValue(string name, string context)
        {
            Name = name;
            Context = context != null ? context : "";
        }
        
        public long EvaluateInteger(IDialogueContext context) 
        {
            return context.LookupDataContext(Context)?.GetValueInt(Name) ?? 0;
        }

        public void Traverse(System.Action<IValue> action) { action.Invoke(this); }

        public override string ToString()
        {
            return Context + Name;
        }
    }

    public class IntegerConstantValue : IIntegerValue
    {
        public long Value;

        public IntegerConstantValue(long value)
        {
            Value = value;
        }
        
        public long EvaluateInteger(IDialogueContext dataContext) 
        {
            return Value;
        }
        
        public void Traverse(System.Action<IValue> action) { action.Invoke(this); }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}