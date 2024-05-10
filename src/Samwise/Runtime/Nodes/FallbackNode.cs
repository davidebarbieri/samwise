// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public class FallbackNode : SelectionNode
    {
        public FallbackNode(int sourceLine) : base(sourceLine) {}

        public override IDialogueNode Next(IDialogueSet dialogues, IDialogueContext context)
        {
            for (int i = 0; i < ChildrenCount; ++i)
            {
                var ccase = GetChild(i);

                if (ccase.Condition != null && !ccase.Condition.EvaluateBool(context))
                    continue;

                ccase.Condition?.OnVisited(context);

                if (ccase.ChildrenCount > 0)
                    return ccase.GetChild(0);
                break;
            }

            return this.FindNextSibling();
        }
        
        public override string PrintPayload()
        {
            return "?";
        }
    }
}