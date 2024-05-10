// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

using System.Collections.Generic;

namespace Peevo.Samwise
{
    public class ConditionNode : DialogueNode, IAutoNode, IDialogueBlock, IBlockContainerNode
    {
        public int ChildrenCount => children.Count;
        public IDialogueNode GetChild(int i) => children[i];

        public NextBlockPolicy NextBlockPolicy => NextBlockPolicy.ParentNext;

        // The block is contained by this node
        public IBlockContainerNode Parent => this;

        public bool Inline { get; private set; }

        internal ConditionNode ElseCondition { get; private set; } // Next node in "if - else if - else if - else" chain
        
        int IBlockContainerNode.ChildrenCount => elseBlock == null ? 1 : 2;
        IDialogueBlock IBlockContainerNode.GetChild(int i) => i == 0 ? (IDialogueBlock)this : elseBlock;

        public ConditionNode(int sourceLineStart, int sourceLineEnd, bool inline) : base(sourceLineStart, sourceLineEnd) { Inline = inline; }


        public void PushChild(IDialogueNode node)
        {
            node.Block = this;
            node.BlockId = children.Count;
            
            children.Add(node);
        }

        public void PopChild()
        {
            children[children.Count - 1].Block = null;
            children.RemoveAt(children.Count - 1);
        }

        public int GetNextIndex(int i)
        {
            ++i;

            if (i < ChildrenCount)
                return i;
            return -1;
        }

        public IDialogueNode Next(IDialogueSet dialogues, IDialogueContext context)
        {
            if (ChildrenCount > 0)
                return GetChild(0);

            return this.FindNextSibling();
        }

        public override string GetAttributesString()
        {
            string attributes = base.GetAttributesString();

            if (!isElse)
                return attributes;

            if (string.IsNullOrEmpty(attributes))
                return "else";

            if (Condition != null)
                return "else " + attributes;
            return "else, " + attributes;
        }
                

        public override string PrintSubtree(string indentationPrefix, string indentationUnit)
        {
            string o;

            if (!Inline)
            {
                o = GetPreambleString(indentationPrefix, true);
                o += "\n";
            }
            else // if (Inline && ChildrenCount > 0)
            {
                o = GetPreambleString(indentationPrefix);
                o += GetChild(0).PrintSubtree("", indentationUnit) + (ChildrenCount == 1 && ElseCondition == null ? "" : "\n");
            }

            var subTabsPrefix = indentationUnit + indentationPrefix;

            for (int i= Inline ? 1 : 0; i<ChildrenCount; ++i)
                o += GetChild(i).PrintSubtree(subTabsPrefix, indentationUnit) + (i == ChildrenCount - 1 && ElseCondition == null ? "" : "\n");

            // Print Else Condition
            if (ElseCondition != null)
                o += ElseCondition.PrintSubtree(indentationPrefix, indentationUnit);

            return o;
        }

        public override string PrintPayload()
        {
            // This node has no payload
            return "";
        }

        // This node has blocks, and so it provides prefix to those 2 blocks (actually no additional indentation)
        public string LookUpChildBlockTabsPrefix(IDialogueBlock childBlock, string indentationUnit)
        {
            if (Block == null)
                return "";
            return Block.LookUpChildNodeTabsPrefix(this, indentationUnit);
        }

        // This node is also a block (the first one, when the condition is true)
        public string LookUpChildNodeTabsPrefix(IDialogueNode child, string indentationUnit) 
        {
            if (Inline && ChildrenCount > 0 && child == GetChild(0))
            {
                if (Block == null) 
                    return ""; 
                else 
                    return Block.LookUpChildNodeTabsPrefix(this, indentationUnit); // inline: same indentation as parent block
            }
            else
            {
                // indent
                if (Block == null) 
                    return indentationUnit; 
                else 
                    return indentationUnit + Block.LookUpChildNodeTabsPrefix(this, indentationUnit);
            }
        }




        internal ConditionNode GetLastElseCondition()
        {
            if (ElseCondition == null)
                return this;
            return ElseCondition.GetLastElseCondition();
        }

        internal void SetElseCondition(ConditionNode node)
        {
            if (elseBlock == null)
                elseBlock = new SingleBlock(this);

            elseBlock.PushChild(node);
            ElseCondition = node;
            node.isElse = true;
        }

        List<IDialogueNode> children = new List<IDialogueNode>();
        SingleBlock elseBlock;
        bool isElse;
    }
}