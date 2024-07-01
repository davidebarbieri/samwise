// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public class SelectionCase : SequenceBlock, ICase, IContent
    {
        public IBoolValue Condition { get; set; }
        public TagData TagData { get; set; }

        public int SourceLine {get; internal set;}
        public int SourceLineStart => SourceLine;
        public int SourceLineEnd => SourceLine;

        public new SelectionNode Parent => (SelectionNode)base.Parent;
        IMultiCaseNode ICase.Parent => (SelectionNode)base.Parent;
        public Dialogue GetDialogue() => Parent.GetDialogue();

        public int ContentCount => 1;
        public IContent GetContent(int index) => this;

        public SelectionCase(int sourceLine, SelectionNode parent, IBoolValue condition, TagData tagData) : base(parent)
        {
            Condition = condition;
            TagData = tagData;
            SourceLine = sourceLine;
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
            var s = "-";

            var attributes = GetAttributesString();

            if (!string.IsNullOrEmpty(attributes))
                s += " [" + GetAttributesString() + "]";

            return s;
        }

        public string PrintLine(string indentationUnit) 
        { 
            return Parent.LookUpTabsPrefix(indentationUnit) + indentationUnit + PrintPayload() + DialogueNode.GetTagsString(TagData);
        }

        public override string ToString()
        {
            return PrintLine("\t");
        }

        public string GenerateUidPreamble(Dialogue dialogue)
        {
            return dialogue.Label + "_";
        }

        string GetAttributesString()
        {
            string attributes = "";

            if (Condition != null)
                attributes = Condition.ToString();

            return attributes;
        }
    }
}