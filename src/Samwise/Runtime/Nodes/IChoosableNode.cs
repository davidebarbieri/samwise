// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public interface IChoosableNode: IDialogueNode
    {
        int OptionsCount { get; }
        IOption GetOption(int i);
        string CharacterId { get; }
        double? Time { get; }

        IDialogueNode Next(IOption choice, IDialogueContext context);
    }
}