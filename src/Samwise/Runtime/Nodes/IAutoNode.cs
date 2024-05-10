// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public interface IAutoNode : IDialogueNode
    {
        IDialogueNode Next(IDialogueSet dialogues, IDialogueContext context);
    }
}