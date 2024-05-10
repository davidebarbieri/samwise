// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public class JoinNode : DialogueNode, IAutoNode
    {
        public string BranchName;

        public JoinNode(int sourceLine, string branchName) : base(sourceLine, sourceLine)
        {
            BranchName = branchName;
        }
        
        public IDialogueNode Next(IDialogueSet dialogues, IDialogueContext context)
        {
            return this.FindNextSibling();
        }

        public override string PrintPayload()
        {
            return  (string.IsNullOrEmpty(BranchName) ? "" : BranchName + " ") + "<=";
        }
    }
}