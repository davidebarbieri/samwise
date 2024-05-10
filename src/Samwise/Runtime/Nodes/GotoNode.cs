// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    // Goto Node (to other Dialogue)
    public class GotoNode : DialogueNode, IAutoNode, IReferencingNode
    {
        public string DestinationDialogueId { get; set; }
        public string DestinationLabel { get; set; }

        public GotoNode(int sourceLine, string dialogueId, string label) : base(sourceLine, sourceLine)
        {
            DestinationDialogueId = dialogueId;
            DestinationLabel = label;
        }

        public IDialogueNode Next(IDialogueSet dialogues, IDialogueContext context)
        {
            if (!dialogues.GetDialogue(DestinationDialogueId, out var dialogue))
                throw new DialogueNotFoundException(context, this);

            IDialogueNode nextNode = null;
            if (!string.IsNullOrEmpty(DestinationLabel))
                nextNode = dialogues.GetNodeFromLabel(DestinationDialogueId, DestinationLabel);
            else if (dialogue.ChildrenCount > 0)
                nextNode = dialogue.GetChild(0);

            if (nextNode == null)
                throw new DialogueNodeNotFoundException(context, DestinationDialogueId, DestinationLabel);

            return nextNode;
        }
        
        public override string PrintPayload()
        {
            string target = DestinationDialogueId;

            if (!string.IsNullOrEmpty(DestinationLabel))
                target += "." + DestinationLabel;

            return "-> " + target;
        }
    }

    public class LocalGotoNode : DialogueNode, IResolvableNode, IAutoNode, IReferencingNode
    {
        public string DestinationDialogueId => this.GetDialogue()?.Label;
        public string DestinationLabel { get; private set; }
        public IDialogueNode Destination { get; private set; }

        public void Resolve(IDialogueNode destination) { Destination = destination; }

        public LocalGotoNode(int sourceLine, string destinationLabel, IDialogueNode destination) : base(sourceLine, sourceLine)
        {
            DestinationLabel = destinationLabel;
            Destination = destination;
        }

        public IDialogueNode Next(IDialogueSet dialogues, IDialogueContext context)
        {
            return Destination;
        }

        public override string PrintPayload()
        {
            return "-> " + (string.IsNullOrEmpty(Destination.Label) ? "__undefined__" : Destination.Label);
        }
    }
}