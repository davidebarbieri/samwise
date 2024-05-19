// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public class CancelNode : DialogueNode, IAutoNode
    {
        public string BranchName;

        public CancelNode(int sourceLine, string branchName) : base(sourceLine, sourceLine)
        {
            BranchName = branchName;
        }
        
        public IDialogueNode Next(IDialogueSet dialogues, IDialogueContext context)
        {
            return this.FindNextSibling();
        }

        public override string PrintPayload()
        {
            return  (string.IsNullOrEmpty(BranchName) ? "" : BranchName + " ") + "<!=";
        }
    }
}