// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

using System.Collections.Generic;

namespace Peevo.Samwise
{
    public abstract class SelectionNode : DialogueNode, IAutoNode, IBlockContainerNode, IMultiCaseNode
    {
        public int ChildrenCount => children.Count;
        public SelectionCase GetChild(int i) => children[i];
        IDialogueBlock IBlockContainerNode.GetChild(int i) => children[i];
        public int CasesCount => children.Count;
        public ICase GetCase(int i) => children[i];

        public SelectionNode(int sourceLine) : base(sourceLine, sourceLine)
        {
        }

        public void AddCase(SelectionCase item)
        {
            children.Add(item);
        }

        public abstract IDialogueNode Next(IDialogueSet dialogues, IDialogueContext context);

        public override string PrintSubtree(string indentationPrefix, string indentationUnit)
        {
            string o = GetPreambleString(indentationPrefix) + PrintPayload() + GetTagsString() + "\n";

            for (int i = 0; i < children.Count; ++i)
                o += children[i].PrintSubtree(indentationUnit + indentationPrefix, indentationUnit) + (i == children.Count - 1 ? "" : "\n");

            return o;
        }

        public string LookUpChildBlockTabsPrefix(IDialogueBlock childBlock, string indentationUnit)
        {
            string prefix = indentationUnit + indentationUnit; // tab + "-" + tab
            
            var parentPrefix = Block?.LookUpChildNodeTabsPrefix(this, indentationUnit) ?? null;

            if (parentPrefix != null)
                prefix = parentPrefix + prefix;

            return prefix;
        }

        List<SelectionCase> children = new List<SelectionCase>();
    }
}