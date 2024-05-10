using System.Collections.Generic;

namespace Peevo.Samwise
{
    // Sample IDialogueSet implementation
    public class DialogueSet : IDialogueSet
    {
        public DialogueSet() {}

        public DialogueSet(IList<Dialogue> dialogues)
        {
            AddDialogues(dialogues);
        }

        public void AddDialogue(Dialogue dialogue)
        {
            symbolMap[dialogue.Label] = dialogue;
        }

        public void AddDialogues(IList<Dialogue> dialogues)
        {
            for (int i=0, count=dialogues.Count; i<count; ++i)
                symbolMap[dialogues[i].Label] = dialogues[i];
        }

        public bool GetDialogue(string dialogueSymbol, out Dialogue dialogue)
        {
            return symbolMap.TryGetValue(dialogueSymbol, out dialogue);
        }

        public IDialogueNode GetNodeFromLabel(string dialogueSymbol, string label)
        {
            if (symbolMap.TryGetValue(dialogueSymbol, out var dialogue))
            {
                return dialogue.GetNodeFromLabel(label);
            }
            return null;
        }

        Dictionary<string, Dialogue> symbolMap = new Dictionary<string, Dialogue>();
    }
}