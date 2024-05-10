// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

using System.Collections.Generic;

namespace Peevo.Samwise
{
    public class ChoiceNode : DialogueNode, IChoosableNode, IBlockContainerNode, IMultiCaseNode
    {
        public string CharacterId { get; private set; }

        public int OptionsCount => options.Count;
        public Option GetOption(int i) => options[i];
        public int CasesCount => options.Count;
        public ICase GetCase(int i) => options[i];
        public double? Time { get; set; }

        public int ChildrenCount => options.Count;
        public IDialogueBlock GetChild(int i) => options[i];

        public void AddOption(Option option)
        {
            options.Add(option);
        }

        public ChoiceNode(int sourceLine, string characterId) : base(sourceLine, sourceLine)
        {
            CharacterId = characterId;
        }

        public IDialogueNode Next(Option option, IDialogueContext context)
        {
            // Skip option
            if (option == null)
                return this.FindNextSibling();

            option.Condition?.OnVisited(context);

            if (option.ChildrenCount > 0)
                return option.GetChild(0);
            return this.FindNextSibling();
        }

        public override string PrintSubtree(string indentationPrefix, string indentationUnit)
        {
            string o = GetPreambleString(indentationPrefix) + PrintPayload() + GetTagsString() + "\n";

            for (int i=0; i<options.Count; ++i)
                o += options[i].PrintSubtree(indentationUnit + indentationPrefix, indentationUnit) + (i == options.Count - 1 ? "" : "\n");

            return o;
        }
        
        public override string PrintPayload()
        {
            return CharacterId + ":";
        }

        public string LookUpChildBlockTabsPrefix(IDialogueBlock childBlock, string indentationUnit)
        {
            string prefix = indentationUnit + indentationUnit; // tab + "-" + tab
            
            var parentPrefix = Block?.LookUpChildNodeTabsPrefix(this, indentationUnit) ?? null;

            if (parentPrefix != null)
                prefix = parentPrefix + prefix;

            return prefix;
        }

        public override string GetAttributesString()
        {
            var attributes = base.GetAttributesString();

            if (Time.HasValue)
            {
                if (!string.IsNullOrEmpty(attributes))
                    attributes += ", ";

                attributes += Time.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) + "s";
            }

            return attributes;
        }

        List<Option> options = new List<Option>();
    }
}