// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public class Option : SequenceBlock, IOption
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public IBoolValue Condition { get; set; }
        public TagData TagData { get; set; }
        public bool MuteOption  { get; set; }
        public bool ReturnOption  { get; set; }
        public double? Time => OverriddenTime.HasValue ? OverriddenTime : Parent.Time; // this time or parent's time
        public double? OverriddenTime;
        public string Check;
        public bool IsPreCheck;

        public Dialogue GetDialogue() => Parent.GetDialogue();
        public int SourceLineStart {get; internal set;}
        public int SourceLineEnd {get; internal set;}

        public IDialogueBlock Block => this;
        public override NextBlockPolicy NextBlockPolicy => ReturnOption ? NextBlockPolicy.ReturnToParent : NextBlockPolicy.ParentNext;
        public new IChoosableNode Parent => (IChoosableNode)base.Parent;
        IMultiCaseNode ICase.Parent => (ChoiceNode)base.Parent;

        public Option(int sourceLineStart, int sourceLineEnd, int optionId, ChoiceNode parent, string text, bool muteOption, bool returnOption, IBoolValue condition, TagData tagData, double? time, string check, bool isPreCheck) : base(parent)
        {
            Id = optionId;
            Text = text;
            MuteOption = muteOption;
            ReturnOption = returnOption;
            Condition = condition;
            TagData = tagData;
            OverriddenTime = time;
            SourceLineStart = sourceLineStart;
            SourceLineEnd = sourceLineEnd;
            Check = check;
            IsPreCheck = isPreCheck;
        }

        public bool IsAvailable(IDialogueContext context)
        {
            if (Condition == null)
                return true;

            return Condition.EvaluateBool(context);
        }

        public bool HasCheck(out bool isPreCheck, out string checkName)
        {
            isPreCheck = IsPreCheck;
            checkName = Check;
            return Check != null;
        }

        public bool HasTime(out double time)
        {
            if (Time.HasValue)
            {
                time = Time.Value;
                return true;
            }

            time = 0;
            return false;
        }

        public void OnVisited(IDialogueContext context)
        {
            if (Condition == null)
                return;

            if (Condition.EvaluateBool(context))
            {
                Condition.OnVisited(context);
                return;
            }
        }
        public override string PrintSubtree(string indentationPrefix, string indentationUnit)
        {
            var s = indentationPrefix + PrintPayload() + DialogueNode.GetTagsString(TagData);
            
            if (base.ChildrenCount > 0)
                s += "\n" + base.PrintSubtree(indentationUnit + indentationPrefix, indentationUnit);

            return s;
        }

        public string PrintPayload()
        {
            var s = "";
            
            if (ReturnOption)
                s += MuteOption ? "<-- " : "<- "; 
            else
                s += MuteOption ? "-- " : "- "; 

            var attributes = GetAttributesString();

            if (!string.IsNullOrEmpty(attributes))
                s += "[" + GetAttributesString() + "] ";

            s += Text.Replace("\n", "↵\n");;

            return s;
        }

        string GetAttributesString()
        {
            string attributes = "";

            if (Condition != null)
                attributes = Condition.ToString();

            if (Check != null)
            {
                if (!string.IsNullOrEmpty(attributes))
                    attributes += ", ";

                attributes += IsPreCheck ? "precheck" : "check";
                if (!string.IsNullOrEmpty(Check))
                {
                    attributes += " " + Check;
                }
            }

            if (OverriddenTime.HasValue)
            {
                if (!string.IsNullOrEmpty(attributes))
                    attributes += ", ";

                attributes += OverriddenTime.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) + "s";
            }

            return attributes;
        }

        public string GenerateUidPreamble(Dialogue dialogue)
        {
            return dialogue.Label + "_" + Parent.CharacterId + "_";
        }

        public string PrintLine(string indentationUnit) 
        { 
            return Parent.LookUpTabsPrefix(indentationUnit) + indentationUnit + PrintPayload() + DialogueNode.GetTagsString(TagData);
        }

        public override string ToString()
        {
            return PrintLine("\t");
        }
    }
}