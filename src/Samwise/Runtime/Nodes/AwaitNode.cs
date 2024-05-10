// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public class AwaitNode : DialogueNode, IAutoNode, IReferencingNode
    {
        public string DestinationDialogueId { get; set; }
        public string DestinationLabel { get; set; }

        public AwaitNode(int sourceLine, string dialogueId, string label) : base(sourceLine, sourceLine)
        {
            DestinationDialogueId = dialogueId;
            DestinationLabel = label;
        }
        
        public IDialogueNode Next(IDialogueSet dialogues, IDialogueContext context)
        {
            return this.FindNextSibling();
        }

        public override string PrintPayload()
        {
            string dest = string.IsNullOrEmpty(DestinationDialogueId) ? DestinationLabel : 
                string.IsNullOrEmpty(DestinationLabel) ? DestinationDialogueId : 
                    DestinationDialogueId + "." + DestinationLabel;
            return "<=> " + dest;
        }
    }
    
    public class LocalAwaitNode : DialogueNode, IResolvableNode, IAutoNode, IReferencingNode
    {
        public string DestinationDialogueId => this.GetDialogue()?.Label;
        public string DestinationLabel { get; private set; }
        public IDialogueNode Destination { get; private set; }
        
        public void Resolve(IDialogueNode destination) { Destination = destination; }

        public LocalAwaitNode(int sourceLine, string destinationLabel, IDialogueNode destination) : base(sourceLine, sourceLine)
        {
            DestinationLabel = destinationLabel;
            Destination = destination;
        }
        
        public IDialogueNode Next(IDialogueSet dialogues, IDialogueContext context)
        {
            return this.FindNextSibling();
        }

        public override string PrintPayload()
        {
            string dest = (string.IsNullOrEmpty(Destination.Label) ? "__undefined__" : Destination.Label);
            return "<=> " + dest;
        }
    }

    public class ExternalAwaitNode : DialogueNode, IAutoNode
    {
        public IAsyncCode Code;

        public ExternalAwaitNode(int sourceLine, IAsyncCode code) : base(sourceLine, sourceLine)
        {
            Code = code;
        }
        
        public IDialogueNode Next(IDialogueSet dialogues, IDialogueContext context)
        {
            return this.FindNextSibling();
        }

        public override string PrintPayload()
        {
            return "<=> {{" + Code.ToString() + "}}";
        }
    }
}