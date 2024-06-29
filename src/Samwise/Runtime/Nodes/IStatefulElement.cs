// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    // Piece of data that provide text (like a caption, a speech node or an option).
    // It's what you'll probably localize.
    public interface IStatefulElement
    {
        string StateVariableName { get; set; }
        string StateVariableContext { get; set; }
        bool UsesAnonymousVariable { get; set; }
    }
}