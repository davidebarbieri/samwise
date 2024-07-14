// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

using System.Collections.Generic;

namespace Peevo.Samwise
{
    public class OptionGroup : SequenceBlock
    {
        // Note to me: return property cannot be overridden because it's a property of the block, not of the option;
        // Imagine you have a jump into an option block that has two alternatives (a simple one, and a "return" alternative): 
        // when the block is over what should happen?
        
        public int GroupId { get; set; } // Used just to recover state when node uid were not saved

        public bool ReturnOption  { get; set; }
        public bool MuteOption  { get; set; }

        public override NextBlockPolicy NextBlockPolicy => ReturnOption ? NextBlockPolicy.ReturnToParent : NextBlockPolicy.ParentNext;

        public int OptionsCount => options.Count;
        public Option GetOption(int index) => options[index];

        public OptionGroup(ChoiceNode parent, int groupId, bool muteOption, bool returnOption) : base(parent)
        {
            GroupId = groupId;
            MuteOption = muteOption;
            ReturnOption = returnOption;
        }

        public void AddOption(Option option)
        {
            options.Add(option);
        }

        public Option GetAvailableOption(IDialogueContext context)
        {
            for (int i=0, count=options.Count; i<count; i++)
            {
                Option option = options[i];

                if (option.Condition == null || option.Condition.EvaluateBool(context))
                    return option;
            }

            return null;
        }
        
        public override string PrintSubtree(string indentationPrefix, string indentationUnit)
        {
            var s = "";

            for (int i=0, count=options.Count; i<count; ++i)
            {
                s += options[i].PrintLine(indentationUnit);

                if (i < count - 1)
                    s += "\n";
            }
            
            if (base.ChildrenCount > 0)
                s += "\n" + base.PrintSubtree(indentationUnit + indentationPrefix, indentationUnit);

            return s;
        }

        public bool IsFirstOption(Option option)
        {
            return options.Count > 0 && options[0] == option;
        }

        List<Option> options = new List<Option>(); 
    }

}