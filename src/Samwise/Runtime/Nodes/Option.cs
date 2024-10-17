// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public class Option : IOption
    {
        public int Id {get; private set; }
        public int IdWithinGroup {get; private set; }

        public string Text { get; set; }
        public IBoolValue Condition { get; set; }
        public ITagData TagData { get; set; }
        public bool MuteOption => OptionGroup.MuteOption;
        public bool ReturnOption => OptionGroup.ReturnOption;
        public double? Time => OverriddenTime.HasValue ? OverriddenTime : Parent.Time; // this time or parent's time
        public double? OverriddenTime;
        public string Check;
        public bool IsPreCheck;

        public Dialogue GetDialogue() => Parent.GetDialogue();
        public int SourceLineStart {get; internal set;}
        public int SourceLineEnd {get; internal set;}

        public OptionGroup OptionGroup {get; private set; }
        
        public IDialogueBlock Block => OptionGroup;
        public IChoosableNode Parent { get; private set; }
        IMultiCaseNode ICase.Parent => (ChoiceNode)Parent;

        public Option(OptionGroup group, int id, int idWithinGroup, int sourceLineStart, int sourceLineEnd, ChoiceNode parent, string text, IBoolValue condition, TagData tagData, double? time, string check, bool isPreCheck)
        {
            Id = id;
            OptionGroup = group;
            IdWithinGroup = idWithinGroup;
            Parent = parent;
            Text = text;
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
            return OptionGroup.GetAvailableOption(context) == this;
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

        public string PrintPayload()
        {
            string s;
            
            bool isFirstOptionInGroup = OptionGroup.IsFirstOption(this);

            if (isFirstOptionInGroup)
            {
                if (ReturnOption)
                    s = MuteOption ? "<-- " : "<- "; 
                else
                    s = MuteOption ? "-- " : "- "; 
            }
            else
            {
                s = "| ";
            }

            var attributes = GetAttributesString();

            if (!string.IsNullOrEmpty(attributes))
                s += "[" + GetAttributesString() + "] ";

            s += Text.Replace("\n", "↵\n").Replace("#", "##");

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
            bool isFirstOptionInGroup = OptionGroup.IsFirstOption(this);

            var line = Parent.LookUpTabsPrefix(indentationUnit) + indentationUnit;
            
            if (!isFirstOptionInGroup)
                line += indentationUnit;

            line += PrintPayload() + DialogueNode.GetTagsString(TagData);

            return line;
        }

        public override string ToString()
        {
            return PrintLine("\t");
        }
    }
}