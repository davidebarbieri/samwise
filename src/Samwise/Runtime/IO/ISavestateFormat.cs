// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

using System.IO;

namespace Peevo.Samwise
{
    public interface ISavestateFormat
    {
        int Version { get; }
        
        bool SaveState(BinaryWriter writer, DialogueMachine machine, IExternalContextSaveResolver externalContextResolver = null);
        bool LoadState(BinaryReader reader, DialogueMachine machine, System.Func<string, Dialogue> onUnresolvedDialogue, IExternalContextLoadResolver externalContextResolver = null);

    }
}