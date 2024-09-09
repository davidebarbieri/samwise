// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public class CaptionNode : DialogueNode, IAdvanceableNode, ITextContent
    {
        public string Text { get; set; }

        public CaptionNode(int sourceLineStart, int sourceLineEnd, string text) : base(sourceLineStart, sourceLineEnd)
        {
            Text = text;
        }

        public IDialogueNode Advance(IDialogueSet dialogues)
        {
            return this.FindNextSibling();
        }

        public override string PrintPayload()
        {
            return  "* " + Text.Replace("\n", "↵\n").Replace("#", "##");
        }

        public override string GenerateUidPreamble(Dialogue dialogue)
        {
            return dialogue.Label + "_Caption_";
        }
    }
}