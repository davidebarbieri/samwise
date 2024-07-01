// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

using System.Collections.Generic;

namespace Peevo.Samwise
{
    public class Option : SequenceBlock, ITextContent, ICase, IOption
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public IBoolValue Condition { get; set; }
        public TagData TagData { get; set; }
        public bool MuteOption { get; set; }
        public bool ReturnOption { get; set; }
        public double? Time => OverriddenTime.HasValue ? OverriddenTime : Parent.Time; // this time or parent's time
        public double? OverriddenTime;
        public string Check;
        public bool IsPreCheck;

        public ITextContent DefaultContent => this;
        public IReadOnlyList<ITextContent> AlternativeContents => alternatives;

        public Dialogue GetDialogue() => Parent.GetDialogue();
        public int SourceLineStart { get; internal set; }
        public int SourceLineEnd { get; internal set; }

        public override NextBlockPolicy NextBlockPolicy => ReturnOption ? NextBlockPolicy.ReturnToParent : NextBlockPolicy.ParentNext;
        public new ChoiceNode Parent => (ChoiceNode)base.Parent;
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

        public bool IsAvailable(IDialogueContext context, out ITextContent textContent)
        {
            textContent = this;

            if (Condition == null)
                return true;

            if (Condition.EvaluateBool(context))
                return true;

            textContent = GetAvailableAlternative(context);
            if (textContent != null)
                return true;

            return false;
        }

        public ITextContent GetTextContent(IDialogueContext context)
        {
            if (Condition == null)
                return this;

            if (Condition.EvaluateBool(context))
                return this;

            return GetAvailableAlternative(context);
        }

        public bool HasCheck(out bool isPreCheck, out string checkName)
        {
            isPreCheck = IsPreCheck;
            if (Check == null)
            {
                checkName = null;
                return false;
            }

            checkName = Check;
            return true;
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
            
            var availableAlternative = GetAvailableAlternative(context);
            availableAlternative?.Condition?.OnVisited(context);
        }

        public void AddAlternative(OptionAlternative alternative)
        {
            if (alternatives == null)
                alternatives = new List<OptionAlternative>();
                
            alternatives.Add(alternative);
        }

        public void RemoveAlternative(OptionAlternative alternative)
        {
            alternatives?.Remove(alternative);
        }

        public override string PrintSubtree(string indentationPrefix, string indentationUnit)
        {
            var s = indentationPrefix + PrintPayload() + DialogueNode.GetTagsString(TagData);

            if (alternatives != null)
            {
                for (int i=0, count = alternatives.Count; i<count; ++i)
                {
                    var alternative = alternatives[i];
                    s += "\n" + indentationPrefix + indentationUnit + alternative.PrintPayload() + DialogueNode.GetTagsString(alternative.TagData);
                }
            }
            
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

            s += Text.Replace("\n", "↵\n");

            return s;
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

        ITextContent GetAvailableAlternative(IDialogueContext context)
        {
            if (alternatives == null)
                return null;
            
            for (int i=0, count = alternatives.Count; i<count; ++i)
            {
                var alternative = alternatives[i];

                if (alternative.Condition == null || alternative.Condition.EvaluateBool(context))
                {
                    return alternative;
                }
            }

            return null;
        }

        List<OptionAlternative> alternatives;
    }
}