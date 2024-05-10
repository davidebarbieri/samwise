// (c) Copyright 2022 Davide 'PeevishDave' Barbieri
using System.Collections.Generic;


namespace Peevo.Samwise
{
    public interface IDialogueBlock
    {
        IBlockContainerNode Parent { get; }
        NextBlockPolicy NextBlockPolicy { get; }

        int ChildrenCount { get; }
        IDialogueNode GetChild(int i);
        void PushChild(IDialogueNode node);
        void PopChild();

        int GetNextIndex(int i);

        string LookUpChildNodeTabsPrefix(IDialogueNode childNode, string indentationUnit);
    }

    public static class IDialogueBlockExtensions
    {
        public static string LookUpTabsPrefix(this IDialogueBlock block, string indentationUnit)
        {
            if (block.Parent == null)
                return "";

            return block.Parent.LookUpChildBlockTabsPrefix(block, indentationUnit);
        }

        
        // Find all nodes
        public static IEnumerable<IDialogueNode> Traverse(this IDialogueBlock block)
        {
            for (int i=0, count = block.ChildrenCount; i<count; ++i)
            {
                var node = block.GetChild(i);
                yield return node;

                var blockContainer = node as IBlockContainerNode;

                if (blockContainer != null)
                {
                    for (int ii=0, icount = blockContainer.ChildrenCount; ii<icount; ++ii)
                    {
                        foreach (var subNode in Traverse(blockContainer.GetChild(ii)))
                            yield return subNode;
                    }
                }
            }
        }

        // Find all nodes of type
        public static IEnumerable<IDialogueNode> Traverse(this IDialogueBlock block, System.Type type)
        {
            for (int i=0, count = block.ChildrenCount; i<count; ++i)
            {
                var node = block.GetChild(i);

                if (type.IsInstanceOfType(node))
                    yield return node;

                var blockContainer = node as IBlockContainerNode;

                if (blockContainer != null)
                {
                    for (int ii=0, icount = blockContainer.ChildrenCount; ii<icount; ++ii)
                    {
                        foreach (var subNode in Traverse(blockContainer.GetChild(ii), type))
                            yield return subNode;
                    }
                }
            }
        }

        public static IEnumerable<T> Traverse<T>(this IDialogueBlock block) where T : IDialogueNode
        {
            for (int i=0, count = block.ChildrenCount; i<count; ++i)
            {
                var node = block.GetChild(i);

                if (node is T tNode)
                    yield return tNode;

                var blockContainer = node as IBlockContainerNode;

                if (blockContainer != null)
                {
                    for (int ii=0, icount = blockContainer.ChildrenCount; ii<icount; ++ii)
                    {
                        foreach (var subNode in Traverse<T>(blockContainer.GetChild(ii)))
                            yield return subNode;
                    }
                }
            }
        }
    }
}