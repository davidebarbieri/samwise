// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    [System.Serializable]
    public class DialogueNodeNotFoundException : DialogueException
    {
        public string DialogueId;
        public string Label;

        public DialogueNodeNotFoundException(IDialogueContext context, string dialogueId, string label) 
            : base(context, $"Label {dialogueId}.{label} not found.") { DialogueId = dialogueId; Label = label; }

        public override string ToString()
        {
            return "Unable to find node reference (" + DialogueId + "." + Label + ")";
        }
    }
}