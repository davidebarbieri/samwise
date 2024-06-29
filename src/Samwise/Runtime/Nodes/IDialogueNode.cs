// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public interface IDialogueNode: IContent, ICheckable
    {
        string Label { get; set; }
        string PreCheck { get; set; }

        IDialogueBlock Block { get; set; }
        int BlockId { get; set; }

        string PrintSubtree(string indentationPrefix, string indentationUnit);
        string PrintPayload();

        InterruptibleNode InnermostInterruptibleNode {get; set;}
        
        string LookUpTabsPrefix(string indentationUnit);
    }

    public static class IDialogueNodeMethods
    {
        public static bool IsBranching(this IDialogueNode node)
        {
            return node.Condition != null || node.PreCheck != null;
        }

        public static IDialogueNode FindNextSibling(this IDialogueNode node)
        {
            do
            {
                var block = node.Block;
                int nextId = block.GetNextIndex(node.BlockId);

                if (nextId >= 0)
                {
                    return block.GetChild(nextId);
                }

                switch (block.NextBlockPolicy)
                {
                    case NextBlockPolicy.End:
                        return null;
                    case NextBlockPolicy.ReturnToParent:
                        return block.Parent;
                }

                node =  block.Parent;
            } while (node != null);

            return null;
        }
    }
}