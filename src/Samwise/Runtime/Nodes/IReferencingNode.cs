// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public interface IReferencingNode : IDialogueNode
    {
        string DestinationDialogueId { get; }
        string DestinationLabel { get; }
    }
}