// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public class OptionAlternative : ITextContent
    {
        public string Text { get; set; }

        public int SourceLineStart { get; set; }

        public int SourceLineEnd { get; set; }

        public TagData TagData { get; set; }
        public IBoolValue Condition { get; set; }
        public ChoiceNode Parent { get; set; }

        public OptionAlternative(Option option, int sourceLineStart, int sourceLineEnd, string text, IBoolValue condition, TagData tagData)
        {
            Parent = option.Parent;
            Text = text;
            Condition = condition;
            TagData = tagData;
            SourceLineStart = sourceLineStart;
            SourceLineEnd = sourceLineEnd;
        }

        public string GenerateUidPreamble(Dialogue dialogue)
        {
            return dialogue.Label + "_" + Parent.CharacterId + "_";
        }

        public Dialogue GetDialogue()
        {
            return Parent.GetDialogue();
        }
        public string PrintPayload()
        {
            var s = "| ";

            var attributes = GetAttributesString();

            if (!string.IsNullOrEmpty(attributes))
                s += "[" + GetAttributesString() + "] ";

            s += Text.Replace("\n", "↵\n");

            return s;
        }

        string GetAttributesString()
        {
            string attributes = "";

            if (Condition != null)
                attributes = Condition.ToString();

            return attributes;
        }

        public string PrintLine(string indentationUnit)
        {
            return Parent.LookUpTabsPrefix(indentationUnit) + indentationUnit + indentationUnit + PrintPayload() + DialogueNode.GetTagsString(TagData);
        }
    }

}