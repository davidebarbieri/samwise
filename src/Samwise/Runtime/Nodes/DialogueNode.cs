
// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public abstract class DialogueNode : IDialogueNode
    {
        public string Label { get; set; }
        public IBoolValue Condition { get; set; }
        public ITagData TagData { get; set; }
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

        public bool HasCheck(out bool isPreCheck, out string checkName)
        {
            isPreCheck = true;
            checkName = PreCheck;
            return PreCheck != null;
        }

        public string GetPreambleString(string prefix, bool skipsTrailingSpace = false) 
        {
            if (Block is ConditionNode conditionNode && conditionNode.Inline && BlockId == 0 )
                return conditionNode.GetPreambleString(prefix, skipsTrailingSpace); // inherits from parent inline condition node

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

        public static string GetTagsString(ITagData tagsData)
        {
            if (tagsData == null || !tagsData.HasTags())
                return "";

            string tagsString = " # ";

            bool firstTag = true;
                
            // Named tags
            foreach (var tag in tagsData.GetTags())
            {
                if (!firstTag)
                    tagsString += ", ";

                firstTag = false;

                if (tag.Value == null)
                {
                    bool hasNonNameCharacters = false;
                    for (int i=0,count=tag.Key.Length;i<count;i++)
                    {
                        if (!TokenUtils.IsNameChar(tag.Key[i]))
                        {
                            hasNonNameCharacters = true;
                            break;
                        }
                    }

                    if (hasNonNameCharacters)
                        tagsString += "\"" + tag.Key + "\"";
                    else
                        tagsString += tag.Key;
                }
                else
                {
                    bool hasNonNameCharacters = false;
                    for (int i=0,count=tag.Value.Length;i<count;i++)
                    {
                        if (!TokenUtils.IsNameChar(tag.Value[i]))
                        {
                            hasNonNameCharacters = true;
                            break;
                        }
                    }

                    tagsString += tag.Key + "=" + (hasNonNameCharacters ? ("\"" + tag.Value + "\"") :  tag.Value);
                }
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