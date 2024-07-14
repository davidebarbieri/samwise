// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public interface IOption : ICase, ITextContent, ICheckableContent, ITimeable
    {
        bool MuteOption { get; }
        bool ReturnOption { get; }

        bool IsAvailable(IDialogueContext context);

        IDialogueBlock Block { get; }
        new IChoosableNode Parent { get; }
    }
}