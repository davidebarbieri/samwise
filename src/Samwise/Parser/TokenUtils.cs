// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

using System;
using System.Collections.Generic;

namespace Peevo.Samwise
{
    public static class TokenUtils
    {
        public static string ReadTillEndLine(string text, ref int position, ref int line)
        {
            int startPos = position;
            while (position < text.Length)
            {
                var prevPos = position;
                if (ParseEndline(text, ref position, ref line))
                {
                    return text.Substring(startPos, prevPos - startPos).Trim();
                }
                else
                    ++position;
            }

            return text.Substring(startPos, position - startPos).Trim();
        }

        public static string ReadBeforeCharacter(string text, ref int position, ref int line, char character)
        {
            int startPos = position;
            while (position < text.Length)
            {
                if (text[position] == character)
                {
                    return text.Substring(startPos, position - startPos).Trim();
                }
                else
                    ++position;
            }

            return text.Substring(startPos, position - startPos).Trim();
        }

        public static string ReadFreeString(string text, ref int position, ref int line)
        {
            var sb = stringBuilder.Value;
            sb.Clear();

            SkipWhitespaces(text, ref position);

            int startPos = position;
            while (position < text.Length)
            {
                switch (text[position])
                {
                    case '\u21B5': // ↵
                        {
                            // Non-breaking carriage return, try to skip end line
                            sb.Append(text, startPos, position - startPos);
                            sb.Append('\n');
                            ++position;
                            ParseEndline(text, ref position, ref line);
                            startPos = position;
                            break;
                        }
                    case '\n':
                        {
                            // end line
                            sb.Append(text, startPos, position - startPos);
                            goto makeString;
                        }
                    case '#':
                        {
                            if (position == text.Length - 1 || text[position + 1] != '#')
                            {
                                sb.Append(text, startPos, position - startPos);
                                startPos = position;
                                goto makeString;
                            }

                            // double ##
                            position += 2;
                            break;
                        }
                    case '\r':
                        {
                            // Skip
                            sb.Append(text, startPos, position - startPos);

                            ++position;
                            startPos = position;
                            break;
                        }
                    default:
                        ++position;
                        break;
                }
            }

            if (position > startPos)
                sb.Append(text, startPos, position - startPos);

            makeString:
            return sb.ToString().TrimEnd();
        }

        public static bool ParseDoubleQuotedString(string text, ref int position, int line, out string result, out string error)
        {
            result = "";
            error = "";

            TokenUtils.SkipWhitespaces(text, ref position);

            if (position >= text.Length)
                return false;

            if (text[position] != '"')
                return false;

            ++position;

            var sb = stringBuilder.Value;
            sb.Clear();
            while (position < text.Length)
            {
                var prevPos = position;
                if (text[position] == '\n')
                {
                    error = "Expected leading \" character";
                    return false;
                }
                else if (text[position] == '\"')
                {
                    result = sb.ToString();
                    ++position;
                    return true;
                }
                else if (text[position] == '\\')
                {
                    if (position == text.Length)
                        return false;

                    switch (text[position + 1])
                    {
                        case '\"':
                        case '\\':
                            sb.Append(text[position + 1]);
                            position += 2;
                            break;

                        case 't':
                            sb.Append('\t');
                            position += 2;
                            break;

                        case 'n':
                            sb.Append('\n');
                            position += 2;
                            break;

                        default:
                            error = "Unrecognized escape character \\" + text[position + 1];
                            return false;
                    }
                }
                else
                {
                    sb.Append(text[position]);
                    ++position;
                }
            }

            error = "Expected leading \" character";
            return false;
        }

        public static bool ReadLineUntil(string text, ref int position, out string output, System.Func<char, bool> testFunction)
        {
            output = "";
            int startPos = position;
            while (position < text.Length)
            {
                var currChar = text[position];
                if (PeekEndline(text, position) || testFunction(currChar))
                {
                    break;
                }
                else
                    ++position;
            }

            if (startPos == position)
                return false;

            output = text.Substring(startPos, position - startPos);
            return true;
        }

