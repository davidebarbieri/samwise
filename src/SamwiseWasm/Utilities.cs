using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Peevo.Samwise.Wasm
{
    public static class Utilities
    {
        public static string ExportToCSV(string text)
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

                List<(string id, string kind, string who, string text)> rows = new List<(string id, string kind, string who, string text)>();

                foreach (var dialogue in dialogues)
                {
                    foreach (var content in dialogue.FindTextContent(true))
                    {
                        var id = content.GetID();

                        var asCaption = content as CaptionNode;
                        var asOption = content as Option;
                        var asSpeech = content as SpeechNode;

                        string kind = asCaption != null ? "*" :
                            asOption != null ? (asOption.MuteOption ? "--" : "-") :
                            ">";

                        string who = asSpeech != null ? asSpeech.CharacterId :
                            asOption != null ? asOption.Parent.CharacterId :
                            "";

                        rows.Add((id != null ? id : "", kind, who, content.Text));
                    }
                }

                StringBuilder csvData = new StringBuilder();
                using (StringWriter sw = new StringWriter(csvData))
                {
                    foreach (var row in rows)
                    {
                        string line = StringToCSVCell(row.Item1) + ", " + StringToCSVCell(row.Item2) + ", " + StringToCSVCell(row.Item3) + ", " + StringToCSVCell(row.Item4);

                        // Scrivi la riga nel file CSV
                        sw.WriteLine(line);
                    }
                    return sw.ToString();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                Console.WriteLine(e.StackTrace);
                return null;
            }
        }

        public static void GetInfo(IDialogueBlock dialogue, out int nodeCount, out int wordCount)
        {
            nodeCount = 0;
            wordCount = 0;

            foreach (var node in dialogue.Traverse())
            {
                ++nodeCount;

                if (node is ITextContent textNode)
                    wordCount += CountWords(textNode.Text);

                if (node is IChoosableNode)
                {
                    var choosableNode = (IChoosableNode)node;

                    for (int i = 0, count = choosableNode.OptionsCount; i < count; ++i)
                    {
                        var option = choosableNode.GetOption(i);
                        wordCount += CountWords(option.DefaultContent.Text);

                        if (option.AlternativeContents != null)
                            foreach (var content in option.AlternativeContents)
                                wordCount += CountWords(content.Text);
                    }
                }
            }
        }

        static int CountWords(string text)
        {
            int wordCount = 0, i = 0;

            TokenUtils.SkipWhitespaces(text, ref i);

            while (i < text.Length)
            {
                while (i < text.Length && text[i] != ' ')
                    i++;

                wordCount++;

                TokenUtils.SkipWhitespaces(text, ref i);
            }

            return wordCount;
        }

        static string StringToCSVCell(string text)
        {
            bool toEscape = (text.Contains(',') || text.Contains('"') || text.Contains('\r') || text.Contains('\n'));
            if (toEscape)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append('\"');
                foreach (char nextChar in text)
                {
                    sb.Append(nextChar);
                    if (nextChar == '"')
                        sb.Append('\"');
                }
                sb.Append('\"');
                return sb.ToString();
            }

            return text;
        }
    }
}