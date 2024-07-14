// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

using System.Collections.Generic;

namespace Peevo.Samwise
{
    public interface IChoosableNode: IDialogueNode
    {
        int OptionsCount { get; }
        Option GetOption(int index);

        IEnumerator<IOption> GetAvailableOptions(IDialogueContext context);
        string CharacterId { get; }
        double? Time { get; }

        IDialogueNode Next(IOption choice, IDialogueContext context);
    }
}