        public static bool ReadTillNextAttributes(string text, int line, ref int position, out string output)
        {
            int startPos = position;
            int newPos = position;
            int newLine = line;

            while (position < text.Length)
            {
                if (ParseEndline(text, ref newPos, ref newLine))
                {
                    break;
                }
                else if (ParseCharacter(text, ref newPos, ',') || ParseCharacter(text, ref newPos, ']'))
                {
                    if (newPos - 1 == startPos)
                        break;

                    output = text.Substring(startPos, newPos - 1 - startPos).Trim();
                    position = newPos - 1;

                    return true;
                }
                else
                    ++newPos;
            }

            output = "";
            return false;
        }
        public static ErrorCode ParseLineStart(string text, ref int position, ref int line, out int newDepth, ref string indentation)
        {
        retry:
            newDepth = 0;

            if (indentation == null)
            {
                // infer indentation
                if (ParseCharacter(text, ref position, '\t'))
                {
                    indentation = "\t";
                    newDepth = 1;
                }
                else if (ParseCharacter(text, ref position, ' '))
                {
                    // Use spaces
                    int whitespaces = 1;

                    while (ParseCharacter(text, ref position, ' '))
                        ++whitespaces;

                    indentation = new string(' ', whitespaces);
                    newDepth = 1;
                }
                else
                {
                    // no indentation yet
                    return ErrorCode.Success;
                }

                // mixed tabs and spaces
                if (ParseCharacter(text, ref position, '\t') || ParseCharacter(text, ref position, ' '))
                {
                    indentation = null;
                    return ErrorCode.BadIndentation;
                }

                // empty line
                if (ParseEndline(text, ref position, ref line))
                {
                    // don't consider this line as a sample for the chosen indentation
                    indentation = null;
                    goto retry;
                }

                return ErrorCode.Success;
            }
            else
            {
                while (ParseToken(text, ref position, indentation, false))
                    ++newDepth;

                bool badIndentation = ParseCharacter(text, ref position, '\t') || ParseCharacter(text, ref position, ' ');

                if (ParseEndline(text, ref position, ref line))
                    goto retry;

                if (badIndentation)
                    return ErrorCode.BadIndentation;

                return ErrorCode.Success;
            }
        }

        public static ErrorCode ParseTitleLabel(string text, ref int position, ref int line, out string name)
        {
            SkipWhitespaces(text, ref position);

            int startPos = position;

            if (position < text.Length)
            {
                if (!IsUpperCaseChar(text[position]))
                {
                    name = "";
                    return ErrorCode.NoFirstUpperCase;
                }

                ++position;

                while (position < text.Length && IsAlphanumericChar(text[position]))
                    ++position;
            }

            if (startPos < text.Length && position > startPos)
            {
                name = text.Substring(startPos, position - startPos);
                return ErrorCode.Success;
            }

            name = "";
            return ErrorCode.EmptyLabel;
        }

        public static ErrorCode ParseLabel(string text, ref int position, ref int line, out string name)
        {
            SkipWhitespaces(text, ref position);

            int startPos = position;

            if (position < text.Length)
            {
                if (!IsLowerCaseChar(text[position]))
                {
                    name = "";
                    return ErrorCode.NoFirstLowerCase;
                }

                ++position;

                while (position < text.Length && IsAlphanumericChar(text[position]))
                    ++position;
            }

            if (startPos < text.Length && position > startPos)
            {
                name = text.Substring(startPos, position - startPos);
                return ErrorCode.Success;
            }

            name = "";
            return ErrorCode.EmptyLabel;
        }

        public static bool ParseName(string text, ref int position, out string fullName)
        {
            SkipWhitespaces(text, ref position);

            int startPos = position;

            while (position < text.Length && IsNameChar(text[position]))
                ++position;

            if (startPos < text.Length && position > startPos)
            {
                fullName = text.Substring(startPos, position - startPos);
                return true;
            }

            fullName = "";
            return false;
        }

        public static bool ParseSymbolString(string text, ref int position, out string fullName)
        {
            SkipWhitespaces(text, ref position);

            int startPos = position;

            // Symbol strings must start with an Uppercase character, and be followed by uppercase chars, dots, underscore
            if (position < text.Length && IsUpperCaseChar(text[position]))
                ++position;
            else
            {
                fullName = "";
                position = startPos;
                return false;
            }

            while (position < text.Length && IsSymbolStringChar(text[position]))
                ++position;

            if (startPos < text.Length && position > startPos)
            {
                fullName = text.Substring(startPos, position - startPos);
                return true;
            }

            fullName = "";
            position = startPos;
            return false;
        }

        public static string MakeAbsoluteContext(string varContext, bool hasShortcutName, string dialogueSymbol)
        {
            // Unique context is "" "." or "[A].[B].[C]."

            if (string.IsNullOrEmpty(varContext))
                return "";

            if (hasShortcutName)
                varContext = varContext.Replace("@", dialogueSymbol + ".");

            if (varContext.Length == 1)
            {
                if (varContext[0] == '.')
                    return "."; // acceptable
                return varContext + ".";
            }

            // trim initial and final "."
            var firstDot = varContext[0] == '.';
            var lastDot = varContext[varContext.Length - 1] == '.';

            if (firstDot)
                varContext = varContext.Substring(1);

            if (lastDot)
                return varContext;

            return varContext + ".";
        }

