// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public interface IOption : ICase, ITextContent, ICheckableContent, ITimeable
    {
        int Id { get; set; }
        bool MuteOption { get; set; }
        bool ReturnOption { get; set; }

        bool IsAvailable(IDialogueContext context);

        IDialogueBlock Block { get; }
        new IChoosableNode Parent { get; }
    }
}