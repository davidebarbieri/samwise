using System;
using System.Collections.Generic;

namespace Peevo.Samwise.Wasm
{
    public class CodebaseDatabase :IDialogueSet
    {    
        public void AddDialogue(string filename, Dialogue dialogue)
        {
            dialogueSet.AddDialogue(dialogue);
            dialogueToFile[dialogue] = filename;

            // Gather references
            GatherReferences(filename, dialogue);
        }

        public string GetFilename(Dialogue dialogue)
        {
            if (dialogueToFile.TryGetValue(dialogue, out var filename))
                return filename;
            return null;
        }

        public bool GetDialogue(string dialogueName, out Dialogue dialogue)
        {
            return dialogueSet.GetDialogue(dialogueName, out dialogue);
        }

        public IDialogueNode GetNodeFromLabel(string dialogueId, string label)
        {
            return dialogueSet.GetNodeFromLabel(dialogueId, label);
        }

        public HashSet<ReferenceEntry> GetReferences(string fullSymbol)
        {
            if (references.TryGetValue(fullSymbol, out var refs))
                return refs;

            return null;
        }

        void GatherReferences(string file, Dialogue dialogue)
        {
            // find Referencing Nodes
            foreach (var node in dialogue.Traverse<IReferencingNode>())
            {
                var targetDialogue = node.DestinationDialogueId;
                var targetLabel = node.DestinationLabel;

                if (node is ExitNode)
                    continue;

                var fullSymbol = targetDialogue;
                if (!string.IsNullOrEmpty(targetLabel))
                    fullSymbol += "." + targetLabel;

                if (!references.TryGetValue(fullSymbol, out var refs))
                    references[fullSymbol] = refs = new HashSet<ReferenceEntry>();

                refs.Add(new ReferenceEntry {file=file, line=node.SourceLineStart});

                if (!referencedSymbols.TryGetValue(file, out var refSymbols))
                    referencedSymbols[file] = refSymbols = new HashSet<string>();

                refSymbols.Add(fullSymbol);
            }
        }

        public void ResetReferences(string file)
        {
            if (referencedSymbols.TryGetValue(file, out var refSymbols))
            {
                foreach (var refEntry in refSymbols)
                {
                    references[refEntry].RemoveWhere((a) => a.file == file);
                }
            }

            referencedSymbols.Remove(file);
        }

        public struct ReferenceEntry : IEquatable<ReferenceEntry>
        {
            public string file;
            public int line;

            public override bool Equals(object obj)
            {
                if (obj is ReferenceEntry other)
                {
                    return Equals(other);
                }
                return false;
            }

            public bool Equals(ReferenceEntry other)
            {
                return file == other.file && line == other.line;
            }

            public override int GetHashCode()
            {
                int hashFile = file != null ? file.GetHashCode() : 0;
                return hashFile ^ line;
            }
        }

        DialogueSet dialogueSet = new DialogueSet();

        Dictionary<string, HashSet<ReferenceEntry>> references = new Dictionary<string, HashSet<ReferenceEntry>>();
        Dictionary<string, HashSet<string>> referencedSymbols = new Dictionary<string, HashSet<string>>();
        Dictionary<Dialogue, string> dialogueToFile = new Dictionary<Dialogue, string>();

    }
}