        public static bool ParseVariableName(string text, ref int position, int line, out string name, out string context, out bool hasShortcutName, List<ParseError> errors)
        {
            SkipWhitespaces(text, ref position);

            int startPos = position;
            int contextLastPos = -2;
            hasShortcutName = false;

            while (position < text.Length)
            {
                var nextCharacter = text[position];

                if (nextCharacter == '.')
                {
                    if (contextLastPos == position - 1)
                    {
                        // subsequent dots
                        position = startPos;
                        name = "";
                        context = "";
                        errors.Add(new ParseError { Error = "Subsequent dots found in context name", Line = line });
                        return false;
                    }
                    else
                    {
                        contextLastPos = position;
                    }
                }
                else if (nextCharacter == '@')
                {
                    hasShortcutName = true;
                    contextLastPos = position;
                }
                else if (!IsNameChar(nextCharacter))
                    break;

                ++position;
            }

            if (contextLastPos == -2)
                contextLastPos = startPos - 1;

            // context without variable
            if (position <= contextLastPos + 1)
            {
                name = "";
                context = "";
                position = startPos;
                return false;
            }

            if (startPos < text.Length && position > startPos)
            {
                var firstChar = text[contextLastPos + 1];

                if (firstChar != 'b' && firstChar != 'i' && firstChar != 's')
                {
                    name = "";
                    context = "";
                    position = startPos;
                    return false;
                }

                name = text.Substring(contextLastPos + 1, position - contextLastPos - 1);

                if (contextLastPos >= startPos) // "." is a valid global context
                    context = text.Substring(startPos, contextLastPos - startPos + 1);
                else
                    context = "";

                return true;
            }

            name = "";
            context = "";
            position = startPos;
            return false;
        }

        public static bool ParseVariableNameToken(string text, ref int position, out int tokenLength)
        {
            SkipWhitespaces(text, ref position);

            int startPos = position;

            char nextChar;
            while (position < text.Length && (IsNameChar(nextChar = text[position]) || nextChar == '@'))
                ++position;

            if (startPos < text.Length && position > startPos)
            {
                tokenLength = position - startPos;
                return true;
            }

            tokenLength = 0;
            return false;
        }

        public static bool ParseMultiplier(string text, ref int position, out int multiplier)
        {
            SkipWhitespaces(text, ref position);

            int startPos = position;

            while (position < text.Length && IsNumberChar(text[position]))
                ++position;

            if (position < text.Length && text[position] == 'x')
            {
                if (startPos < text.Length && position > startPos)
                {
                    int valueLength = position++ - startPos;
                    return int.TryParse(text.Substring(startPos, valueLength), out multiplier);
                }
            }

            position = startPos;
            multiplier = 0;
            return false;
        }

        public static bool ParseTime(string text, ref int position, out double time)
        {
            SkipWhitespaces(text, ref position);

            int startPos = position;

            while (position < text.Length && (IsNumberChar(text[position]) || text[position] == '.'))
                ++position;

            if (position < text.Length && text[position] == 's')
            {
                if (startPos < text.Length && position > startPos)
                {
                    return double.TryParse(text.Substring(startPos, position++ - startPos), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out time);
                }
            }

            position = startPos;
            time = 0;
            return false;
        }

        public static bool ParseNamedAttributes(string type, string text, ref int position, out string name)
        {
            int startPos = position;

            if (!ParseToken(text, ref position, type, true))
                goto fail;

            if (position < text.Length && IsNameChar(text[position]))
                goto fail;

            startPos = position;

            if (ParseName(text, ref position, out name))
                return true;

            throw new ArgumentException("Invalid name");
        fail:
            position = startPos;
            name = null;
            return false;
        }

