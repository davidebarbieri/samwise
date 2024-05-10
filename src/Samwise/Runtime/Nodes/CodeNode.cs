// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public class CodeNode : DialogueNode, IAutoNode
    {
        
        public IStatement Statement;

        public CodeNode(int sourceLine, IStatement statement) : base(sourceLine, sourceLine)
        {
            Statement = statement;
        }    
        
        public IDialogueNode Next(IDialogueSet dialogues, IDialogueContext context)
        {
            Statement.Execute(context);
            return this.FindNextSibling();
        }
        
        public override string PrintPayload()
        {
            return "{ " + Statement.ToString() + " }";
        }
    }

    public class ExternalCodeNode : DialogueNode, IAutoNode
    {
        
        public IStatement Statement;

        public ExternalCodeNode(int sourceLine, IStatement statement) : base(sourceLine, sourceLine)
        {
            Statement = statement;
        }    
        
        public IDialogueNode Next(IDialogueSet dialogues, IDialogueContext context)
        {
            Statement.Execute(context);
            return this.FindNextSibling();
        }
        
        public override string PrintPayload()
        {
            return "{{ " + Statement.ToString() + " }}";
        }
    }
}