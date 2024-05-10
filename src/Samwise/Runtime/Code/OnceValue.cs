// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public class OnceValue : IBoolValue, IStatefulElement
    {
        public string StateVariableName { get; set; }
        public string StateVariableContext { get; set; }
        public bool UsesAnonymousVariable { get; set; }

        public OnceValue(string variableName, string context, bool usesAnonymousVariable)
        {
            StateVariableName = variableName;
            StateVariableContext = context;
            UsesAnonymousVariable = usesAnonymousVariable;
        }
        
        public bool EvaluateBool(IDialogueContext context) 
        {
            var c = context.LookupDataContext(StateVariableContext);
            if (c == null)
                return true;

            return !c.GetValueBool(StateVariableName);
        }

        public void OnVisited(IDialogueContext context) 
        {
            context.LookupOrCreateDataContext(StateVariableContext).SetValueBool(StateVariableName, true);
        }

        public void Reset(IDialogueContext context)
        {
            context.LookupOrCreateDataContext(StateVariableContext).SetValueBool(StateVariableName, false);
        }

        public void Traverse(System.Action<IValue> action)
        {
            action.Invoke(this);
        }

        public override string ToString()
        {
            if (UsesAnonymousVariable || string.IsNullOrEmpty(StateVariableName))
                return "once";
            return "once(" + StateVariableContext + StateVariableName + ")" ;
        }
    }
}