// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public class CheckResultBlock : SequenceBlock
    {
        public ITagData TagData { get; set; }
        public bool Pass { get; private set; }

        public new CheckNode Parent => (CheckNode)base.Parent;

        public CheckResultBlock(bool pass, CheckNode parent, ITagData tagData) : base(parent)
        {
            Pass = pass;
            TagData = tagData;
        }

        public override string PrintSubtree(string indentationPrefix, string indentationUnit)
        {
            var s = indentationPrefix + PrintPayload() + DialogueNode.GetTagsString(TagData); 

            if (base.ChildrenCount > 0)
                s += "\n" + base.PrintSubtree(indentationUnit + indentationPrefix, indentationUnit);

            return s;
        }

        public string PrintPayload()
        {
            var s = Pass ? "+" : "-";

            return s;
        }
    }
}