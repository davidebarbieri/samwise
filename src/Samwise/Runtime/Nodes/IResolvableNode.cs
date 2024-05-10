// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public interface IResolvableNode
    {
        void Resolve(IDialogueNode destination);
    }
}