        public static bool ParseSymbol(string text, ref int position, ref int line, out Symbol symbol)
        {
            if (ParseToken(text, ref position, "?", true))
            {
                symbol = Symbol.Fallback;
                return true;
            }
            if (ParseToken(text, ref position, ">", true))
            {
                if (ParseCharacter(text, ref position, '>'))
                {
                    if (ParseCharacter(text, ref position, '>'))
                    {
                        symbol = Symbol.Clamp;
                        return true;
                    }

                    symbol = Symbol.Loop;
                    return true;
                }

                if (ParseCharacter(text, ref position, '<'))
                {

                    symbol = Symbol.PingPong;
                    return true;
                }

                symbol = Symbol.Says;
                return true;
            }
            if (ParseToken(text, ref position, ":", true))
            {
                symbol = Symbol.Choice;
                return true;
            }
            if (ParseToken(text, ref position, "%", true))
            {
                symbol = Symbol.Score;
                return true;
            }
            if (ParseToken(text, ref position, "=>", true))
            {
                symbol = Symbol.Fork;
                return true;
            }
            if (ParseToken(text, ref position, "<", true))
            {
                if (ParseToken(text, ref position, "=", false))
                {
                    if (ParseToken(text, ref position, ">", false))
                    {
                        symbol = Symbol.Await;
                        return true;
                    }

                    symbol = Symbol.Join;
                    return true;
                }
                else if (ParseToken(text, ref position, "!=", false))
                {
                    symbol = Symbol.Cancel;
                    return true;
                }
            }
            if (ParseToken(text, ref position, "$", true))
            {
                symbol = Symbol.Check;
                return true;
            }
            if (ParseToken(text, ref position, "!", true))
            {
                if (ParseToken(text, ref position, "!", true))
                {
                    symbol = Symbol.ResetAndInterrupt;
                    return true;
                }

                symbol = Symbol.Interrupt;
                return true;
            }

            symbol = Symbol.Invalid;
            return false;
        }

        public static bool ParseInt(string text, ref int position, ref int line, out int value)
        {
            SkipWhitespaces(text, ref position);

            int startPos = position;

            while (position < text.Length && IsNumberChar(text[position]))
                ++position;

            if (startPos < text.Length && position > startPos)
            {
                return int.TryParse(text.Substring(startPos, position - startPos), out value);
            }

            position = startPos;

            value = 0;
            return false;
        }

        public static bool ParseLong(string text, ref int position, ref int line, out long value)
        {
            SkipWhitespaces(text, ref position);

            int startPos = position;

            if (position < text.Length && (text[position] == '-' || text[position] == '+'))
                ++position;

            while (position < text.Length && IsNumberChar(text[position]))
                ++position;

            if (startPos < text.Length && position > startPos)
            {
                return long.TryParse(text.Substring(startPos, position - startPos), out value);
            }

            position = startPos;

            value = 0;
            return false;
        }

        public static bool ParseToken(string text, ref int position, string token, bool skipWhitespaces = true)
        {
            if (skipWhitespaces)
                SkipWhitespaces(text, ref position);

            if (text.Length - position < token.Length)
                return false;

            if (string.CompareOrdinal(text, position, token, 0, token.Length) == 0)
            {
                position += token.Length;
                return true;
            }

            return false;
        }

        public static void SkipWhitespaces(string text, ref int position)
        {
            while (ParseCharacter(text, ref position, ' ')) ;
        }

        public static int SkipWhitespacesAndTabs(string text, ref int position)
        {
            int initPos = position;
            while (ParseCharacter(text, ref position, ' ') || ParseCharacter(text, ref position, '\t')) ;
            return position - initPos;
        }

        public static void SkipWhiteLines(string text, ref int position, ref int line)
        {
            int cPos = position;
            int cLine = line;

            int disabledTextOpened = 0;

            int indentSize = -1;
        retry:
            int newIndentSize = SkipWhitespacesAndTabs(text, ref cPos);
            if (indentSize < 0) indentSize = newIndentSize;

            if (cPos == text.Length)
            {
                position = cPos;
                return;
            }

            // Skip disabled lines
            if (ParseToken(text, ref cPos, "/~", true) && ParseEndline(text, ref cPos, ref cLine))
            {
                ++disabledTextOpened;
                position = cPos;
                line = cLine;
                goto retry;
            }

            if (ParseToken(text, ref cPos, "~/", true) && ParseEndline(text, ref cPos, ref cLine))
            {
                --disabledTextOpened;
                position = cPos;
                line = cLine;
                goto retry;
            }

            if (disabledTextOpened > 0)
            {
                // Disabled line
                SkipLine(text, ref cPos, ref cLine);
                position = cPos;
                line = cLine;
                goto retry;
            }

            if (ParseToken(text, ref cPos, "//"))
            {
                SkipMultiline(text, ref cPos, ref cLine);
                position = cPos;
                line = cLine;
                goto retry;
            }
            
            
            if (ParseToken(text, ref cPos, "~"))
            {
                SkipSublines(indentSize, text, ref cPos, ref cLine);
                position = cPos;
                line = cLine;
                goto retry;
            }

            // Empty line
            if (ParseEndline(text, ref cPos, ref cLine))
            {
                position = cPos;
                line = cLine;
                goto retry;
            }
        }

