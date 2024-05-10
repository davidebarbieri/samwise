// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

using System.Collections.Generic;

namespace Peevo.Samwise
{
    public class ScoreNode : DialogueNode, IAutoNode, IBlockContainerNode, IMultiCaseNode
    {

        public int ChildrenCount => children.Count;
        public ScoreCase GetChild(int i) => children[i];
        IDialogueBlock IBlockContainerNode.GetChild(int i) => children[i];
        public int CasesCount => children.Count;
        public ICase GetCase(int i) => children[i];

        public ScoreNode(int sourceLine) : base(sourceLine, sourceLine) {}

        public void AddCase(ScoreCase item)
        {
            children.Add(item);
        }

        public IDialogueNode Next(IDialogueSet dialogues, IDialogueContext context)
        {
            if (tmpRandomCases == null)
                tmpRandomCases = new List<ScoreCase>();
            else
                tmpRandomCases.Clear();

            int maxRandomValue = 0;
            long maxScore = long.MinValue;

            for (int i = 0; i < ChildrenCount; ++i)
            {
                var ccase = GetChild(i);

                if (ccase.Condition != null)
                    if (!ccase.Condition.EvaluateBool(context))
                        continue;

                // Check maximum score between available cases
                if (ccase.Score == null)
                    maxScore = System.Math.Max(maxScore, 0); // default score is 0
                else
                    maxScore = System.Math.Max(maxScore, ccase.Score.EvaluateInteger(context));

                tmpRandomCases.Add(ccase);
            }

            // No available possibilities
            if (tmpRandomCases.Count == 0)
                return this.FindNextSibling();

            // select highest score
            for (int i = 0; i < tmpRandomCases.Count; ++i)
            {
                var ccase = tmpRandomCases[i];

                long score = 0;
                if (ccase.Score != null)
                    score = ccase.Score.EvaluateInteger(context);

                // remove lower score cases
                if (score != maxScore)
                {
                    tmpRandomCases.RemoveAt(i--);
                }
                else
                    maxRandomValue += ccase.ProbabilityFactor;
            }

            var rnd = context.GetRandom() % maxRandomValue;

            for (int i = 0; i < tmpRandomCases.Count; ++i)
            {
                var rndCase = tmpRandomCases[i];
                if (rnd < rndCase.ProbabilityFactor)
                {
                    tmpRandomCases.Clear();

                    // random case selected
                    rndCase.Condition?.OnVisited(context);

                    if (rndCase.ChildrenCount > 0)
                        return rndCase.GetChild(0);
                    return this.FindNextSibling();
                }
                else
                    rnd -= rndCase.ProbabilityFactor;
            }

            // actually unreachable
            tmpRandomCases.Clear();
            return this.FindNextSibling();
        }

        public override string PrintSubtree(string indentationPrefix, string indentationUnit)
        {
            string o = GetPreambleString(indentationPrefix) + PrintPayload() + GetTagsString() + "\n";

            for (int i = 0; i < children.Count; ++i)
                o += children[i].PrintSubtree(indentationUnit + indentationPrefix, indentationUnit) + (i == children.Count - 1 ? "" : "\n");

            return o;
        }

        public override string PrintPayload()
        {
            return "%";
        }
       
        public string LookUpChildBlockTabsPrefix(IDialogueBlock childBlock, string indentationUnit)
        {
            string prefix = indentationUnit + indentationUnit; // tab + "-" + tab
            
            var parentPrefix = Block?.LookUpChildNodeTabsPrefix(this, indentationUnit) ?? null;

            if (parentPrefix != null)
                prefix = parentPrefix + prefix;

            return prefix;
        }

        List<ScoreCase> children = new List<ScoreCase>();

        [System.ThreadStatic]
        static List<ScoreCase> tmpRandomCases;
    }
}