// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

using System.Collections.Generic;

namespace Peevo.Samwise
{
    public class ChoiceNode : DialogueNode, IChoosableNode, IBlockContainerNode, IMultiCaseNode
    {
        public string CharacterId { get; private set; }

        public int OptionsGroupsCount => optionsGroups.Count;
        public int OptionsCount 
        {
            get
            {
                int optionsCount = 0;
                
                for (int i=0,count=optionsGroups.Count; i<count; ++i) 
                    optionsCount += optionsGroups[i].OptionsCount;
                
                return optionsCount;
            }
        } 

        public int CasesCount => optionsGroups.Count;
        public ICase GetCase(int index) => GetOption(index);

        public OptionGroup GetOptionGroup(int groupId) => optionsGroups.Count >groupId ? optionsGroups[groupId] : null;
        public Option GetOption(int optionId) 
        {
            for (int i=0,count=optionsGroups.Count; i<count; ++i) 
            {
                var group = optionsGroups[i];

                if (group.OptionsCount > optionId)
                    return group.GetOption(optionId);

                optionId -= group.OptionsCount;
            }

            return null;
        }

        public double? Time { get; set; }

        public int ChildrenCount => optionsGroups.Count;
        public IDialogueBlock GetChild(int i) => optionsGroups[i];

        public void AddOptionGroup(OptionGroup optionGroup)
        {
            optionsGroups.Add(optionGroup);
        }

        public ChoiceNode(int sourceLine, string characterId) : base(sourceLine, sourceLine)
        {
            CharacterId = characterId;
        }
        
        public IEnumerator<IOption> GetAvailableOptions(IDialogueContext context)
        {
            foreach (var group in optionsGroups)
            {
                var availableGroup = group.GetAvailableOption(context);

                if (availableGroup != null)
                    yield return availableGroup;
            }
        }

        public IDialogueNode Next(IOption option, IDialogueContext context)
        {
            // Skip option
            if (option == null)
                return this.FindNextSibling();

            var block = option.Block;
            if (block.ChildrenCount > 0)
                return block.GetChild(0);
            return this.FindNextSibling();
        }

        public override string PrintSubtree(string indentationPrefix, string indentationUnit)
        {
            string o = GetPreambleString(indentationPrefix) + PrintPayload() + GetTagsString() + "\n";

            for (int i=0; i<optionsGroups.Count; ++i)
                o += optionsGroups[i].PrintSubtree(indentationUnit + indentationPrefix, indentationUnit) + (i == optionsGroups.Count - 1 ? "" : "\n");

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

        List<OptionGroup> optionsGroups = new List<OptionGroup>();
    }
}