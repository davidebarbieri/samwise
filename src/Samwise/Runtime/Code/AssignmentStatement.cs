// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public class BoolAssignmentStatement : IStatement
    {
        public string Context = "";
        public string Name = "";
        public IBoolValue Value;

        public void Execute(IDialogueContext context)
        {
            context.LookupOrCreateDataContext(Context).SetValueBool(Name, Value.EvaluateBool(context));
        }

        public override string ToString()
        {
            return Context + Name + " = " + Value.ToString();
        }
    }

    public class IntegerAssignmentStatement : IStatement
    {
        public string Context = "";
        public string Name = "";
        public IIntegerValue Value;

        public virtual void Execute(IDialogueContext context)
        {
            context.LookupOrCreateDataContext(Context).SetValueInt(Name, Value.EvaluateInteger(context));
        }

        public override string ToString()
        {
            return Context + Name + " = " + Value.ToString();
        }
    }

    public class SymbolAssignmentStatement : IStatement
    {
        public string Context = "";
        public string Name = "";
        public ISymbolValue Value;

        public void Execute(IDialogueContext context)
        {
            context.LookupOrCreateDataContext(Context).SetValueSymbol(Name, Value.EvaluateSymbol(context));
        }

        public override string ToString()
        {
            return Context + Name + " = " + Value.ToString();
        }
    }
}