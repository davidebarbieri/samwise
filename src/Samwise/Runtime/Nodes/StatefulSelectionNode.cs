// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public abstract class StatefulSelectionNode : SelectionNode, IStatefulElement
    {
        public string StateVariableName { get; set; }
        public string StateVariableContext { get; set; }
        public bool UsesAnonymousVariable { get; set; }

        public StatefulSelectionNode(int sourceLine, string context, string counterVariable, bool usesAnonymousVariable) : base(sourceLine)
        {
            StateVariableContext = context;
            StateVariableName = counterVariable;
            UsesAnonymousVariable = usesAnonymousVariable;
        }
    }
}