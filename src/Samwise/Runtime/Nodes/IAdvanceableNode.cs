// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public interface IAdvanceableNode
    {
        IDialogueNode Advance(IDialogueSet dialogues);
    }
}