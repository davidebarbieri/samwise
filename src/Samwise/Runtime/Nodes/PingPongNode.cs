// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public class PingPongNode : StatefulSelectionNode
    {    
        public PingPongNode(int sourceLine, string context, string counterVariable, bool usesAnonymousVariable) : base(sourceLine, context, counterVariable, usesAnonymousVariable)
        {
        }

        public override IDialogueNode Next(IDialogueSet dialogues, IDialogueContext context)
        {
            var dataContext = context.LookupOrCreateDataContext(StateVariableContext);
            int id = (int)dataContext.GetValueInt(StateVariableName);

            if (id >= 0)
            {
                if (id < 0)
                    id = 0; 
                else if (id > ChildrenCount - 1)
                    id = ChildrenCount - 1; 

                for (int i = id; i < ChildrenCount; ++i)
                {
                    var ccase = GetChild(i);

                    if (ccase.Condition != null && !ccase.Condition.EvaluateBool(context))
                        continue;

                    ccase.Condition?.OnVisited(context);
                    
                    if (i == ChildrenCount - 1)
                        dataContext.SetValueInt(StateVariableName, (long)(2-ChildrenCount));
                    else
                        dataContext.SetValueInt(StateVariableName, (long)(i + 1));
                        
                    if (ccase.ChildrenCount > 0)
                    {
                        return ccase.GetChild(0);
                    }    
                    return this.FindNextSibling();
                }

                for (int i = id - 1; i >= 0; --i)
                {
                    var ccase = GetChild(i);

                    if (ccase.Condition != null && !ccase.Condition.EvaluateBool(context))
                        continue;

                    ccase.Condition?.OnVisited(context);
                    
                    if (i == 0)
                        dataContext.SetValueInt(StateVariableName, 1);
                    else
                        dataContext.SetValueInt(StateVariableName, (long)(i - 1));
                    
                    if (ccase.ChildrenCount > 0)
                    {
                        return ccase.GetChild(0);
                    }    
                    return this.FindNextSibling();
                }
            }
            else
            {
                if (id < -ChildrenCount + 1)
                    id = -ChildrenCount + 1; 
                else if (id > 0)
                    id = 0; 

                for (int i = -id; i >= 0; --i)
                {
                    var ccase = GetChild(i);

                    if (ccase.Condition != null && !ccase.Condition.EvaluateBool(context))
                        continue;

                    ccase.Condition?.OnVisited(context);
                    
                    if (i == 0)
                        dataContext.SetValueInt(StateVariableName, 1);
                    else
                        dataContext.SetValueInt(StateVariableName, (long)(-i + 1));
                    
                    if (ccase.ChildrenCount > 0)
                    {
                        return ccase.GetChild(0);
                    }    
                    return this.FindNextSibling();
                }

                for (int i = -id + 1; i < ChildrenCount; ++i)
                {
                    var ccase = GetChild(i);

                    if (ccase.Condition != null && !ccase.Condition.EvaluateBool(context))
                        continue;

                    ccase.Condition?.OnVisited(context);
                    
                    if (i == ChildrenCount - 1)
                        dataContext.SetValueInt(StateVariableName, (long)(2-ChildrenCount));
                    else
                        dataContext.SetValueInt(StateVariableName, (long)(i + 1));
                    
                    if (ccase.ChildrenCount > 0)
                    {
                        return ccase.GetChild(0);
                    }    
                    return this.FindNextSibling();
                }
            }

            return this.FindNextSibling();
        }

        public override string PrintPayload()
        {
            return ">< " + StateVariableContext + StateVariableName;
        }
    }
}