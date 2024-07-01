using System;
using System.Collections.Generic;

namespace Peevo.Samwise.Wasm
{
    public static class IDialogueBlockUtils
    {
        public static IEnumerable<ITextContent> FindTextContent(this IDialogueBlock block, bool includeMuteContent)
        {
            for (int i = 0, count = block.ChildrenCount; i < count; ++i)
            {
                var node = block.GetChild(i);

                if (node is ITextContent)
                    yield return (ITextContent)node;

                if (node is IChoosableNode)
                {
                    var choosableNode = (IChoosableNode)node;

                    for (int ii = 0, icount = choosableNode.OptionsCount; ii < icount; ++ii)
                    {
                        var option = choosableNode.GetOption(ii);
                        if (includeMuteContent || !option.MuteOption)
                        {
                            yield return option.DefaultContent;

                            if (option.AlternativeContents != null)
                                foreach (var content in option.AlternativeContents)
                                    yield return content;
                        }
                    }
                }

                var blockContainer = node as IBlockContainerNode;

                if (blockContainer != null)
                {
                    for (int ii = 0, icount = blockContainer.ChildrenCount; ii < icount; ++ii)
                    {
                        foreach (var subNode in FindTextContent(blockContainer.GetChild(ii), includeMuteContent))
                            yield return subNode;
                    }
                }
            }
        }
        public static IEnumerable<IContent> FindContent(this IDialogueBlock block)
        {
            for (int i = 0, count = block.ChildrenCount; i < count; ++i)
            {
                var node = block.GetChild(i);

                yield return node;

                if (node is IMultiCaseNode multiNode)
                {
                    for (int ii = 0, icount = multiNode.CasesCount; ii < icount; ++ii)
                    {
                        var option = multiNode.GetCase(ii);

                        for (int iii=0; iii<option.ContentCount; ++iii)
                            yield return option.GetContent(iii);
                    }
                }


                var blockContainer = node as IBlockContainerNode;

                if (blockContainer != null)
                {
                    for (int ii = 0, icount = blockContainer.ChildrenCount; ii < icount; ++ii)
                    {
                        foreach (var subNode in FindContent(blockContainer.GetChild(ii)))
                            yield return subNode;
                    }
                }
            }
        }

        public static IEnumerable<(IContent, List<IStatefulElement>)> FindAnonymousContent(this IDialogueBlock block)
        {
            for (int i = 0, count = block.ChildrenCount; i < count; ++i)
            {
                var node = block.GetChild(i);

                List<IStatefulElement> elements = null;

                if (node is IStatefulElement stateful && stateful.UsesAnonymousVariable)
                {
                    if (elements == null)
                        elements = new List<IStatefulElement>();

                    elements.Add(stateful);
                }

                if (node.Condition != null)
                {
                    // Check if condition has anonymous content
                    node.Condition.Traverse((a) =>
                    {
                        if (a is IStatefulElement statefulValue && statefulValue.UsesAnonymousVariable)
                        {
                            if (elements == null)
                                elements = new List<IStatefulElement>();

                            elements.Add(statefulValue);
                        }
                    });
                }

                if (elements != null)
                    yield return (node, elements);

                // Check cases too
                if (node is IMultiCaseNode multiNode)
                {
                    for (int ii = 0, icount = multiNode.CasesCount; ii < icount; ++ii)
                    {
                        var option = multiNode.GetCase(ii);

                        for (int iii=0; iii<option.ContentCount; ++iii)
                        {
                            var content = option.GetContent(iii);
                            var condition = content.Condition;

                            if (condition != null)
                            {
                                elements = null;

                                // Check if case condition has anonymous content
                                condition.Traverse((a) =>
                                {
                                    if (a is IStatefulElement statefulValue && statefulValue.UsesAnonymousVariable)
                                    {
                                        if (elements == null)
                                            elements = new List<IStatefulElement>();

                                        elements.Add(statefulValue);
                                    }
                                });

                                if (elements != null)
                                    yield return (content, elements);
                            } 
                        }
                    }
                }

                var blockContainer = node as IBlockContainerNode;

                if (blockContainer != null)
                {
                    for (int ii = 0, icount = blockContainer.ChildrenCount; ii < icount; ++ii)
                    {
                        foreach (var subNode in FindAnonymousContent(blockContainer.GetChild(ii)))
                            yield return subNode;
                    }
                }
            }
        }

        public static void GatherUniqueIDs(this IDialogueBlock block, HashSet<string> uniqueIDs)
        {
            foreach (var content in block.FindTextContent(true))
            {
                var id = content.GetID();

                if (id != null)
                    uniqueIDs.Add(id);
            }
        }

        public static void GatherStatefulVariables(this IDialogueBlock block, HashSet<string> uniqueVariables)
        {
            for (int i = 0, count = block.ChildrenCount; i < count; ++i)
            {
                var node = block.GetChild(i);

                if (node is IStatefulElement stateful)
                {
                    uniqueVariables.Add(stateful.StateVariableContext + stateful.StateVariableName);
                }

                if (node.Condition != null)
                {
                    // Check if condition has anonymous content
                    node.Condition.Traverse((a) =>
                    {
                        if (a is IStatefulElement statefulValue && statefulValue.UsesAnonymousVariable)
                        {
                            uniqueVariables.Add(statefulValue.StateVariableContext + statefulValue.StateVariableName);
                        }
                    });
                }

                // Check cases too
                if (node is IMultiCaseNode multiNode)
                {
                    for (int ii = 0, icount = multiNode.CasesCount; ii < icount; ++ii)
                    {
                        var option = multiNode.GetCase(ii);

                        for (int iii=0; iii<option.ContentCount; ++iii)
                        {
                            var content = option.GetContent(iii);
                            var condition = content.Condition;

                            if (condition != null)
                            {
                                // Check if case condition has anonymous content
                                condition.Traverse((a) =>
                                {
                                    if (a is IStatefulElement statefulValue && statefulValue.UsesAnonymousVariable)
                                    {
                                        uniqueVariables.Add(statefulValue.StateVariableContext + statefulValue.StateVariableName);
                                    }
                                });
                            }
                        }
                    }
                }

                var blockContainer = node as IBlockContainerNode;

                if (blockContainer != null)
                {
                    for (int ii = 0, icount = blockContainer.ChildrenCount; ii < icount; ++ii)
                    {
                        GatherStatefulVariables(blockContainer.GetChild(ii), uniqueVariables);
                    }
                }
            }
        }
    }
}