// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public class ForkNode : DialogueNode, IAutoNode, IReferencingNode
    {
        public string BranchName;

        public string DestinationDialogueId { get; set;}
        public string DestinationLabel { get; set;}

        public ForkNode(int sourceLine, string branchName, string dialogueId, string label) : base(sourceLine, sourceLine)
        {
            BranchName = branchName;
            DestinationDialogueId = dialogueId;
            DestinationLabel = label;
        }
        
        public IDialogueNode Next(IDialogueSet dialogues, IDialogueContext context)
        {
            return this.FindNextSibling();
        }
        
        public override string PrintPayload()
        {
            string dest = string.IsNullOrEmpty(DestinationDialogueId) ? DestinationLabel : 
                string.IsNullOrEmpty(DestinationLabel) ? DestinationDialogueId : 
                    DestinationDialogueId + "." + DestinationLabel;

            return (string.IsNullOrEmpty(BranchName) ? "" : BranchName + " ") + "=> " + dest;
        }
    }

    public class LocalForkNode : DialogueNode, IResolvableNode, IAutoNode, IReferencingNode
    {
        public string BranchName { get; private set; }

        public string DestinationDialogueId => this.GetDialogue()?.Label;
        public string DestinationLabel { get; private set; }
        public IDialogueNode Destination { get; private set; }
        
        public void Resolve(IDialogueNode destination) { Destination = destination; }

        public LocalForkNode(int sourceLine, string branchName, string destinationLabel, IDialogueNode destination) : base(sourceLine, sourceLine)
        {
            DestinationLabel = destinationLabel;
            BranchName = branchName;
            Destination = destination;
        }
        
        public IDialogueNode Next(IDialogueSet dialogues, IDialogueContext context)
        {
            return this.FindNextSibling();
        }
        
        public override string PrintPayload()
        {
            string dest = (string.IsNullOrEmpty(Destination.Label) ? "__undefined__" : Destination.Label);
            string o = (string.IsNullOrEmpty(BranchName) ? "" : BranchName + " ") + "=> " + dest;

            return o;
        }
    }

    public class ExternalForkNode : DialogueNode, IAutoNode
    {
        public string BranchName;

        public IAsyncCode Code;

        public ExternalForkNode(int sourceLine, string branchName, IAsyncCode code) : base(sourceLine, sourceLine)
        {
            BranchName = branchName;
            Code = code;
        }
        
        public IDialogueNode Next(IDialogueSet dialogues, IDialogueContext context)
        {
            return this.FindNextSibling();
        }
        
        public override string PrintPayload()
        {
            string dest = Code.ToString();
            return (string.IsNullOrEmpty(BranchName) ? "" : BranchName + " ") + "=> {{" + dest + "}}";
        }
    }

    public class AnonymousForkNode : DialogueNode, IAutoNode, IBlockContainerNode
    {
        public string BranchName { get; private set; }

        public readonly AnonymousSequenceBlock AnonymousBlock;
        
        public int ChildrenCount => 1;
        public IDialogueBlock GetChild(int i) => AnonymousBlock;

        public AnonymousForkNode(int sourceLine, string branchName) : base(sourceLine, sourceLine)
        {
            BranchName = branchName;
            AnonymousBlock = new AnonymousSequenceBlock(this);
        }
        
        public IDialogueNode Next(IDialogueSet dialogues, IDialogueContext context)
        {
            return this.FindNextSibling();
        }

        public override string PrintSubtree(string indentationPrefix, string indentationUnit)
        {
            string o = GetPreambleString(indentationPrefix) + PrintPayload() + GetTagsString() + "\n";
            
            for (int i = 0; i < AnonymousBlock.ChildrenCount; ++i)
                o += AnonymousBlock.GetChild(i).PrintSubtree(indentationUnit + indentationPrefix, indentationUnit) + (i == AnonymousBlock.ChildrenCount - 1 ? "" : "\n");

            return o;
        }
        
        public override string PrintPayload()
        {
            string o = (string.IsNullOrEmpty(BranchName) ? "" : BranchName + " ") + "=>";

            return o;
        }
        

        public string LookUpChildBlockTabsPrefix(IDialogueBlock childBlock, string indentationUnit)
        {
            string prefix = indentationUnit; // tab
            
            var parentPrefix = Block?.LookUpChildNodeTabsPrefix(this, indentationUnit) ?? null;

            if (parentPrefix != null)
                prefix = parentPrefix + prefix;

            return prefix;
        }
    }
}