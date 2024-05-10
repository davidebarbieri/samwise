using System;
using System.Collections.Generic;

namespace Peevo.Samwise.Wasm
{
    public class ParsedDialogueFile
    {
        public string Filename;
        public readonly DebugInformation DebugInformation = new DebugInformation();

        public ParsedDialogueFile(string filename)
        {
            Filename = filename;
        }

        public void AddDialogue(Dialogue dialogue)
        {
            DebugInformation.Gather(dialogue);
        }
    }
}