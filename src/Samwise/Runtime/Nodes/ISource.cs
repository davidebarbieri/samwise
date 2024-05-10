// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public interface ISource
    {
        Dialogue GetDialogue();
        int SourceLineStart { get; }
        int SourceLineEnd { get; }
    }
}