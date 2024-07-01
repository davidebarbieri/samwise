// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace Peevo.Samwise
{
    public partial class SamwiseParser
    {
        public bool ReleaseMode { get; private set; }
        public readonly List<ParseError> Errors = new List<ParseError>();

        public SamwiseParser(IExternalCodeParser externalCodeParser = null, bool releaseMode = false)
        {
            ReleaseMode = releaseMode;
            this.externalCodeParser = externalCodeParser;

            InitializeExpressionParser();
        }

        public bool Parse(List<Dialogue> output, string text, bool clearErrors = true, bool removeDuplicates = false)
        {
            interruptibleSections.Clear();

            if (clearErrors)
                Errors.Clear();

            // Try to skip utf BOM and 0-width space
            text = text.Trim(bomCharacters);

            if (text.Length == 0)
            {
                PushError(0, "Empty input text.");
                return false;
            }

            if (text[0] == '\uFFFD')
            {
                PushError(0, "Invalid character (\uFFFD) found. This mostly means the input file is not in UTF8 format.");
                return false;
            }

            int position = 0;
            int line = 1;
            while (position < text.Length)
            {
                if (!ParseDialogue(text, ref position, ref line, out var dialogue))
                    return false;

                if (removeDuplicates)
                    output.RemoveAll((d) => d.Label == dialogue.Label);

                output.Add(dialogue);
            }

            return true;
        }


        public bool ParseDialogue(string text, ref int position, ref int line, out Dialogue dialogue)
        {
            dialogue = new Dialogue();

            int depth = 0;

            TokenUtils.SkipWhiteLines(text, ref position, ref line);

            dialogue.SourceLineStart = line;

            bool res = ParseTitle(text, ref position, ref line, dialogue);

            res = res && ParseBlock(text, ref position, ref line, ref depth, dialogue, dialogue);

            dialogue.SourceLineEnd = System.Math.Max(dialogue.SourceLineStart, line - 1);

            bool hasResolveErrors = false;
            if (res)
            {
                // Resolve GOTOs
                for (int i = 0; i < unresolvedNodes.Count; ++i)
                {
                    var node = dialogue.GetNodeFromLabel(unresolvedNodes[i].Item2);

                    if (node != null)
                        unresolvedNodes[i].Item1.Resolve(node);
                    else
                    {
                        hasResolveErrors = true;
                        PushError(unresolvedNodes[i].Item3, "Can't resolve goto node. Undefined label: " + unresolvedNodes[i].Item2);
                    }
                }
                unresolvedNodes.Clear();
            }

            dialogue.IndentationUnit = inferredIndentation == null ? "\t" : inferredIndentation;
            
            return res && !hasResolveErrors;
        }

        bool ParseTitle(string text, ref int position, ref int line, Dialogue dialogue)
        {
            dialogue.Title = "";
            dialogue.Label = "";
            dialogue.TagData = null;

            if (position > text.Length)
                return false;

            TagData tempTagData = null;

            // Top / in-line  
            bool res = false;
            
            bool moreAdornment;

            do 
            {
                moreAdornment = ParseTitleAdornment(text, ref position);
                res = res || moreAdornment;

                if (moreAdornment)
                {
                    ParseTagBlock(text, ref position, ref line, ref tempTagData);
                }

                moreAdornment = moreAdornment && TokenUtils.ParseEndline(text, ref position, ref line);
            }
            while (moreAdornment);

            // Optional Symbol
            var hasLabel = ParseLabel(text, ref position, ref line, out dialogue.Label, true);

            if (Errors.Count > 0)
                return false;

            // Parse Title / Name (until adornment or tags)
            if (TokenUtils.ReadLineUntil(text, ref position, out dialogue.Title, (c) => TokenUtils.IsTitleSign(c) || c == '#'))
                dialogue.Title = dialogue.Title.Trim();
            else
                dialogue.Title = "";

            if (string.IsNullOrEmpty(dialogue.Title) && !hasLabel)
            {
                PushError(line, "Title must have a name or a label");
                return false;
            }

            if (!hasLabel)
                dialogue.Label = dialogue.Title.Replace(" ", ""); // Remove spaces from title

            if (char.IsLower(dialogue.Label[0]))
            {
                PushError(line, "Title labels must start with an upper case character");
                return false;
            }

            // optional in-line adornment + following adornments
            ParseTitleAdornment(text, ref position);
            ParseTagBlock(text, ref position, ref line, ref tempTagData);

            TokenUtils.ParseEndline(text, ref position, ref line);

            do 
            {
                moreAdornment = ParseTitleAdornment(text, ref position);
                if (moreAdornment)
                {
                    ParseTagBlock(text, ref position, ref line, ref tempTagData);
                }
                    
                moreAdornment = moreAdornment && TokenUtils.ParseEndline(text, ref position, ref line);
            }
            while (moreAdornment);

            if (tempTagData != null && tempTagData.HasData()) dialogue.TagData = tempTagData;

            if (!res) PushError(line, "Can't parse dialogue title");

            return res;
        }

        bool ParseTitleAdornment(string text, ref int position)
        {
            TokenUtils.SkipWhitespaces(text, ref position);

            if (position >= text.Length)
                return false;

            bool atLeastOne = false;
            while (position < text.Length && TokenUtils.IsTitleSign(text[position]))
            {
                ++position;
                atLeastOne = true;
            }

            return atLeastOne;
        }

        bool ParseBlock(string text, ref int position, ref int line, ref int depth, IDialogueBlock nodes, Dialogue dialogue)
        {
            while (position < text.Length)
            {
                TokenUtils.SkipWhiteLines(text, ref position, ref line);
                if (position >= text.Length)
                    break;   
                
                if (TokenUtils.IsTitleSign(text[position]))
                    break;

                int newPosition = position;
                int newLine = line;
                if (!CheckErrors(line, TokenUtils.ParseLineStart(text, ref newPosition, ref newLine, out var newDepth, ref inferredIndentation)))
                    return false;

                if (newDepth < depth)
                    return true; // block ended

                if (newDepth > depth)
                {
                    // if there's a depth increase (by 1) and the previous node in the block had
                    // a condition, then that was a condition node
                    if (newDepth == depth + 1 && nodes.ChildrenCount > 0 && nodes.GetChild(nodes.ChildrenCount - 1).IsBranching())
                    {
                        if (!GetOrPromoteLastConditionNode(dialogue, nodes, out ConditionNode conditionNode))
                        {
                            PushError(line, "Unable to promote condition node");
                            return false;
                        }
                        
                        // Add this block to condition node
                        if (!ParseBlock(text, ref position, ref line, ref newDepth, conditionNode, dialogue))
                            return false;

                        //conditionNode.SourceLineEnd = line;
                        continue;
                    }

                    PushError(line, "Unexpected indentation");
                    return false;
                }

                position = newPosition;
                line = newLine;

                bool res = ParseNode(text, ref position, ref line, ref depth, dialogue, nodes, out var node);
                if (!res)
                    return false;
            }

            return true;
        }

        bool GetOrPromoteLastConditionNode(Dialogue dialogue, IDialogueBlock block, out ConditionNode conditionNode)
        {
            if (block.ChildrenCount == 0)
            {
                conditionNode = null;
                return false;
            }

            var prevNode = block.GetChild(block.ChildrenCount - 1);

            conditionNode = prevNode as ConditionNode;

            if (conditionNode == null)
            {
                // If previous is not a condition node, then promote it to inline condition node
                conditionNode = new ConditionNode(prevNode.SourceLineStart, prevNode.SourceLineEnd, true);

                conditionNode.Condition = prevNode.Condition;
                conditionNode.PreCheck = prevNode.PreCheck;
                conditionNode.Label = prevNode.Label;

                if (!string.IsNullOrEmpty(prevNode.Label))
                    dialogue.OnLabeledNodeReplaced(prevNode, conditionNode);

                // reset label and condition
                prevNode.Condition = null;
                prevNode.PreCheck = null;
                prevNode.Label = null;

                block.PopChild();
                block.PushChild(conditionNode);

                conditionNode.PushChild(prevNode);
            }

            conditionNode = conditionNode.GetLastElseCondition();

            return true;
        }

        bool ParseNodeLabel(string text, ref int position, ref int line, out string nodeName)
        {
            return ParseLabel(text, ref position, ref line, out nodeName, false);
        }

        bool ParseLabel(string text, ref int position, ref int line, out string nodeName, bool isTitle)
        {
            if (TokenUtils.ParseToken(text, ref position, "("))
            {
                var res = isTitle ? TokenUtils.ParseTitleLabel(text, ref position, ref line, out nodeName) : TokenUtils.ParseLabel(text, ref position, ref line, out nodeName);

                if (res == ErrorCode.NoFirstUpperCase)
                {
                    PushError(line, "Title labels must start with upper case");
                    return false;
                }

                if (res == ErrorCode.NoFirstLowerCase)
                {
                    PushError(line, "Node labels must start with lower case");
                    return false;
                }
                
                if (res != ErrorCode.Success || string.IsNullOrEmpty(nodeName))
                {
                    PushError(line, "Unable to read label");
                    return false;
                }

                if (!TokenUtils.ParseToken(text, ref position, ")"))
                {
                    PushError(line, "Unable to read label");
                    return false;
                }

                return true;
            }
            nodeName = "";
            return false;
        }

        bool ParseNode(string text, ref int position, ref int line, ref int depth, Dialogue dialogue, IDialogueBlock block, out IDialogueNode node)
        {
            TagData tagData = null;

            node = null;
            // Try to read name
            ParseNodeLabel(text, ref position, ref line, out var nodeName);

            var attributesLine = line;
            // Try to parse condition
            ParseAttributesBlock(dialogue, text, ref position, ref line, out var condition, out var score, out var multiplier, out var time, out var check, out var precheck, true, out var elseCondition);

            if (Errors.Count > 0)
                return false;

            int nodeLine = line;

            // Try to read condition
            if (TokenUtils.ParseToken(text, ref position, "*"))
            {
                // Action Node
                var captionText = TokenUtils.ReadFreeString(text, ref position, ref line);
                node = new CaptionNode(nodeLine, line, captionText);

                if (!ParseEndNodeLine(text, ref position, ref line, ref tagData))
                {
                    PushError(line, "Expected end line,");
                    return false;
                }
            }
            else if (TokenUtils.ParseToken(text, ref position, "->"))
            {
                // Goto Node
                if (!TokenUtils.ParseName(text, ref position, out var destination))
                {
                    PushError(line, "Arrow without destination,");
                    return false;
                }

                if (destination.Equals("end"))
                {
                    node = new ExitNode(nodeLine);
                }
                else if (destination.Contains("."))
                {
                    var dotPos = destination.LastIndexOf('.');

                    if (dotPos == 0 || dotPos == destination.Length - 1)
                    {
                        PushError(line, "Invalid target");
                        return false;
                    }

                    node = new GotoNode(nodeLine, destination.Substring(0, dotPos), destination.Substring(dotPos + 1));
                }
                else if (char.IsUpper(destination[0]))
                {
                    node = new GotoNode(nodeLine, destination, "");
                }
                else
                {
                    node = new LocalGotoNode(nodeLine, destination, null);
                    unresolvedNodes.Add(((LocalGotoNode)node, destination, line));
                }

                if (!ParseEndNodeLine(text, ref position, ref line, ref tagData))
                {
                    PushError(line, "Expected end line,");
                    return false;
                }
            }
            else if (ParseCodeBlock(text, ref position, ref line, out node, ref tagData, dialogue))
            {
                // 
            }
            else
            {
                Symbol symbol = Symbol.Invalid;
                bool hasName = TokenUtils.ParseName(text, ref position, out var name);
                bool hasSymbol = TokenUtils.ParseSymbol(text, ref position, ref line, out symbol);

                if (!hasName && !hasSymbol)
                {
                    // If it has a condition but no payload, then it's a condition node
                    if (condition != null || elseCondition)
                    {
                        if (!ParseEndNodeLine(text, ref position, ref line, ref tagData))
                        {
                            PushError(line, "Expected end line");
                            return false;
                        }

                        var conditionNode = new ConditionNode(nodeLine, nodeLine, false);
                        
                        int innerDepth = depth + 1;
                        if (!ParseBlock(text, ref position, ref line, ref innerDepth, conditionNode, dialogue))
                        {
                            PushError(line, "A condition node must have a sub-tree");
                            return false;
                        }

                        node = conditionNode;
                    }
                    else 
                    {
                        if (block.ChildrenCount > 0 )
                        {
                            var lastNode = block.GetChild(block.ChildrenCount - 1);
                            
                            switch (lastNode)
                            {
                                case InterruptibleNode interruptileNode:
                                {
                                    if (TokenUtils.ParseToken(text, ref position, "+", true) ||
                                        TokenUtils.ParseToken(text, ref position, "-", true))
                                    {
                                        PushError(line, "Indentation error");
                                        return false;
                                    }
                                    break;
                                }
                                case IBlockContainerNode multiNode:
                                {
                                    if (TokenUtils.ParseToken(text, ref position, "-", true))
                                    {
                                        PushError(line, "Indentation error");
                                        return false;
                                    }
                                    break;
                                }
                            }
                        }

                        PushError(line, "Can't parse node");
                        return false;
                    }
                }
                else switch (symbol)
                {
                    case Symbol.Fallback:
                    case Symbol.Loop:
                    case Symbol.Clamp:
                    case Symbol.PingPong:
                        {
                            string counterVarContext = null;
                            string counterVarName = null;
                            bool usesAnonymousCounter = false;

                            if (symbol == Symbol.Loop || symbol == Symbol.Clamp || symbol == Symbol.PingPong)
                            {
                                // Parse variable
                                if (!ParseIntegerVariable(text, ref position, line, out counterVarContext, out counterVarName, dialogue))
                                {
                                    //PushError(line, "Expected integer counter variable");
                                    //return false;
                                        
                                    // generate anonymous counter variable
                                    dialogue.GenerateAnonymousIntVarName("Counter", out counterVarName, out counterVarContext);
                                    usesAnonymousCounter = true;
                                }
                            }

                            if (!ParseEndNodeLine(text, ref position, ref line, ref tagData))
                            {
                                PushError(line, "Expected end line");
                                return false;
                            }

                            TokenUtils.SkipWhiteLines(text, ref position, ref line);

                            SelectionNode selectionNode = null;

                            if (symbol == Symbol.Fallback)
                                selectionNode = new FallbackNode(nodeLine);
                            else if (symbol == Symbol.Loop)
                                selectionNode = new LoopNode(nodeLine, counterVarContext, counterVarName, usesAnonymousCounter);
                            else if (symbol == Symbol.PingPong)
                                selectionNode = new PingPongNode(nodeLine, counterVarContext, counterVarName, usesAnonymousCounter);
                            else
                                selectionNode = new ClampNode(nodeLine, counterVarContext, counterVarName, usesAnonymousCounter);

                            node = selectionNode;

                            int newDepth;
                            int newPos = position;
                            int newLine = line;
                            if (!CheckErrors(line, TokenUtils.ParseLineStart(text, ref newPos, ref newLine, out newDepth, ref inferredIndentation)))
                                return false;

                            while (newDepth == depth + 1)
                            {
                                position = newPos;
                                line = newLine;

                                if (TokenUtils.ParseToken(text, ref position, "-"))
                                {
                                    ParseAttributesBlock(dialogue, text, ref position, ref line, out var childCondition, out var childScore, out var childMultiplier, out var childTime, out var childCheck, out var childPreCheck, false, out _);

                                    if (Errors.Count > 0)
                                        return false;

                                    int childLine = line;
                                    TagData childTagData = null;
                                    if (!ParseEndNodeLine(text, ref position, ref line, ref childTagData))
                                    {
                                        PushError(line, "Expected end line");
                                        return false;
                                    }

                                    if (!ExpectAttributes(line, childMultiplier, childTime, childCheck, childPreCheck, childScore, false, false, false, false, false))
                                        return false;

                                    SelectionCase item = new SelectionCase(childLine, selectionNode, childCondition, childTagData);
                                    selectionNode.AddCase(item);

                                    int innerDepth = newDepth + 1;
                                    ParseBlock(text, ref position, ref line, ref innerDepth, item, dialogue);
                                    newPos = position;
                                    newLine = line;
                                }
                                else
                                {
                                    PushError(line, "Unable to parse selection case");
                                    return false;
                                }

                                if (!CheckErrors(newLine, TokenUtils.ParseLineStart(text, ref newPos, ref newLine, out newDepth, ref inferredIndentation)))
                                    return false;
                            }

                            if (selectionNode.CasesCount == 0)
                            {
                                PushError(selectionNode.SourceLineStart, "No cases found in selection node");
                                return false;
                            }
                            
                            break;
                        }
                    case Symbol.Score:
                        {
                            if (!ParseEndNodeLine(text, ref position, ref line, ref tagData))
                            {
                                PushError(line, "Expected end line");
                                return false;
                            }
                            
                            TokenUtils.SkipWhiteLines(text, ref position, ref line);

                            var randomNode = new ScoreNode(nodeLine);
                            node = randomNode;

                            int newDepth;
                            int newPos = position;
                            int newLine = line;
                            if (!CheckErrors(line, TokenUtils.ParseLineStart(text, ref newPos, ref newLine, out newDepth, ref inferredIndentation)))
                                return false;

                            while (newDepth == depth + 1)
                            {
                                position = newPos;
                                line = newLine;

                                if (TokenUtils.ParseToken(text, ref position, "-"))
                                {
                                    ParseAttributesBlock(dialogue, text, ref position, ref line, out var childCondition, out var childScore, out var childMultiplier, out var childTime, out var childCheck, out var childPreCheck, false, out _);

                                    if (Errors.Count > 0)
                                        return false;

                                    int childLine = line;
                                    TagData childTagData = null;
                                    if (!ParseEndNodeLine(text, ref position, ref line, ref childTagData))
                                    {
                                        PushError(line, "Expected end line");
                                        return false;
                                    }

                                    if (!ExpectAttributes(line, childMultiplier, childTime, childCheck, childPreCheck, childScore, true, false, false, false, true))
                                        return false;

                                    ScoreCase item = new ScoreCase(childLine, randomNode, childCondition, childScore, childMultiplier.HasValue ? childMultiplier.Value : 1, childTagData);
                                    randomNode.AddCase(item);

                                    int innerDepth = newDepth + 1;
                                    ParseBlock(text, ref position, ref line, ref innerDepth, item, dialogue);
                                    newPos = position;
                                    newLine = line;
                                }
                                else
                                {
                                    PushError(line, "Unable to parse random case");
                                    return false;
                                }

                                if (!CheckErrors(newLine, TokenUtils.ParseLineStart(text, ref newPos, ref newLine, out newDepth, ref inferredIndentation)))
                                    return false;
                            }
                            break;
                        }
                    case Symbol.Fork:
                    case Symbol.Await:
                        {
                            if (TokenUtils.ParseToken(text, ref position, "{{"))
                            {
                                if (!ParseAsyncCode(text, ref position, line, out IAsyncCode asyncCode))
                                    return false;

                                if (symbol == Symbol.Fork)
                                    node = new ExternalForkNode(nodeLine, name, asyncCode);
                                else
                                    node = new ExternalAwaitNode(nodeLine, asyncCode);
                            }
                            else
                            {
                                // Fork Node
                                if (!TokenUtils.ParseName(text, ref position, out var destination))
                                {
                                    if (symbol == Symbol.Fork)
                                    {
                                        var anonNode = new AnonymousForkNode(nodeLine, name);
                                        node = anonNode;

                                        if (!ParseEndNodeLine(text, ref position, ref line, ref tagData))
                                        {
                                            PushError(line, "Expected end line,");
                                            return false;
                                        }
                                
                                        int innerDepth = depth + 1;
                                        bool res = ParseBlock(text, ref position, ref line, ref innerDepth, anonNode.AnonymousBlock, dialogue);

                                        if (!res || anonNode.AnonymousBlock.ChildrenCount == 0)
                                        {
                                            PushError(line, "An anonymous fork must have a valid body");
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        PushError(line, "Expected destination");
                                        return false;
                                    }
                                }
                                else
                                {
                                    // Single-line forks

                                    if (destination.Equals("end"))
                                    {
                                        PushError(line, "Cannot use 'end' as destination to fork or await");
                                        return false;
                                    }
                                    else if (destination.Contains("."))
                                    {
                                        var dotPos = destination.LastIndexOf('.');

                                        if (dotPos == 0 || dotPos == destination.Length - 1)
                                        {
                                            PushError(line, "Invalid target");
                                            return false;
                                        }

                                        if (symbol == Symbol.Fork)
                                            node = new ForkNode(nodeLine, name, destination.Substring(0, dotPos), destination.Substring(dotPos + 1));
                                        else
                                            node = new AwaitNode(nodeLine, destination.Substring(0, dotPos), destination.Substring(dotPos + 1));
                                    }
                                    else if (char.IsUpper(destination[0]))
                                    {
                                        if (symbol == Symbol.Fork)
                                            node = new ForkNode(nodeLine, name, destination, "");
                                        else
                                            node = new AwaitNode(nodeLine, destination, "");
                                    }
                                    else
                                    {
                                        if (symbol == Symbol.Fork)
                                            node = new LocalForkNode(nodeLine, name, destination, null);
                                        else
                                            node = new LocalAwaitNode(nodeLine, destination, null);

                                        unresolvedNodes.Add(((IResolvableNode)node, destination, nodeLine));
                                    }

                                    if (!ParseEndNodeLine(text, ref position, ref line, ref tagData))
                                    {
                                        PushError(line, "Expected end line,");
                                        return false;
                                    }
                                }
                            }
                            break;
                        }
                    case Symbol.Join:
                        {
                            node = new JoinNode(nodeLine, name);

                            if (!ParseEndNodeLine(text, ref position, ref line, ref tagData))
                            {
                                PushError(line, "Expected end line,");
                                return false;
                            }
                            break;
                        }
                    case Symbol.Cancel:
                        {
                            node = new CancelNode(nodeLine, name);

                            if (!ParseEndNodeLine(text, ref position, ref line, ref tagData))
                            {
                                PushError(line, "Expected end line,");
                                return false;
                            }
                            break;
                        }
                    case Symbol.Choice:
                        {
                            // Choice Node
                            if (!ParseEndNodeLine(text, ref position, ref line, ref tagData))
                            {
                                PushError(line, "Expected end line");
                                return false;
                            }
                            
                            TokenUtils.SkipWhiteLines(text, ref position, ref line);

                            var choiceNode = new ChoiceNode(nodeLine, name);
                            choiceNode.Time = time;
                            node = choiceNode;

                            int newDepth;
                            int newPos = position;
                            int newLine = line;
                            if (!CheckErrors(line, TokenUtils.ParseLineStart(text, ref newPos, ref newLine, out newDepth, ref inferredIndentation)))
                                return false;

                            int optionId = 0;
                            while (newDepth == depth + 1)
                            {
                                position = newPos;
                                line = newLine;

                                bool mute = false;

                                bool returnOption = TokenUtils.ParseToken(text, ref position, "<");

                                if ((TokenUtils.ParseToken(text, ref position, "--") && (mute = true)) ||
                                    TokenUtils.ParseToken(text, ref position, "-"))
                                {
                                    ParseAttributesBlock(dialogue, text, ref position, ref line, out var childCondition, out var childScore, out var childMultiplier, out var childTime, out var childCheck, out var childPreCheck, false, out _);

                                    if (Errors.Count > 0)
                                        return false;
                                        
                                    if (!ExpectAttributes(line, childMultiplier, childTime, childCheck, childPreCheck, childScore, false, true, true, true, false))
                                        return false;

                                    int optionLineStart = line;
                                    var optionText = TokenUtils.ReadFreeString(text, ref position, ref line);
                                    int optionLineEnd = line;

                                    TagData childTagData = null;
                                    if (!ParseEndNodeLine(text, ref position, ref line, ref childTagData))
                                    {
                                        PushError(line, "Expected end line");
                                        return false;
                                    }

                                    Option option = new Option(optionLineStart, optionLineEnd, optionId++, choiceNode, optionText, mute, returnOption, childCondition, childTagData, childTime, childCheck ?? childPreCheck, childPreCheck != null);
                                    choiceNode.AddOption(option);

                                    int innerDepth = newDepth + 1;

                                    // Try to parse alternatives
                                    while (ParseAlternative(dialogue, option, text, ref position, ref line, innerDepth, out OptionAlternative alternative))
                                    {
                                        option.AddAlternative(alternative);
                                    }
                                    
                                    if (Errors.Count > 0)
                                        return false;

                                    bool res = ParseBlock(text, ref position, ref line, ref innerDepth, option, dialogue);

                                    if (!res)
                                        return false;

                                    newPos = position;
                                    newLine = line;
                                }
                                else
                                {
                                    PushError(line, "Expected \"-\" or \"--\"");
                                    return false;
                                }

                                if (!CheckErrors(newLine, TokenUtils.ParseLineStart(text, ref newPos, ref newLine, out newDepth, ref inferredIndentation)))
                                    return false;

                                if (newDepth > depth + 1)
                                {
                                    PushError(line, "Unexpected indentation");
                                    return false;
                                }
                            }

                            break;
                        }
                    case Symbol.Says:
                        {
                            var speechText = TokenUtils.ReadFreeString(text, ref position, ref line);
                            node = new SpeechNode(nodeLine, line, name, speechText);

                            if (!ParseEndNodeLine(text, ref position, ref line, ref tagData))
                            {
                                PushError(line, "Expected end line");
                                return false;
                            }
                            break;
                        }
                    case Symbol.ResetAndInterrupt:
                    case Symbol.Interrupt:
                    {
                        // Parse variable
                        if (!ParseBoolVariable(text, ref position, line, out var signalVarContext, out var signalVarName, dialogue))
                        {
                            PushError(line, "Expected bool signal variable");
                            return false;
                        }

                        if (string.IsNullOrEmpty(signalVarContext))
                        {
                            PushError(line, "Interrupt nodes supports global variables only");
                            return false;
                        }

                        if (!ParseEndNodeLine(text, ref position, ref line, ref tagData))
                        {
                            PushError(line, "Expected end line");
                            return false;
                        }
                        
                        TokenUtils.SkipWhiteLines(text, ref position, ref line);

                        var interruptNode = new InterruptibleNode(symbol == Symbol.ResetAndInterrupt, interruptibleSections.Count > 0 ? interruptibleSections.Peek() : null, nodeLine, signalVarContext, signalVarName);
                        
                        node = interruptNode; 

                        interruptibleSections.Push(interruptNode);

                        int innerDepth = depth + 1;
                        ParseBlock(text, ref position, ref line, ref innerDepth, interruptNode.InterruptibleBlock, dialogue);
                        interruptibleSections.Pop();
                        break;
                    }
                    case Symbol.Check:
                        {
                            if (!TokenUtils.ParseName(text, ref position, out var checkName))
                            {
                                PushError(line, "Missing check name");
                                return false;
                            }

                            if (!ParseEndNodeLine(text, ref position, ref line, ref tagData))
                            {
                                PushError(line, "Expected end line");
                                return false;
                            }
    
                            TokenUtils.SkipWhiteLines(text, ref position, ref line);
                           
                            var checkNode = new CheckNode(nodeLine, checkName);
                            node = checkNode;

                            int newDepth;
                            int newPos = position;
                            int newLine = line;
                            if (!CheckErrors(line, TokenUtils.ParseLineStart(text, ref newPos, ref newLine, out newDepth, ref inferredIndentation)))
                                return false;

                            while (newDepth == depth + 1)
                            {
                                position = newPos;
                                line = newLine;

                                if (TokenUtils.ParseToken(text, ref position, "-"))
                                {
                                    int childLine = line;
                                    TagData childTagData = null;
                                    if (!ParseEndNodeLine(text, ref position, ref line, ref childTagData))
                                    {
                                        PushError(line, "Expected end line");
                                        return false;
                                    }

                                    if (checkNode.FailBlock != null)
                                    {
                                        PushError(line, "Multiple fail cases found");
                                        return false;
                                    }

                                    checkNode.FailBlock = new CheckResultBlock(false, checkNode, childTagData);

                                    int innerDepth = newDepth + 1;

                                    ParseBlock(text, ref position, ref line, ref innerDepth, checkNode.FailBlock, dialogue);
                                    newPos = position;
                                    newLine = line;
                                }
                                else if (TokenUtils.ParseToken(text, ref position, "+"))
                                {
                                    int childLine = line;
                                    TagData childTagData = null;
                                    if (!ParseEndNodeLine(text, ref position, ref line, ref childTagData))
                                    {
                                        PushError(line, "Expected end line");
                                        return false;
                                    }

                                    if (checkNode.PassBlock != null)
                                    {
                                        PushError(line, "Multiple pass cases found");
                                        return false;
                                    }

                                    checkNode.PassBlock = new CheckResultBlock(true, checkNode, childTagData);

                                    int innerDepth = newDepth + 1;
                                    ParseBlock(text, ref position, ref line, ref innerDepth, checkNode.PassBlock, dialogue);
                                    newPos = position;
                                    newLine = line;
                                }
                                else
                                {
                                    PushError(line, "Unable to parse pass/fail cases");
                                    return false;
                                }

                                if (!CheckErrors(newLine, TokenUtils.ParseLineStart(text, ref newPos, ref newLine, out newDepth, ref inferredIndentation)))
                                    return false;
                            }
                            break;
                        }
                    default:
                        PushError(line, "Unrecognized token");
                        return false;
                }
            }

            if (!ExpectAttributes(attributesLine, multiplier, time, check, precheck, score, false, node is ChoiceNode, false, true, false))
                return false;

            var innermostInterruptibleNode = interruptibleSections.Count > 0 ? interruptibleSections.Peek() : null;

            // if elseCondition is true, then this is a conditional node 
            // create a container conditional node if needed
            if (elseCondition)
            {
                var embeddableNode = node;

                if (embeddableNode is ConditionNode)
                {
                    // already a condition node, no need to embedd it
                }
                else
                {
                    // embedd node into an inline condition node
                    node = new ConditionNode(node.SourceLineStart, node.SourceLineEnd, true);
                    ((ConditionNode)node).PushChild(embeddableNode);
                    embeddableNode.InnermostInterruptibleNode = innermostInterruptibleNode;
                }
            }

            node.Label = nodeName;
            node.Condition = condition;
            node.TagData = tagData;
            node.PreCheck = precheck;
            node.InnermostInterruptibleNode = innermostInterruptibleNode;

            if (!string.IsNullOrEmpty(nodeName))
            {
                if (!dialogue.OnLabeledNodeAdded(node))
                {
                    // reused label
                    PushError(line - 1, "Duplicate label " + node.Label);
                    return false;
                }
            }

            if (elseCondition)
            {
                // find previous conditional node or promote previous node to conditional
                if (!GetOrPromoteLastConditionNode(dialogue, block, out ConditionNode prevConditionNode)
                    || prevConditionNode.Condition == null) // second check also covers "else if/else" after simple "else"
                {
                    PushError(line, "else can be used only after another node with a condition");
                    return false;
                }

                var conditionNode = node as ConditionNode;
                prevConditionNode.SetElseCondition(conditionNode);
            }
            else
            {
                // add this node to the block
                block.PushChild(node);
            }
            
            return true;
        }

        bool ParseAlternative(Dialogue dialogue, Option option, string text, ref int position, ref int line, int innerDepth, out OptionAlternative alternative)
        {
            alternative = null;

            int newPosition = position;
            int newLine = line;
            if (!CheckErrors(line, TokenUtils.ParseLineStart(text, ref newPosition, ref newLine, out var newDepth, ref inferredIndentation)))
                return false;

            if (newDepth != innerDepth)
            {
                return false;
            }

            if (TokenUtils.ParseToken(text, ref newPosition, "|"))
            {
                position = newPosition;
                line = newLine;

                ParseAttributesBlock(dialogue, text, ref position, ref line, out var condition, out var score, out var multiplier, out var time, out var check, out var preCheck, false, out _);

                if (Errors.Count > 0)
                    return false;
                    
                if (!ExpectAttributes(line, multiplier, time, check, preCheck, score, false, false, false, false, false))
                {
                    if (time.HasValue)
                    {
                        PushError(line, "An option alternative cannot have an individual Time attribute, only the parent option can.");
                        return false;
                    }

                    if (check != null)
                    {
                        PushError(line, "An option alternative cannot have an individual Challenge Check, only the parent option can.");
                        return false;
                    }

                    return false;
                }

                int optionLineStart = line;
                var optionText = TokenUtils.ReadFreeString(text, ref position, ref line);
                int optionLineEnd = line;

                TagData tagData = null;
                if (!ParseEndNodeLine(text, ref position, ref line, ref tagData))
                {
                    PushError(line, "Expected end line");
                    return false;
                }

                alternative = new OptionAlternative(option, optionLineStart, optionLineEnd, optionText, condition, tagData);
                return true;
            }

            return false;
        }

        bool ExpectAttributes(int line, int? multiplier, double? time, string check, string precheck, IIntegerValue intExpr, bool expectMultiplier, bool expectTime, bool expectCheck, bool expectPreCheck, bool expectIntegerExpression)
        {
            if (!expectMultiplier && multiplier.HasValue)
            {
                PushError(line, "Unexpected multiplier");
                return false;
            }

            if (!expectTime && time.HasValue)
            {
                PushError(line, "Unexpected time attribute");
                return false;
            }

            if (!expectCheck && check != null)
            {
                PushError(line, "Unexpected check attribute");
                return false;
            }

            if (!expectPreCheck && precheck != null)
            {
                PushError(line, "Unexpected precheck attribute");
                return false;
            }

            if (check != null && precheck != null)
            {
                PushError(line, "Check and precheck can't be used together");
                return false;
            }

            if (!expectIntegerExpression && intExpr != null)
            {
                PushError(line, "Unexpected integer expression attribute");
                return false;
            }

            return true;
        }


        bool ParseAttributesBlock(Dialogue dialogue, string text, ref int position, ref int line,
            out IBoolValue condition, out IIntegerValue score, out int? multiplier, 
            out double? time, out string check, out string precheck, 
            bool supportsElseCondition, out bool elseCondition)
        {
            condition = null;
            score = null;
            multiplier = null;
            time = null;
            check = null;
            precheck = null;
            elseCondition = false;

            // Try to parse condition
            if (TokenUtils.ParseToken(text, ref position, "["))
            {
                if (TokenUtils.ParseToken(text, ref position, "]"))
                    return true;

                do
                {
                    try
                    {
                        if (TokenUtils.ParseMultiplier(text, ref position, out var parsedMultiplier))
                        {
                            if (multiplier.HasValue)
                            {
                                PushError(line, "multiple multiplier attributes found");
                                return false;
                            }

                            multiplier = parsedMultiplier;
                        }
                        else if (TokenUtils.ParseTime(text, ref position, out var parsedTime))
                        {
                            if (time.HasValue)
                            {
                                PushError(line, "multiple time attributes found");
                                return false;
                            }

                            time = parsedTime;
                        }
                        else if (TokenUtils.ParseNamedAttributes("check", text, ref position, out var parsedName))
                        {
                            if (check != null || precheck != null)
                            {
                                PushError(line, "multiple check attributes found");
                                return false;
                            }

                            check = parsedName;
                        } 
                        else if (TokenUtils.ParseNamedAttributes("precheck", text, ref position, out var parsedPreName))
                        {
                            if (check != null || precheck != null)
                            {
                                PushError(line, "multiple check attributes found");
                                return false;
                            }

                            precheck = parsedPreName;
                        }
                        else
                        {
                            // check if else attribute
                            bool foundElseCondition = TokenUtils.ParseToken(text, ref position, "else", true);

                            if (foundElseCondition)
                            {
                                if (!supportsElseCondition)
                                {
                                    PushError(line, "multiple 'else' attributes found");
                                    return false;
                                }

                                if (elseCondition)
                                {
                                    PushError(line, "multiple 'else' attributes found");
                                    return false;
                                }
                            }

                            elseCondition = foundElseCondition;

                            // Condition
                            if (!ParseNodeExpression(dialogue, text, ref position, ref line, out var foundExpression))
                            {
                                if (elseCondition)
                                    continue; // else alone is legit

                                PushError(line, "Cannot parse expression");
                                return false;
                            }

                            if (foundExpression is IBoolValue boolFoundExpression)
                            {
                                if (condition != null)
                                {
                                    // Multiple condition attributes found, put them in and
                                    condition = new AndValue(condition, boolFoundExpression);
                                }
                                else
                                    condition = boolFoundExpression;
                            }
                            else if (foundExpression is IIntegerValue intFoundExpression)
                            {
                                if (score != null)
                                {
                                    PushError(line, "Multiple score expressions found");
                                    return false;
                                }

                                score = intFoundExpression;
                            }
                            else
                            {
                                PushError(line, "Unsupported expression type");
                                return false;
                            }

                        }
                    } 
                    catch (Exception e)
                    {
                        PushError(line, "Error: " + e.Message);
                        return false;
                    }
                } while (TokenUtils.ParseToken(text, ref position, ","));

                if (!TokenUtils.ParseToken(text, ref position, "]"))
                {
                    PushError(line, "Cannot parse attributes block");
                    return false;
                }

                return true;
            }

            condition = null;
            return false;
        }

        bool ParseEndNodeLine(string text, ref int position, ref int line, ref TagData tagData)
        {
            if (!ParseTagBlock(text, ref position, ref line, ref tagData))
            {
                if (Errors.Count > 0)
                    return false;
            }

            if (!TokenUtils.ParseEndline(text, ref position, ref line))
                return false;

            return true;
        }

        bool ParseTagBlock(string text, ref int position, ref int line, ref TagData tagData)
        {
            // Try to parse condition
            if (TokenUtils.ParseToken(text, ref position, "#"))
            {
                if (TokenUtils.PeekEndline(text, position))
                    return true;

                do
                {
                    if (TokenUtils.ParseDoubleQuotedString(text, ref position, line, out var stringValue, out var error))
                    {
                        if (tagData == null)
                            tagData = new TagData();

                        tagData.AddComment(stringValue);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(error))
                            PushError(line, error);

                        // Tag
                        if (TokenUtils.ParseName(text, ref position, out var tag))
                        {
                            tag = tag.ToLowerInvariant();

                            // Check if named data
                            if (TokenUtils.ParseToken(text, ref position, "="))
                            {
                                if (TokenUtils.ParseDoubleQuotedString(text, ref position, line, out stringValue, out error))
                                {
                                    if (tag.Equals("id"))
                                    {
                                        PushError(line, "cannot use the name 'id' for comments");
                                        return false;
                                    }

                                    if (tagData == null)
                                        tagData = new TagData();

                                    tagData.SetNamedComment(tag, stringValue);
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(error))
                                        PushError(line, error);

                                    if (TokenUtils.ParseName(text, ref position, out var value))
                                    {
                                        if (tagData == null)
                                            tagData = new TagData();
                                        
                                        tagData.SetNamedTag(tag, value);
                                    }
                                    else
                                    {
                                        PushError(line, "Cannot parse named tag");
                                        return false;
                                    }
                                }
                            }
                            else
                            {
                                if (tagData == null)
                                    tagData = new TagData();
                                
                                tagData.AddTag(tag);
                            }
                        }
                        else
                        {
                            PushError(line, "Cannot parse tag");
                            return false;
                        }
                    }
                }
                while (TokenUtils.ParseToken(text, ref position, ","));

                if (!TokenUtils.PeekEndline(text, position))
                {
                    PushError(line, "Cannot parse tag block");
                    return false;
                }

                return true;
            }

            return false;
        }

        bool CheckErrors(int line, ErrorCode code)
        {
            switch (code)
            {
                case ErrorCode.Success:
                    return true;
                case ErrorCode.BadIndentation:
                    PushError(line, "Bad indentation. Please make sure the use of tabs or spaces is consistent throughout the file");
                    return false;
                case ErrorCode.NoFirstUpperCase:
                    PushError(line, "First character must be upper case");
                    return false;
                case ErrorCode.NoFirstLowerCase:
                    PushError(line, "First character must be lower case");
                    return false;
                case ErrorCode.EmptyLabel:
                    PushError(line, "Label is empty");
                    return false;

            }

            return false;
        }
        
        void PushError(int line, string message) {Errors.Add(new ParseError() {Error = message, Line = line});}

        string inferredIndentation;
        Stack<InterruptibleNode> interruptibleSections = new Stack<InterruptibleNode>();
        List<(IResolvableNode, string, int)> unresolvedNodes = new List<(IResolvableNode, string, int)>();
        IExternalCodeParser externalCodeParser;

        static readonly char[] bomCharacters = new char[] { '\uFEFF', '\u200B' };
    }
}