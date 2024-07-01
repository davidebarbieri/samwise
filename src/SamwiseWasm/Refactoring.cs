using System;
using System.Collections.Generic;
using System.Linq;

namespace Peevo.Samwise.Wasm
{
    public static class Refactoring
    {
        public static string ReplaceAnonymousVariables(string text, string filename, int line = -1)
        {
            List<Dialogue> dialogues = new List<Dialogue>();
            SamwiseParser parser = new SamwiseParser(new DummyCodeParser());

            try
            {
                bool res = parser.Parse(dialogues, text);

                if (!res)        
                {
                    Console.WriteLine("Unable to parse input text. Errors:");
                    foreach (var e in parser.Errors)
                    {
                        Console.WriteLine("- " + e);
                    }
                    return null;
                }
                
                HashSet<string> uniqueVariables = new HashSet<string>();
                
                foreach (var dialogue in dialogues)
                    dialogue.GatherStatefulVariables(uniqueVariables);

                List<string> lines = new List<string>(text.Split(new[] { Environment.NewLine }, StringSplitOptions.None));
                
                if (line < 0)
                {
                    foreach (var dialogue in dialogues)
                        foreach (var content in dialogue.ReplaceAnonymousVariablesStepped(uniqueVariables))
                        {
                             ReplaceLinesInRange(lines, content.SourceLineStart - 1, content.SourceLineEnd - 1, content.PrintLine(dialogue.IndentationUnit));
                        }
                }
                else
                {
                    Dialogue dialogue = EntryPoint.GetDialogueFromLine(filename, line);

                    foreach (var content in dialogue.ReplaceAnonymousVariablesStepped(uniqueVariables))
                    {
                        ReplaceLinesInRange(lines, content.SourceLineStart - 1, content.SourceLineEnd - 1, content.PrintLine(dialogue.IndentationUnit));
                    }
                }

                return string.Join(Environment.NewLine, lines.Where(s => !string.IsNullOrEmpty(s)));

            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                Console.WriteLine(e.StackTrace);
                return null;
            }
        }

        public static string AddUniqueIds(string text, string filename, int line = -1, bool toText = true)
        {
            List<Dialogue> dialogues = new List<Dialogue>();
            SamwiseParser parser = new SamwiseParser(new DummyCodeParser());

            try
            {
                bool res = parser.Parse(dialogues, text);

                if (!res)        
                {
                    Console.WriteLine("Unable to parse input text. Errors:");
                    foreach (var e in parser.Errors)
                    {
                        Console.WriteLine("- " + e);
                    }
                    return null;
                }
                
                HashSet<string> uniqueIDs = new HashSet<string>();
                
                foreach (var dialogue in dialogues)
                    dialogue.GatherUniqueIDs(uniqueIDs);

                List<string> lines = new List<string>(text.Split(new[] { Environment.NewLine }, StringSplitOptions.None));
                
                if (line < 0)
                {
                    foreach (var dialogue in dialogues)
                        foreach (var content in (toText ? dialogue.AssignUniqueIDsSteppedText(uniqueIDs) : dialogue.AssignUniqueIDsSteppedContent(uniqueIDs)))
                        {
                            ReplaceLinesInRange(lines, content.SourceLineStart - 1, content.SourceLineEnd - 1, content.PrintLine(dialogue.IndentationUnit));
                        }
                }
                else
                {
                    Dialogue dialogue = EntryPoint.GetDialogueFromLine(filename, line);

                    foreach (var content in (toText ? dialogue.AssignUniqueIDsSteppedText(uniqueIDs) : dialogue.AssignUniqueIDsSteppedContent(uniqueIDs)))
                         {
                        ReplaceLinesInRange(lines, content.SourceLineStart - 1, content.SourceLineEnd - 1, content.PrintLine(dialogue.IndentationUnit));
                    }
                }
                
                return string.Join(Environment.NewLine, lines.Where(s => !string.IsNullOrEmpty(s)));

            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                Console.WriteLine(e.StackTrace);
                return null;
            }
        }

        static void ReplaceLinesInRange(List<string> lines, int startLine, int endLine, string replacement)
        {
            if (startLine < 0 || endLine >= lines.Count || startLine > endLine)
            {
                throw new ArgumentException("Invalid range");
            }

            for (int i = startLine; i <= endLine; i++)
            {
                lines[i] = null;
            }
            
            lines[startLine] = replacement;
        }
    }
}