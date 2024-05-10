// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public partial class SamwiseParser
    {
        bool ParseCodeBlock(string text, ref int position, ref int line, out IDialogueNode node, ref TagData tagData, Dialogue dialogue)
        {
            node = null;
            tagData = null;

            bool isExternalCode = false;

            if (!TokenUtils.ParseToken(text, ref position, "{"))
                return false;

            if (TokenUtils.ParseToken(text, ref position, "{"))
                isExternalCode = true;

            int startCode = position;
            int startLine = line;
            bool endedLine = false;

            if (isExternalCode)
            {
                if (!ReadExternalCodeBlock(text, ref position, line, out string codeBlock))
                    return false;

                if (externalCodeParser.Parse(codeBlock, out var statement, (e) => PushError(startLine, e)))
                {
                    node = new ExternalCodeNode(startLine, statement);
                }
                else
                {
                    PushError(line, "Unable to parse code");
                    return false;
                }
            }
            else
            {
                while (!TokenUtils.ParseToken(text, ref position, "}") && !(endedLine = TokenUtils.ParseEndline(text, ref position, ref line)))
                    ++position;

                if (endedLine)
                {
                    PushError(line, "Expected '}'");
                    return false;
                }

                var codeText = text.Substring(startCode, position - startCode - 1);

                int innerPosition = 0;
                if (TokenUtils.ParseToken(codeText, ref innerPosition, "wait"))
                {
                    if (TokenUtils.ParseTime(codeText, ref innerPosition, out var time))
                    {
                        node = new WaitTimeNode(startLine, time);
                    }
                    else
                    {
                        int tempLine = line;
                        string expressionString = TokenUtils.ReadBeforeCharacter(codeText, ref innerPosition, ref tempLine, '}');
                        var value = ParseExpression(dialogue, expressionString, ref tempLine);

                        if (value is IBoolValue boolExpr)
                        {
                            node = new WaitExpressionNode(startLine, boolExpr);
                        }
                        else
                        {
                            PushError(line, "Wait node must have a time or boolean expression");
                            return false;
                        }
                    }
                }
                else if (ParseCode(codeText, line, out var statement, dialogue))
                {
                    node = new CodeNode(startLine, statement);
                }
                else
                {
                    PushError(line, "Unable to parse code");
                    return false;
                }
            }

            if (!ParseEndNodeLine(text, ref position, ref line, ref tagData))
            {
                PushError(line, "Expected end line");
                return false;
            }

            return true;
        }

        bool ParseAsyncCode(string text, ref int position, int line, out IAsyncCode asyncCode)
        {
            asyncCode = null;
            if (!ReadExternalCodeBlock(text, ref position, line, out string codeBlock))
                return false;

            if (externalCodeParser.ParseAsync(codeBlock, out asyncCode, (e) => PushError(line, e)))
            {
                return true;
            }
            else
            {
                PushError(line, "Unable to parse code");
                return false;
            }
        }

        bool ReadExternalCodeBlock(string text, ref int position, int line, out string codeBlock)
        {
            var startCode = position;
            codeBlock = "";
            bool endedLine = false;
            if (externalCodeParser == null)
            {
                PushError(line, "External code found but no external parser was configured.");
                return false;
            }

            int scopeDepth = 2;

            int endBlockPos = 2;
            while (!(endedLine = TokenUtils.ParseEndline(text, ref position, ref line)))
            {
                if (text[position] == '{')
                    ++scopeDepth;
                else if (text[position] == '}')
                {
                    if (scopeDepth == 2)
                        endBlockPos = position;

                    --scopeDepth;
                }

                ++position;

                if (scopeDepth == 0)
                    break;
            }

            if (endedLine)
            {
                PushError(line, "Expected '}}'");
                return false;
            }

            codeBlock = text.Substring(startCode, endBlockPos - startCode);
            return true;
        }

        bool ParseCode(string text, int line, out IStatement statement, Dialogue dialogue)
        {
            statement = null;

            int position = 0;

            TokenUtils.SkipWhitespaces(text, ref position);

            if (position >= text.Length)
                return false;

            string varName = "";
            string varContext = "";

            if (TokenUtils.ParseVariableName(text, ref position, line, out varName, out varContext, out var hasShortcutName, Errors))
            {
                varContext = TokenUtils.MakeAbsoluteContext(varContext, hasShortcutName, dialogue.Label);

                if (TokenUtils.ParseToken(text, ref position, "+="))
                {
                    switch (varName[0])
                    {
                        case 'i':
                            statement = new IncrementAssignmentStatement();
                            break;
                        default:
                            PushError(line, "Unsupported variable");
                            return false;
                    }
                }
                else if (TokenUtils.ParseToken(text, ref position, "-="))
                {
                    switch (varName[0])
                    {
                        case 'i':
                            statement = new DecrementAssignmentStatement();
                            break;
                        default:
                            PushError(line, "Unsupported variable");
                            return false;
                    }
                }
                else if (TokenUtils.ParseToken(text, ref position, "="))
                {
                    switch (varName[0])
                    {
                        case 'i':
                            statement = new IntegerAssignmentStatement();
                            break;
                        case 'b':
                            statement = new BoolAssignmentStatement();
                            break;
                        case 's':
                            statement = new SymbolAssignmentStatement();
                            break;
                        default:
                            PushError(line, "Unsupported variable");
                            return false;
                    }
                }
                else
                    return false;

                int tempLine = line;
                string expressionString = TokenUtils.ReadTillEndLine(text, ref position, ref tempLine);

                var value = ParseExpression(dialogue, expressionString, ref line);

                if (value is IBoolValue && statement is BoolAssignmentStatement)
                {
                    ((BoolAssignmentStatement)statement).Name = varName;
                    ((BoolAssignmentStatement)statement).Context = varContext;
                    ((BoolAssignmentStatement)statement).Value = (IBoolValue)value;
                }
                else if (value is IIntegerValue && statement is IntegerAssignmentStatement)
                {
                    ((IntegerAssignmentStatement)statement).Name = varName;
                    ((IntegerAssignmentStatement)statement).Context = varContext;
                    ((IntegerAssignmentStatement)statement).Value = (IIntegerValue)value;
                }
                else if (value is ISymbolValue && statement is SymbolAssignmentStatement)
                {
                    ((SymbolAssignmentStatement)statement).Name = varName;
                    ((SymbolAssignmentStatement)statement).Context = varContext;
                    ((SymbolAssignmentStatement)statement).Value = (ISymbolValue)value;
                }
                else
                    return false;

                return true;
            }
            else
                return false; // not recognized

            
        }

        bool ParseIntegerVariable(string text, ref int position, int line, out string variableContext, out string variableName, Dialogue dialogue)
        {
            variableName = null;
            variableContext = null;

            TokenUtils.SkipWhitespaces(text, ref position);

            if (position >= text.Length)
                return false;
        
            if (!TokenUtils.ParseVariableName(text, ref position, line, out variableName, out variableContext, out var hasShortcutName, Errors))
                return false;

            variableContext = TokenUtils.MakeAbsoluteContext(variableContext, hasShortcutName, dialogue.Label);

            return variableName[0] == 'i';
        }

        bool ParseBoolVariable(string text, ref int position, int line, out string variableContext, out string variableName, Dialogue dialogue)
        {
            variableName = null;
            variableContext = null;

            TokenUtils.SkipWhitespaces(text, ref position);

            if (position >= text.Length)
                return false;
        
            if (!TokenUtils.ParseVariableName(text, ref position, line, out variableName, out variableContext, out var hasShortcutName, Errors))
                return false;

            variableContext = TokenUtils.MakeAbsoluteContext(variableContext, hasShortcutName, dialogue.Label);

            return variableName[0] == 'b';
        }
    }
}