// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public class CheckNode : DialogueNode, IBlockContainerNode, ICheckable
    {
        public string Name;

        public CheckResultBlock PassBlock;
        public CheckResultBlock FailBlock;

        public int ChildrenCount => (PassBlock != null ? 1 : 0) + (FailBlock != null ? 1 : 0);
        public IDialogueBlock GetChild(int i) => i == 0 && PassBlock != null ? PassBlock : FailBlock;

        public CheckNode(int sourceLine, string name) : base(sourceLine, sourceLine)
        {
            Name = name;
        }

        public IDialogueNode Next(IDialogueContext context)
        {
            var dataContext = context.DataContext;

            if (dataContext.GetValueBool("bPass"))
            {
                if (PassBlock != null && PassBlock.ChildrenCount > 0)
                {
                    return PassBlock.GetChild(0);
                }  
            }
            else
            {
                if (FailBlock != null && FailBlock.ChildrenCount > 0)
                {
                    return FailBlock.GetChild(0);
                }  
            }

            return this.FindNextSibling();
        }

        public override string PrintSubtree(string indentationPrefix, string indentationUnit)
        {
            string o = GetPreambleString(indentationPrefix) + PrintPayload() + GetTagsString() + "\n";

            var hasPassBlock = PassBlock != null && PassBlock.ChildrenCount > 0;
            var hasFailBlock = FailBlock != null && FailBlock.ChildrenCount > 0;

            if (hasPassBlock)
            {
                o += PassBlock.PrintSubtree(indentationUnit + indentationPrefix, indentationUnit);

                if (hasFailBlock)
                    o += "\n";
            }
                
            if (hasFailBlock)
            {
                o += FailBlock.PrintSubtree(indentationUnit + indentationPrefix, indentationUnit);
            }

            return o;
        }

        public override string PrintPayload()
        {
            return "$ " + Name;
        }

        public string LookUpChildBlockTabsPrefix(IDialogueBlock childBlock, string indentationUnit)
        {
            string prefix = indentationUnit + indentationUnit; // tab + "-" or "+" + tab
            
            var parentPrefix = Block?.LookUpChildNodeTabsPrefix(this, indentationUnit) ?? null;

            if (parentPrefix != null)
                prefix = parentPrefix + prefix;

            return prefix;
        }
    }
}