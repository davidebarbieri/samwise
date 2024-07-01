// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

using System.Collections.Generic;

namespace Peevo.Samwise
{
    public interface IOption: ITaggable, ICheckable, ISource
    {
        int Id { get; }
        ChoiceNode Parent { get; }
        bool MuteOption { get; }
        bool ReturnOption { get; }

        ITextContent DefaultContent { get; }
        IReadOnlyList<ITextContent> AlternativeContents { get; }

        // Don't show this option if this returns false, 
        // otherwise textContent will show the content to use to preview the choice
        bool IsAvailable(IDialogueContext context, out ITextContent textContent);

        // Actual text content. Could be the default one or an alternative, or null if not available
        ITextContent GetTextContent(IDialogueContext context);

        // Challenge Check Option?
        bool HasCheck(out bool isPreCheck, out string checkName);

        // Time-based Option?
        bool HasTime(out double time);
    }
}