// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

using System.Collections.Generic;

namespace Peevo.Samwise
{
    public class SequenceBlock : IDialogueBlock
    {
        public virtual NextBlockPolicy NextBlockPolicy => NextBlockPolicy.ParentNext;
        public IBlockContainerNode Parent { get; private set; }
        public int ChildrenCount => children.Count;
        public IDialogueNode GetChild(int i) => children[i];

        public SequenceBlock(IBlockContainerNode parent) { Parent = parent; }

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

        public virtual string PrintSubtree(string indentationPrefix, string indentationUnit)
        {
            string o = "";
            for (int i=0; i<ChildrenCount; ++i)
                o += GetChild(i).PrintSubtree(indentationPrefix, indentationUnit) + (i == ChildrenCount - 1 ? "" : "\n");

            return o;
        }
        
        public string LookUpChildNodeTabsPrefix(IDialogueNode childNode, string indentationUnit)
        {
            // same indentation as block
            return this.LookUpTabsPrefix(indentationUnit);
        }

        List<IDialogueNode> children = new List<IDialogueNode>();
    }
}