        public static void SkipLine(string text, ref int position, ref int line)
        {
            int startPos = position;
            while (position < text.Length)
            {
                var prevPos = position;
                if (ParseEndline(text, ref position, ref line))
                {
                    return;
                }
                else
                    ++position;
            }
        }

        public static void SkipMultiline(string text, ref int position, ref int line)
        {
            while (position < text.Length)
            {
                var prevPos = position;

                bool isNewLineCharacter = ParseToken(text, ref position, "↵", true);
                bool isEndLine = ParseEndline(text, ref position, ref line);

                if (!isNewLineCharacter && isEndLine)
                {
                    return;
                }
                else
                    ++position;
            }
        }

        public static void SkipSublines(int baseIndent, string text, ref int position, ref int line)
        {
            SkipMultiline(text, ref position, ref line);

            while (position < text.Length)
            {
                int cPos = position;
                var newIndent = SkipWhitespacesAndTabs(text, ref cPos);

                // Empty line
                if (ParseEndline(text, ref cPos, ref line))
                {
                    position = cPos;
                }
                else if (newIndent > baseIndent)
                {
                    SkipSublines(newIndent, text, ref position, ref line);
                }
                else
                    return;
            }
        }

        public static bool ParseCharacter(string text, ref int position, char character)
        {
            if (position >= text.Length)
                return false;

            if (text[position] == character)
            {
                position++;
                return true;
            }
            return false;
        }

        public static bool PeekEndline(string text, int position)
        {
            int line = 0;
            return ParseEndline(text, ref position, ref line);
        }

        public static bool ParseEndline(string text, ref int position, ref int line)
        {
            int initialPos = position;
            SkipWhitespaces(text, ref position);

            if (position > text.Length)
            {
                position = initialPos;
                return false;
            }

            // EOF reached
            if (position == text.Length)
            {
                ++position;
                ++line;
                return true;
            }
            else if (ParseCharacter(text, ref position, '\n')
                || ParseToken(text, ref position, "\r\n")
                || ParseToken(text, ref position, "\r")
                )
            {
                ++line;
                return true;
            }

            position = initialPos;
            return false;
        }

        // Symbol Strings are uppercase
        public static bool IsSymbolStringChar(char character)
        {
            return (character >= 'A' && character <= 'Z') ||
                IsNumberChar(character) ||
                character == '.' ||
                character == '_';
        }

        public static bool IsNameChar(char character)
        {
            return (character >= 'a' && character <= 'z') ||
                (character >= 'A' && character <= 'Z') ||
                IsNumberChar(character) ||
                character == '.' ||
                character == '_';
        }

        public static bool IsLowerCaseChar(char character)
        {
            return (character >= 'a' && character <= 'z');
        }

        public static bool IsUpperCaseChar(char character)
        {
            return (character >= 'A' && character <= 'Z');
        }

        public static bool IsAlphanumericChar(char character)
        {
            return (character >= 'a' && character <= 'z') ||
                (character >= 'A' && character <= 'Z') ||
                character == '_' ||
                IsNumberChar(character);
        }

        public static bool IsNumberChar(char character) => character >= '0' && character <= '9';

        public static bool IsTitleSign(char character)
        {
            switch (character)
            {
                case '¯': // 238
                case '«': // 174
                case '»': // 175

                case '─': // 196
                case '┬': // 194
                case '┼': // 197
                case '│': // 179
                case '┤': // 180
                case '├': // 195
                case '┴': // 193
                case '┐': // 191
                case '┘': // 217
                case '┌': // 218
                case '└': // 192

                case '═': // 205
                case '╦': // 203
                case '╬': // 205
                case '║': // 186
                case '╣': // 185
                case '╠': // 204
                case '╩': // 202
                case '╗': // 187
                case '╝': // 188
                case '╔': // 201
                case '╚': // 200

                case '░': // 176
                case '▒': // 177
                case '▓': // 178
                case '█': // 219

                case '▀': // 223
                case '■': // 254
                case '▄': // 220

                case '±': // 241
                case '‗': // 242

                case '¶': // 244
                case '§': // 245

                    return true;
                default:
                    return false;
            }
        }

        static System.Threading.ThreadLocal<System.Text.StringBuilder> stringBuilder =
            new System.Threading.ThreadLocal<System.Text.StringBuilder>(() => new System.Text.StringBuilder());
    }
}