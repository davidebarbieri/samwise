// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

using System;

namespace Peevo.Samwise
{
    public class ClampNode : StatefulSelectionNode
    {

        public ClampNode(int sourceLine, string context, string counterVariable, bool usesAnonymousVariable) : base(sourceLine, context, counterVariable, usesAnonymousVariable)
        {
        }

        public override IDialogueNode Next(IDialogueSet dialogues, IDialogueContext context)
        {
            var dataContext = context.LookupOrCreateDataContext(StateVariableContext);

            int id = (int)dataContext.GetValueInt(StateVariableName);

            // Clamp value
            if (id >= ChildrenCount && ChildrenCount > 0)
                id = ChildrenCount - 1;

            for (int i = id; i < ChildrenCount; ++i)
            {
                var ccase = GetChild(i);

                if (ccase.Condition != null && !ccase.Condition.EvaluateBool(context))
                    continue;
                    
                ccase.Condition?.OnVisited(context);

                dataContext.SetValueInt(StateVariableName, Math.Min(id + 1, ChildrenCount - 1));

                if (ccase.ChildrenCount > 0)
                {
                    return ccase.GetChild(0);
                }    
                return this.FindNextSibling();
            }

            dataContext.SetValueInt(StateVariableName, ChildrenCount - 1);
            return this.FindNextSibling();
        }
        
        public override string PrintPayload()
        {
            return ">>> " + StateVariableContext + StateVariableName;
        }
    }
}