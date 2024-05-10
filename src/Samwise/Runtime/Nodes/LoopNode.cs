// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public class LoopNode : StatefulSelectionNode
    {
        public LoopNode(int sourceLine, string context, string counterVariable, bool usesAnonymousVariable) : base(sourceLine, context, counterVariable, usesAnonymousVariable)
        {
        }

        public override IDialogueNode Next(IDialogueSet dialogues, IDialogueContext context)
        {
            var dataContext = context.LookupOrCreateDataContext(StateVariableContext);
            int id = (int)dataContext.GetValueInt(StateVariableName);

            for (int i = 0; i < ChildrenCount; ++i)
            {
                var ccase = GetChild((id + i) % ChildrenCount);

                if (ccase.Condition != null && !ccase.Condition.EvaluateBool(context))
                    continue;

                ccase.Condition?.OnVisited(context);
                
                // Set next value (loop)
                dataContext.SetValueInt(StateVariableName, (long)((id + i + 1) % ChildrenCount));

                if (ccase.ChildrenCount > 0)
                {
                    return ccase.GetChild(0);
                }  
                return this.FindNextSibling();  
            }

            return this.FindNextSibling();
        }

        public override string PrintPayload()
        {
            return ">> " + StateVariableContext + StateVariableName;
        }
    }
}