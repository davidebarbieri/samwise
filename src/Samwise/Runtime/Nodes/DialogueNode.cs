
// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

using System.Collections.Generic;

namespace Peevo.Samwise
{
    public abstract class DialogueNode : IDialogueNode
    {
        public string Label { get; set; }
        public IBoolValue Condition { get; set; }
        public TagData TagData { get; set; }
        public string PreCheck { get; set; }
        public IDialogueBlock Block { get; set; }
        public int BlockId { get; set; }
        public int SourceLineStart {get; internal set; }
        public int SourceLineEnd {get; internal set; }
        public InterruptibleNode InnermostInterruptibleNode { get; set; }

        public DialogueNode(int sourceLineStart, int sourceLineEnd) { SourceLineStart = sourceLineStart; SourceLineEnd = sourceLineEnd; }

        public Dialogue GetDialogue() 
        {
            var block = Block;

            while (block != null && block.Parent != null)
                block = block.Parent.Block;

            return block as Dialogue;
        }

        public string GetPreambleString(string prefix, bool skipsTrailingSpace = false) 
        {
            if (skipsTrailingSpace)
            {
                if (!string.IsNullOrEmpty(Label)) 
                    prefix += "(" + Label + ")";

                var attributes = GetAttributesString();

                if (!string.IsNullOrEmpty(attributes)) 
                {
                    if (!string.IsNullOrEmpty(Label)) 
                        prefix += " [" + attributes + "]";
                    else
                        prefix += "[" + attributes + "]";
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(Label)) 
                    prefix += "(" + Label + ") ";

                var attributes = GetAttributesString();

                if (!string.IsNullOrEmpty(attributes)) 
                    prefix += "[" + attributes + "] ";
            }

            return prefix;
        }

        public string GetTagsString()
        {
            return GetTagsString(TagData);
        }

        public static string GetTagsString(TagData tagsData)
        {
            if (tagsData == null || !tagsData.HasData())
                return "";

            string tagsString = " # ";

            bool firstTag = true;
            if (tagsData.Tags != null)
                foreach (var tag in tagsData.Tags)
                {
                    if (!firstTag)
                        tagsString += ", ";

                    firstTag = false;

                    tagsString += tag;
                }
                
            if (tagsData.Comments != null)
                foreach (var comment in tagsData.Comments)
                {
                    if (!firstTag)
                        tagsString += ", ";

                    firstTag = false;

                    tagsString += "\"" + comment + "\"";
                }

            // Named tags
            foreach (var tag in tagsData.GetNamedTags())
            {
                if (!firstTag)
                    tagsString += ", ";

                firstTag = false;

                tagsString += tag.Item1 + "=" + tag.Item2;
            }

            // Named comments
            foreach (var comment in tagsData.GetNamedComments())
            {
                if (!firstTag)
                    tagsString += ", ";

                firstTag = false;

                tagsString += comment.Item1 + "=" + "\"" + comment.Item2 + "\"";
            }

            return tagsString;
        }

        public virtual string GetAttributesString()
        {
            string attributes = "";

            if (Condition != null)
                attributes = Condition.ToString();

            if (PreCheck != null)
            {
                if (string.IsNullOrEmpty(PreCheck))
                {
                    attributes = string.IsNullOrEmpty(attributes) ? "precheck" : attributes + ", precheck";
                }
                else
                {
                    var checkString = "precheck " + PreCheck;
                    attributes = string.IsNullOrEmpty(attributes) ? checkString : attributes + ", " + checkString;
                }
            }

            return attributes;
        }

        public virtual string PrintSubtree(string indentationPrefix, string indentationUnit) => GetPreambleString(indentationPrefix) + PrintPayload() + GetTagsString();

        // Get the tab prefix for the child block
        public string LookUpTabsPrefix(string indentationUnit) { if (Block == null) return ""; else return Block.LookUpChildNodeTabsPrefix(this, indentationUnit); }

        public string PrintLine(string indentationUnit) 
        {
            string preamble;
            if (Block == null) 
                preamble = GetPreambleString("");
            else
            {
                ConditionNode conditionBlock = Block as ConditionNode;
                // The inline condition node is the only virtual node that lies on the same line (and inherits the preamble)
                if (conditionBlock != null && conditionBlock.Inline && conditionBlock.GetChild(0) == this)
                    preamble = conditionBlock.GetPreambleString(LookUpTabsPrefix(indentationUnit)); 
                else
                    preamble = GetPreambleString(LookUpTabsPrefix(indentationUnit)); 
            } 
            
            return preamble + PrintPayload() + GetTagsString();
        }

        public abstract string PrintPayload();

        public virtual string GenerateUidPreamble(Dialogue dialogue)
        {
            return dialogue.Label + "_" ;
        }

        public override string ToString()
        {
            return PrintLine("\t");
        }
    }
}