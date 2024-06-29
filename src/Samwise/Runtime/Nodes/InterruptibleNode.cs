// (c) Copyright 2023 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public class InterruptibleNode : DialogueNode, IAutoNode, IBlockContainerNode
    {
        public bool Reset; // Reset the variable when the node is entered
        public string Context;
        public string InterruptVariable;
        public InterruptibleNode Parent { get; private set; }
        public readonly SequenceBlock InterruptibleBlock;
        
        public int ChildrenCount => 1;
        public IDialogueBlock GetChild(int i) => InterruptibleBlock;

        public InterruptibleNode(bool reset, InterruptibleNode parent, int sourceLine, string context, string variable) : base(sourceLine, sourceLine) 
        {
            Reset = reset;
            Parent = parent;
            Context = context;
            InterruptVariable = variable;
            InterruptibleBlock = new SequenceBlock(this);
        }

        public bool CheckIfTriggered(IDialogueContext context)
        {
            return context.LookupDataContext(Context)?.GetValueBool(InterruptVariable) ?? false;
        }

        public InterruptibleNode FindTriggeredParent(IDialogueContext context)
        {
            if (Parent != null)
            {
                var parentTriggered = FindTriggeredParent(context);
                if (parentTriggered != null)
                    return parentTriggered;
            }
            
            if (CheckIfTriggered(context))
            {
                return this;
            }

            return null;
        }
        
        public IDialogueNode Next(IDialogueSet dialogues, IDialogueContext context)
        {
            if (Reset)
                context.LookupDataContext(Context)?.SetValueBool(InterruptVariable, false);

            bool isAlreadyInterrupted = CheckIfTriggered(context);

            if (isAlreadyInterrupted || InterruptibleBlock.ChildrenCount == 0)
                return this.FindNextSibling();

            return InterruptibleBlock.GetChild(0);
        }

        public bool IsEqualOrParent(InterruptibleNode node)
        {
            while (node != null)
            {
                if (node == this)
                    return true;

                node = node.Parent;
            }

            return false;
        }

        public override string PrintSubtree(string indentationPrefix, string indentationUnit)
        {
            string o = GetPreambleString(indentationPrefix) + PrintPayload() + GetTagsString() + "\n";
            
            for (int i = 0; i < InterruptibleBlock.ChildrenCount; ++i)
                o += InterruptibleBlock.GetChild(i).PrintSubtree(indentationUnit + indentationPrefix, indentationUnit) + (i == InterruptibleBlock.ChildrenCount - 1 ? "" : "\n");

            return o;
        }

        public override string PrintPayload()
        {
            return (Reset ? "!! " : "! ") + Context + InterruptVariable;
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