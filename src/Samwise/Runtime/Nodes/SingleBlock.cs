// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public class SingleBlock : IDialogueBlock
    {
        public virtual NextBlockPolicy NextBlockPolicy => NextBlockPolicy.ParentNext;
        public IBlockContainerNode Parent { get; private set; }
        public int ChildrenCount => 1;
        public IDialogueNode GetChild(int i) => child;

        public SingleBlock(IBlockContainerNode parent) { Parent = parent; }

        public void PushChild(IDialogueNode node)
        {
            node.Block = this;
            node.BlockId = 0;

            child = node;
        }

        public void PopChild()
        {
            child = null;
        }

        public int GetNextIndex(int i)
        {
            return -1;
        }

        public virtual string PrintSubtree(string indentationPrefix, string indentationUnit)
        {
            return child.PrintSubtree(indentationPrefix, indentationUnit);
        }

        public string LookUpChildNodeTabsPrefix(IDialogueNode childNode, string indentationUnit)
        {
            // same indentation as block
            return this.LookUpTabsPrefix(indentationUnit);
        }

        IDialogueNode child;
    }
}