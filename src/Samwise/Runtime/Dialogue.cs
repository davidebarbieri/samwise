// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

using System.Collections.Generic;
namespace Peevo.Samwise
{
    public class Dialogue : IDialogueBlock, ITaggable, ISource
    {
        public string Title;
        public string Label;
        public TagData TagData { get; set; }

        public NextBlockPolicy NextBlockPolicy => NextBlockPolicy.End;
        public IBlockContainerNode Parent => null;
        public int ChildrenCount => children.Count;
        public IDialogueNode GetChild(int i) => children[i];

        // Debug Information
        public Dialogue GetDialogue() => this;
        public int SourceLineStart {get; internal set; }
        public int SourceLineEnd {get; internal set; }

        // The unit used by the parser for indentation (tag or spaces)
        public string IndentationUnit { get => indentationUnit; set { indentationUnit = value; } }

        internal int AnonymousVariables {get; private set; }

        
        public void PushChild(IDialogueNode node)
        {
            node.Block = this;
            node.BlockId = children.Count;

            children.Add(node);
        }

        public void PopChild()
        {
            children[children.Count - 1].Block = null;
            children.RemoveAt(children.Count - 1);
        }

        public int GetNextIndex(int i)
        {
            ++i;

            if (i < ChildrenCount)
                return i;
            return -1;
        }

        public IEnumerable<KeyValuePair<string, IDialogueNode>> GetLabels()
        {
            return labeledNodes;
        }

        public IDialogueNode GetNodeFromLabel(string label)
        {
            if (labeledNodes.TryGetValue(label,  out var node))
                return node;
            return null;
        }

        public IDialogueNode FindNodeFromId(string id)
        {
            foreach (var node in this.Traverse())
                if (string.Equals(id, node.GetID()))
                    return node;

            return null;
        }

        public IDialogueNode FindNodeFromLine(int lineId)
        {
            foreach (var node in this.Traverse())
                if (lineId >= node.SourceLineStart && lineId <= node.SourceLineEnd)
                    return node;

            return null;
        }

        public Option FindOptionFromId(string id)
        {
            foreach (var node in this.Traverse<IChoosableNode>())
            {
                for (int i=0, count = node.OptionsCount; i<count; ++i)
                {
                    var option = node.GetOption(i);
                    if (string.Equals(id, option.GetID()))
                        return option;
                }
            }

            return null;
        }

        public Option FindOptionFromLine(int lineId)
        {
            foreach (var node in this.Traverse<IChoosableNode>())
            {
                for (int i=0, count = node.OptionsCount; i<count; ++i)
                {
                    var option = node.GetOption(i);
                    if (lineId >= option.SourceLineStart && lineId <= option.SourceLineEnd)
                        return option;
                }
            }

            return null;
        }

        internal bool OnLabeledNodeAdded(IDialogueNode node)
        {
            if (string.IsNullOrEmpty(node.Label))
                throw new System.NullReferenceException("Trying to register null label");

            if (labeledNodes.ContainsKey(node.Label))
                return false;
                
            labeledNodes[node.Label] = node;
            return true;
        }

        internal bool OnLabeledNodeReplaced(IDialogueNode prev, IDialogueNode next)
        {
            if (!labeledNodes.ContainsKey(prev.Label))
                throw new System.NullReferenceException("Trying to replace wrong label: " + prev.Label);
                
            next.Label = prev.Label;
            labeledNodes[prev.Label] = next;
            return true;
        }

        internal void GenerateAnonymousBoolVarName(string prefix, out string name, out string context)
        {
            context = "__anon." + Label + ".";
            name = "b" + prefix + "__" + (++AnonymousVariables);
        }

        internal void GenerateAnonymousIntVarName(string prefix, out string name, out string context)
        {
            context = "__anon." + Label + ".";
            name = "i" + prefix + "__" + (++AnonymousVariables);
        }

        public override string ToString()
        {
            string o;
            
            if (Label == Title.Replace(" ", ""))
                o = "§ " + Title;
            else
                o =  "§ (" + Label +") " + Title;

            if (TagData != null && TagData.HasData())
            {
                o += DialogueNode.GetTagsString(TagData);
            }

            o += "\n";

            for (int i=0; i<ChildrenCount; ++i)
                o += GetChild(i).PrintSubtree("", indentationUnit) + "\n";


            o += "\n";

            return o;
        }
        
        public string LookUpChildNodeTabsPrefix(IDialogueNode childNode, string indentationUnit)
        {
            // no indentation
            return "";
        }

        List<IDialogueNode> children = new List<IDialogueNode>();
        Dictionary<string, IDialogueNode> labeledNodes = new Dictionary<string, IDialogueNode>();
        string indentationUnit = "\t"; // default is tab
    }
}