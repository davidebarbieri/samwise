// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

using System.Collections.Generic;

namespace Peevo.Samwise
{
    public partial class SamwiseParser
    {
        bool ParseNodeExpression(Dialogue dialogue, string text, ref int position, ref int line, out IValue expression)
        {
            expression = null;

            int thisLine = line;
            int thisPosition = position;

            if (TokenUtils.ReadTillNextAttributes(text, thisLine, ref thisPosition, out var conditionString))
            {
                expression = ParseExpression(dialogue, conditionString, ref thisLine, true);

                if (expression != null)
                {
                    line = thisLine;
                    position = thisPosition;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        bool ParseConditionToken(string text, ref int position, ref int line, out ConditionToken token, bool supportOnce)
        {
            TokenUtils.SkipWhitespaces(text, ref position);

            token = new ConditionToken() { position = position, length = 1 };

            if (position < text.Length)
            {
                var prevPos = position;

                switch (text[position++])
                {
                    case '&':
                        token.id = ConditionTokenId.And;
                        return true;
                    case '|':
                        token.id = ConditionTokenId.Or;
                        return true;
                    case '!':
                        if (position < text.Length && text[position] == '=')
                        {
                            token.id = ConditionTokenId.Different;
                            token.length = 2;
                            ++position;
                            return true;
                        }

                        token.id = ConditionTokenId.Not;
                        return true;
                    case '(':
                        token.id = ConditionTokenId.RoundOpen;
                        return true;
                    case ')':
                        token.id = ConditionTokenId.RoundClose;
                        return true;
                    case '+':
                        token.id = ConditionTokenId.Add;
                        return true;
                    case '-':
                        token.id = ConditionTokenId.Sub;
                        return true;
                    case '*':
                        token.id = ConditionTokenId.Mul;
                        return true;
                    case '/':
                        token.id = ConditionTokenId.Div;
                        return true;
                    case '{':
                        if (TokenUtils.ParseToken(text, ref position, "{", true))
                        {
                            if (!ReadExternalCodeBlock(text, ref position, line, out var codeBlock))
                            {
                                PushError(line, "Unable to parse external code condition");
                                return false;
                            }
                            token.id = ConditionTokenId.External;
                            token.length = codeBlock.Length;
                            return true;
                        }
                        else
                        {
                            PushError(line, "External code conditions must start with '{{'");
                            return false;
                        }
                    case '>':
                        if (position < text.Length && text[position] == '=')
                        {
                            token.id = ConditionTokenId.GEqual;
                            token.length = 2;
                            ++position;
                            return true;
                        }

                        token.id = ConditionTokenId.Greater;
                        return true;
                    case '<':
                        if (position < text.Length && text[position] == '=')
                        {
                            token.id = ConditionTokenId.LEqual;
                            token.length = 2;
                            ++position;
                            return true;
                        }

                        token.id = ConditionTokenId.Less;
                        return true;
                    case '=':
                        if (position < text.Length && text[position] == '=')
                        {
                            token.id = ConditionTokenId.Equal;
                            token.length = 2;
                            ++position;
                            return true;
                        }

                        PushError(line, "Unrecognized token '='");
                        return false;
                    default:
                        --position;
                        int startPosition = position;

                        if (TokenUtils.ParseToken(text, ref position, "once"))
                        {
                            if (!supportOnce)
                            {
                                PushError(line, "Once is supported only in node conditions and not within code nodes");
                                return false;
                            }

                            if (TokenUtils.ParseToken(text, ref position, "("))
                            {
                                int contextPos = position;
                                if (TokenUtils.ParseVariableNameToken(text, ref position, out var length) && TokenUtils.ParseToken(text, ref position, ")"))
                                {
                                    token.id = ConditionTokenId.Once;
                                    token.position = contextPos;
                                    token.length = length;
                                    return true;
                                }
                                else
                                {
                                    PushError(line, "Cannot parse once attribute");
                                    return false;
                                }
                            }
                            else
                            {
                                if (ReleaseMode)
                                {
                                    PushError(line, "Missing variable name in once expression: this isn't allowed for a release build. Please add a variable manually or using the automated method.");
                                    return false;
                                }
                                else
                                {
                                    token.id = ConditionTokenId.Once;
                                    token.position = position;
                                    token.length = 0;
                                    return true;
                                }
                            }
                        }
                        else if (TokenUtils.ParseToken(text, ref position, "true"))
                        {
                            token.id = ConditionTokenId.True;
                            token.length = 4;
                            return true;
                        }
                        else if (TokenUtils.ParseToken(text, ref position, "false"))
                        {
                            token.id = ConditionTokenId.False;
                            token.length = 5;
                            return true;
                        }
                        else if (TokenUtils.ParseLong(text, ref position, ref line, out var longValue))
                        {
                            token.id = ConditionTokenId.IntegerConst;
                            token.length = position - prevPos;
                            return true;
                        }
                        else if (TokenUtils.ParseVariableName(text, ref position, line, out var varName, out var contextName, out var hasShortcutName, Errors))
                        {
                            token.length = position - startPosition;

                            // variable
                            switch (varName[0])
                            {    
                            case 'i':
                                token.id = ConditionTokenId.IntegerVar;
                                return true;
                            case 'b':
                                token.id = ConditionTokenId.BoolVar;
                                return true;
                            case 's':
                                token.id = ConditionTokenId.SymbolVar;
                                return true;
                            }  
                        }
                        else if (TokenUtils.ParseSymbolString(text, ref position, out var symbolString))
                        {
                            token.length = position - startPosition;
                            token.id = ConditionTokenId.SymbolConst;
                            return true;
                        }

                        PushError(line, "Unrecognized token");
                        return false;
                }
            }

            PushError(line, "Unrecognized token");
            return false;
        }


        IValue ParseExpression(Dialogue dialogue, string text, ref int line, bool supportOnce = false)
        {
            int position = 0;
            waitingValue = true;
            values.Clear();
            operators.Clear();

            while (position < text.Length)
            {
                if (!ParseConditionToken(text, ref position, ref line, out var token, supportOnce))
                    return null;

                bool res = waitingValue ? ParseValue(dialogue, token, text, line) : ParseOperator(token, line);

                if (!res)
                    return null;
            }

            if (waitingValue)
                return null;

            while (operators.Count > 0)
            {
                var op = operators.Peek();
                if (!op.evaluator(line))
                {
                    PushError(line, "Can't evaluate operation");
                    return null;
                }
            }

            if (values.Count != 1)
                return null; // 'Internal error: value left on stack'

            return values.Pop();
        }

        bool UnaryEvaluator(int line)
        {
            var op = operators.Pop().allocator();
            var val = values.Pop();

            if (op is IUnaryOperationBoolValue && val is IBoolValue)
                ((IUnaryOperationBoolValue)op).A = (IBoolValue)val;
            else if (op is IUnaryOperationIntegerValue && val is IIntegerValue)
                ((IUnaryOperationIntegerValue)op).A = (IIntegerValue)val;
            else
            {
                PushError(line, "Operand mismatch");
                return false;
            }

            values.Push(op);
            return true;
        }

        bool BinaryEvaluator(int line)
        {
            var op = (IBinaryOperationValue)operators.Pop().allocator();
            var val2 = values.Pop();
            var val1 = values.Pop();

            op = op.Overload(val1, val2);

            if (op == null)
            {
                PushError(line, "Unsupported operands");
                return false;
            }

            values.Push(op);
            return true;
        }

        bool OnPrefixCloseParenthesis(int line)
        {
            var op1 = operators.Pop();

            PushError(line, "Empty parenthesis");
            return false; // Empty parenthesis
        }

        bool OnPrefixOpenParenthesis(int line)
        {
            // Unclosed open parenthesis
            PushError(line, "Unclosed parenthesis");
            return false;
        }

        bool OnPostfixCloseParenthesis(int line)
        {
            var op1 = operators.Pop();
            var op2 = operators.Pop();

            if (op2.token != ConditionTokenId.RoundOpen)
            {
                PushError(line, "Closing parenthesis without opening it first");
                return false;
            }

            var val1 = values.Pop();

            //values.Push(new ParenthesisValue(val1));
            values.Push(val1);

            return true;
        }

        void PushPriorityOperator(in ExpressionOperator op, int line)
        {
            if (op.leftPriority >= 0)
                while (operators.Count > 0 && operators.Peek().rightPriority >= op.leftPriority)
                    operators.Peek().evaluator(line);

            operators.Push(op);

            if (op.rightPriority < 0)
                operators.Peek().evaluator(line);
        }

        bool ParseValue(Dialogue dialogue, ConditionToken token, string text, int line)
        {
            switch (token.id)
            {
                case ConditionTokenId.IntegerConst:
                {
                    values.Push(new IntegerConstantValue(long.Parse(token.GetText(text))));
                    waitingValue = false;
                    break;
                }
                case ConditionTokenId.True:
                case ConditionTokenId.False:
                {
                    values.Push(new BoolConstantValue(token.id == ConditionTokenId.True ? true : false));
                    waitingValue = false;
                    break;
                }
                case ConditionTokenId.SymbolConst:
                {
                    values.Push(new SymbolValue(token.GetText(text)));
                    waitingValue = false;
                    break;
                }
                case ConditionTokenId.IntegerVar:
                {
                    if (!ParseVariableScope(token.GetText(text), dialogue.Label, out string name, out string context))
                    {
                        PushError(line, "Cannot parse integer variable");
                        return false;
                    }

                    values.Push(new IntegerVariableValue(name, context));
                    waitingValue = false;
                    break;
                }
                case ConditionTokenId.BoolVar:
                {
                    if (!ParseVariableScope(token.GetText(text), dialogue.Label, out string name, out string context))
                    {
                        PushError(line, "Cannot parse bool variable");
                        return false;
                    }

                    values.Push(new BoolVariableValue(name, context));
                    waitingValue = false;
                    break;
                }
                case ConditionTokenId.SymbolVar:
                {
                    if (!ParseVariableScope(token.GetText(text), dialogue.Label, out string name, out string context))
                    {
                        PushError(line, "Cannot parse symbol variable");
                        return false;
                    }

                    values.Push(new SymbolVariableValue(name, context));
                    waitingValue = false;
                    break;
                }
                case ConditionTokenId.External:
                {
                    if (!externalCodeParser.ParseCondition(token.GetText(text), out IBoolValue externalValue, (e) => PushError(line, e)))
                    {
                        PushError(line, "Cannot parse bool variable");
                        return false;
                    }

                    values.Push(externalValue);
                    waitingValue = false;
                    break;
                }
                case ConditionTokenId.Once:
                {
                    string name = "";
                    string context = "";
                    if (text.Length > 0 && !ParseVariableScope(token.GetText(text), dialogue.Label, out name, out context))
                    {
                        PushError(line, "Cannot parse bool variable");
                        return false;
                    }

                    bool usesAnonymousVariable = false;
                    if (string.IsNullOrEmpty(name))
                    {
                        // anonymous once value found, generating a name
                        dialogue.GenerateAnonymousBoolVarName("Once", out name, out context);
                        usesAnonymousVariable = true;
                    }
                    else if (!name.StartsWith("b"))
                    {
                        PushError(line, "The Once expression requires a boolean variable");
                        return false;
                    }

                    values.Push(new OnceValue(name, context, usesAnonymousVariable));
                    waitingValue = false;
                    break;
                }
                default:
                {
                    if (prefixActions.TryGetValue(token.id, out var action))
                    {
                        action(line);
                    }
                    else if (prefixOperators.TryGetValue(token.id, out var op))
                    {
                        PushPriorityOperator(op, line);
                    }
                    else
                    {
                        PushError(line, "Expected value");
                        return false; // Missing Value
                    }
                    break;
                }
            }

            return true;
        }

        bool ParseVariableScope(string variable, string dialogueSymbol, out string name, out string context)
        {
            if (variable.Contains("@"))
                variable = variable.Replace("@", dialogueSymbol + ".");

            var lastContextDot = variable.LastIndexOf('.');

            if (lastContextDot < 0)
            {
                context = "";
                name = variable;
            }
            else if (lastContextDot >= variable.Length - 1)
            {
                context = "";
                name = "";
                return false;
            }
            else
            {
                context = variable.Substring(0, lastContextDot + 1);
                name = variable.Substring(lastContextDot + 1);
            }

            return true;
        }

        bool ParseOperator(ConditionToken token, int line)
        {
            if (postfixOperators.TryGetValue(token.id, out var op))
                PushPriorityOperator(op, line);
            else if (infixOperators.TryGetValue(token.id, out op))
            {
                waitingValue = true;
                PushPriorityOperator(op, line);
            }
            else
            {
                // Missing operator
                return false;
            }
            return true;
        }

        void RegisterPrefixOperator(int prio, ConditionTokenId id, System.Func<int, bool> evaluator, System.Func<IValue> allocator)
        {
            prefixOperators[id] = new ExpressionOperator(id, -1, prio, evaluator, allocator);
        }

        void RegisterPostfixOperator(int prio, ConditionTokenId id, System.Func<int, bool> evaluator, System.Func<IValue> allocator)
        {
            postfixOperators[id] = new ExpressionOperator(id, prio, -1, evaluator, allocator);
        }

        void RegisterInfixOperator(int prio, ConditionTokenId id, System.Func<int, bool> evaluator, System.Func<IValue> allocator)
        {
            infixOperators[id] = new ExpressionOperator(id, prio, prio, evaluator, allocator);
        }

        void RegisterPrefixAction(ConditionTokenId id, System.Func<int, bool> action)
        {
            prefixActions[id] = action;
        }

        void InitializeExpressionParser()
        {
            waitingValue = true;

            RegisterPrefixOperator(0, ConditionTokenId.RoundOpen, OnPrefixOpenParenthesis, null);
            RegisterPostfixOperator(1, ConditionTokenId.RoundClose, OnPostfixCloseParenthesis, null);
            RegisterPrefixAction(ConditionTokenId.RoundClose, OnPostfixCloseParenthesis);
            RegisterInfixOperator(2, ConditionTokenId.Or, BinaryEvaluator, () => new OrCondition());
            RegisterInfixOperator(3, ConditionTokenId.And, BinaryEvaluator, () => new AndValue());

            RegisterInfixOperator(4, ConditionTokenId.Equal, BinaryEvaluator, () => new EqualValue());
            RegisterInfixOperator(4, ConditionTokenId.Different, BinaryEvaluator, () => new DifferentValue());
            RegisterInfixOperator(5, ConditionTokenId.Less, BinaryEvaluator, () => new LessValue());
            RegisterInfixOperator(5, ConditionTokenId.Greater, BinaryEvaluator, () => new GreaterValue());
            RegisterInfixOperator(5, ConditionTokenId.LEqual, BinaryEvaluator, () => new LEqualValue());
            RegisterInfixOperator(5, ConditionTokenId.GEqual, BinaryEvaluator, () => new GEqualValue());

            RegisterInfixOperator(8, ConditionTokenId.Add, BinaryEvaluator, () => new AddValue());
            RegisterInfixOperator(8, ConditionTokenId.Sub, BinaryEvaluator, () => new SubValue());
            RegisterInfixOperator(9, ConditionTokenId.Mul, BinaryEvaluator, () => new MulValue());
            RegisterInfixOperator(9, ConditionTokenId.Div, BinaryEvaluator, () => new DivValue());

            RegisterPrefixOperator(10, ConditionTokenId.Sub, UnaryEvaluator, () => new NegateValue());
            RegisterPrefixOperator(10, ConditionTokenId.Not, UnaryEvaluator, () => new NotValue());
        }

        Stack<IValue> values = new Stack<IValue>();
        Stack<ExpressionOperator> operators = new Stack<ExpressionOperator>();
        bool waitingValue;

        Dictionary<ConditionTokenId, ExpressionOperator> prefixOperators = new Dictionary<ConditionTokenId, ExpressionOperator>();
        Dictionary<ConditionTokenId, ExpressionOperator> infixOperators = new Dictionary<ConditionTokenId, ExpressionOperator>();
        Dictionary<ConditionTokenId, ExpressionOperator> postfixOperators = new Dictionary<ConditionTokenId, ExpressionOperator>();
        Dictionary<ConditionTokenId, System.Func<int, bool>> prefixActions = new Dictionary<ConditionTokenId, System.Func<int, bool>>();
    }
}