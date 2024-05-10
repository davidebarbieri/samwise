// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public interface IExternalCodeContext
    {
        long Uid { get; }

        IDialogueContext Parent { get; }

        // Call this when the external code is ended
        void OnEnd();
    }
}