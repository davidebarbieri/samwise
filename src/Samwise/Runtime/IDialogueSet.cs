// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public interface IDialogueSet
    {
        bool GetDialogue(string dialogueName, out Dialogue dialogue);
        IDialogueNode GetNodeFromLabel(string dialogueId, string label);
    }
}