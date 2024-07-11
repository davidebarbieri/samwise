// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public interface IChoosableNode: IDialogueNode
    {
        int OptionsCount { get; }
        Option GetOption(int i);
        string CharacterId { get; }
        double? Time { get; }

        IDialogueNode Next(Option choice, IDialogueContext context);
    }
}