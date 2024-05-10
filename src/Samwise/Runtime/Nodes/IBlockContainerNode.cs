// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public interface IBlockContainerNode : IDialogueNode
    {
        int ChildrenCount { get; }
        IDialogueBlock GetChild(int i);
        string LookUpChildBlockTabsPrefix(IDialogueBlock childBlock, string indentationUnit);
    }
}