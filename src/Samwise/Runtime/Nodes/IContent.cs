// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    // A taggable and checkable part of a node
    public interface IContent: ISource, ITaggable, IConditional
    {
        string PrintLine(string indentationUnit);
        string GenerateUidPreamble(Dialogue dialogue);
    }
}