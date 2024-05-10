// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public class BoolVariableValue : IBoolValue
    {
        public string Name;
        public string Context;

        public BoolVariableValue(string name, string context)
        {
            Name = name;
            Context = context != null ? context : "";
        }
        
        public bool EvaluateBool(IDialogueContext context) 
        {
            return context.LookupDataContext(Context)?.GetValueBool(Name) ?? false;
        }

        public void OnVisited(IDialogueContext context) {}

        public void Traverse(System.Action<IValue> action) { action.Invoke(this); }

        public override string ToString()
        {
            return Context + Name;
        }
    }

    public class BoolConstantValue : IBoolValue
    {
        public bool Value;

        public BoolConstantValue(bool value)
        {
            Value = value;
        }
        
        public bool EvaluateBool(IDialogueContext context) 
        {
            return Value;
        }
        
        public void OnVisited(IDialogueContext context) {}
        public void Traverse(System.Action<IValue> action) { action.Invoke(this); }

        public override string ToString()
        {
            return Value ? "true" : "false";
        }
    }
}