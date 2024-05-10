// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public class WaitTimeNode : DialogueNode, IAdvanceableNode
    {
        public double Time;

        public WaitTimeNode(int sourceLine, double time) : base(sourceLine, sourceLine)
        {
            Time = time;
        }

        public IDialogueNode Advance(IDialogueSet dialogues)
        {
            return this.FindNextSibling();
        }

        public override string PrintPayload()
        {
            return "{ " + "wait " + Time.ToString(System.Globalization.CultureInfo.InvariantCulture) + "s" + " }";
        }
    }

    public class WaitExpressionNode : DialogueNode
    {
        public IBoolValue Expression;

        public WaitExpressionNode(int sourceLine, IBoolValue expression) : base(sourceLine, sourceLine)
        {
            Expression = expression;
        }

        public override string PrintPayload()
        {
            return "{ " + "wait " + Expression.ToString() + " }";
        }
    }
}