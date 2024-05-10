// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    [System.Serializable]
    public class DialogueNotFoundException : DialogueException
    {
        public IReferencingNode CausingNode;

        public DialogueNotFoundException(IDialogueContext context, IReferencingNode causingNode) : base(context, $"Dialogue {causingNode.DestinationDialogueId} not found.") { CausingNode = causingNode; }
 
    }
}