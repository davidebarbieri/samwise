// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public class SpeechNode : DialogueNode, IAdvanceableNode, ITextContent
    {
        public string CharacterId;
        public string Text { get; set; }

        public SpeechNode(int sourceLineStart, int sourceLineEnd, string characterId, string text) : base(sourceLineStart, sourceLineEnd)
        {
            CharacterId = characterId;
            Text = text;
        }

        public IDialogueNode Advance(IDialogueSet dialogues)
        {
            return this.FindNextSibling();
        }

        public override string PrintPayload()
        {
            return CharacterId + "> " + Text.Replace("\n", "↵\n").Replace("#", "##");
        }

        public override string GenerateUidPreamble(Dialogue dialogue)
        {
            return dialogue.Label + "_" + CharacterId + "_";
        }
    }
}