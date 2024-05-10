// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

using System.Collections.Generic;

namespace Peevo.Samwise
{
    public class DebugInformation
    {
        public string SourceFile {get; private set;}

        public DebugInformation(string sourceFile = "")
        {
            SourceFile = sourceFile;
        }

        // Gather information from dialogue. In order to work, dialogue must be in the same source file (real or virtual) as any other dialogue that's gathered in this object.
        public void Gather(Dialogue dialogue)
        {
            for (int i=dialogue.SourceLineStart; i<=dialogue.SourceLineEnd; ++i)
            {
                lineToDialogue[i] = dialogue;
            }

            GatherBlock(dialogue);
        }

        public Dialogue GetDialogue(int sourceLine)
        {
            if (sourceLine < 0)
                return null;

            return lineToDialogue.TryGetValue(sourceLine, out var dialogue) ? dialogue : null;
        }

        public IDialogueNode GetDialogueNode(int sourceLine)
        {
            if (sourceLine < 0)
                return null;
                
            return lineToNode.TryGetValue(sourceLine, out var node) ? node : null;
        }

        void GatherBlock(IDialogueBlock block)
        {
            for (int i=0; i < block.ChildrenCount; ++i)
            {
                var node = block.GetChild(i);

                for (int line=node.SourceLineStart; line<=node.SourceLineEnd; ++line)
                    lineToNode[line] = node;

                if (node is IBlockContainerNode blockNode)
                {
                    for (int j=0; j<blockNode.ChildrenCount; ++j)
                        {
                            GatherBlock(blockNode.GetChild(j));
                        }
                }
            }
        }

        Dictionary<int, Dialogue> lineToDialogue = new Dictionary<int, Dialogue>();
        Dictionary<int, IDialogueNode> lineToNode = new Dictionary<int, IDialogueNode>();
    }
}
