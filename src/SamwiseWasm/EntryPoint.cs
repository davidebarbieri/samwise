using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Uno.Foundation;

namespace Peevo.Samwise.Wasm
{
    public static class EntryPoint
    {
        public static void DeleteCodeData(string filename)
        {
            codebaseDB.ResetReferences(filename);
        }

        public static string ParseDialogue(string filename, string text)
        {
            var dialogues = ParseDialogues(text, out var errorString);

            if (!string.IsNullOrEmpty(errorString))
            {
                // Remove file
                //fileToDialogues.Remove(filename);
                return "{ \"errors\": " + errorString + "}";
            }

            DeleteCodeData(filename);

            string symbolsTree = "[";

            if (dialogues != null)
            {
                var dialogueFile = new ParsedDialogueFile(filename);
                fileToDialogues[filename] = dialogueFile;

                // Save dialogues
                for (int i=0; i<dialogues.Count; ++i)
                {
                    var dialogue = dialogues[i];
                    codebaseDB.AddDialogue(filename, dialogue);
                    dialogueFile.AddDialogue(dialogue);

                    symbolsTree +=  "{\"symbol\":\"" + dialogue.Label + "\", \"name\":\"" + dialogue.Title + "\", \"type\": \"Dialogue\", \"children\": [";

                    bool firstOne = true;
                    foreach (var label in dialogue.GetLabels())
                    {
                        if (!firstOne)
                            symbolsTree += ", ";
                        firstOne = false;

                        symbolsTree +=  "{\"symbol\":\"" + label.Key + "\", \"name\":\"\", \"type\": \"Label\", \"children\": [], \"startLine\": "
                            + label.Value.SourceLineStart + ", \"endLine\": " + label.Value.SourceLineEnd + "}";
                    }
                    //dialogue.
                    
                    symbolsTree +=  "], \"startLine\": " + dialogue.SourceLineStart + ", \"endLine\": " + dialogue.SourceLineEnd + "}" + (i < dialogues.Count - 1 ? "," : "");
                }
            }   

            return symbolsTree + "]";
        }

        public static void StartDialogue(string symbol)
        {
            Initialize();

            try
            {
                if (codebaseDB.GetDialogue(symbol, out var dialogue))
                    dialogueMachine.Start(dialogue);
            }
            catch (DialogueException e)
            {
                OnError(e.Context, e.Message);
            }
        }

        public static void StartDialogueFromSrcPosition(string filename, int line)
        {
            if (line < 0)
                return;

            Initialize();

            try
            {
                if (fileToDialogues.TryGetValue(filename, out var dFile))
                {
                    var node = dFile.DebugInformation.GetDialogueNode(line);

                    if (node != null)
                    {
                        dialogueMachine.Start(node);
                        //return true;
                    }
                }
            }
            catch (DialogueException e)
            {
                OnError(e.Context, e.Message);
            }

            //return false;
        }

        public static void Advance(long contextId)
        {
            IDialogueContext context;
            if ((context = dialogueMachine.LookupDialogue(contextId)) == null)
            {
                Console.WriteLine("Error: can't find context with id " + contextId);
                return;
            }

            try
            {
                context.Advance();
            }
            catch (Exception e)
            {
                Console.WriteLine("Message: " + e.Message );
                Console.WriteLine("StackTrace: " + e.StackTrace );

                OnError(context, e.Message);
            }
        }

        public static bool TryResolveMissingDialogues(long contextId)
        {
            IDialogueContext context;
            if ((context = dialogueMachine.LookupDialogue(contextId)) == null)
                return false;
                
            try 
            {
                if (context.TryResolveMissingDialogues())
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Message: " + e.Message );
                Console.WriteLine("StackTrace: " + e.StackTrace );
                OnError(context, e.Message);
            }
            return false;
        }

        public static void Stop(long contextId)
        {
            IDialogueContext context;
            if ((context = dialogueMachine.LookupDialogue(contextId)) == null)
            {
                Console.WriteLine("Error: can't find context with id " + contextId);
                return;
            }
                
            context.Stop();
        }

        public static void StopAll()
        {
            dialogueMachine?.StopAll();
        }

        public static void Choose(long contextId, int optionId)
        {
            IDialogueContext context;
            if ((context = dialogueMachine.LookupDialogue(contextId)) == null)
            {
                Console.WriteLine("Error: can't find context with id " + contextId);
                return;
            }

            try
            {
                var chooseNode = context.Current as IChoosableNode;

                if (chooseNode != null)
                {
                    if (optionId >= 0)
                    {
                        var choice = chooseNode.GetOption(optionId);

                        context.Choose(choice);
                    }
                    else
                        context.Choose(null);
                }  
            }
            catch (Exception e)
            {
                Console.WriteLine("Message: " + e.Message );
                Console.WriteLine("StackTrace: " + e.StackTrace );
                OnError(context, e.Message);
            }
        }

        public static void CompleteChallenge(long contextId, bool result)
        {
            IDialogueContext context;
            if ((context = dialogueMachine.LookupDialogue(contextId)) == null)
            {
                Console.WriteLine("Error: can't find context with id " + contextId);
                return;
            }

            try
            {
                context.CompleteChallenge(result);
            }
            catch (Exception e)
            {
                Console.WriteLine("Message: " + e.Message );
                Console.WriteLine("StackTrace: " + e.StackTrace );
                OnError(context, e.Message);
            }
        }

        public static Dialogue GetDialogueFromLine(string filename, int line)
        {
            if (fileToDialogues.TryGetValue(filename, out var info))
            {
                var dialogue = info.DebugInformation.GetDialogue(line);

                return dialogue;
            }
            return null;
        }

        public static string GetDialogueSymbolFromLine(string filename, int line)
        {
            if (fileToDialogues.TryGetValue(filename, out var info))
            {
                var dialogue = info.DebugInformation.GetDialogue(line);

                return dialogue != null ? dialogue.Label : "";
            }
            return "";
        }

        public static string GetDialogueTitleFromLine(string filename, int line)
        {
            if (fileToDialogues.TryGetValue(filename, out var info))
            {
                var dialogue = info.DebugInformation.GetDialogue(line);

                return dialogue != null ? dialogue.Title : "";
            }
            return "";
        }

        public static string GetDestinationSymbol(string filename, int line)
        {
            if (fileToDialogues.TryGetValue(filename, out var info))
            {
                var node = info.DebugInformation.GetDialogueNode(line);

                switch (node)
                {
                case LocalGotoNode localGotoNode:
                    if (localGotoNode.Destination == null)
                        return ""; // -> end
                    return localGotoNode.Destination.Label;
                case LocalAwaitNode localAwaitNode:
                    return localAwaitNode.Destination.Label;
                case LocalForkNode localForkNode:
                    return localForkNode.Destination.Label;
                case GotoNode gotoNode:
                    {
                        string target = gotoNode.DestinationDialogueId;
                        if (!string.IsNullOrEmpty(gotoNode.DestinationLabel))
                            target += "." + gotoNode.DestinationLabel;
                        return target;
                    }
                case AwaitNode awaitNode:
                    {
                        string target = awaitNode.DestinationDialogueId;
                        if (!string.IsNullOrEmpty(awaitNode.DestinationLabel))
                            target += "." + awaitNode.DestinationLabel;
                        return target;
                    }
                case ForkNode forkNode:
                    {
                        string target = forkNode.DestinationDialogueId;
                        if (!string.IsNullOrEmpty(forkNode.DestinationLabel))
                            target += "." + forkNode.DestinationLabel;
                        return target;
                    }

                }
            }
            return "";
        }

        public static bool IsDialogueNodeFromLine(string filename, int line)
        {
            if (line < 0)
                return false;

            if (fileToDialogues.TryGetValue(filename, out var dFile))
            {
                var node = dFile.DebugInformation.GetDialogueNode(line);

                if (node != null)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsExitingNode(string filename, int line)
        {
             if (line < 0)
                return false;

            Initialize();

            try
            {
                if (fileToDialogues.TryGetValue(filename, out var dFile))
                {
                    var node = dFile.DebugInformation.GetDialogueNode(line);

                    if (node != null)
                    {
                        if (node is GotoNode)
                            return false;

                        return node is ExitNode || (node.FindNextSibling() == null && (!(node is IBlockContainerNode) || ((IBlockContainerNode)node).ChildrenCount == 0) );
                    }
                }
            }
            catch (DialogueException e)
            {
                OnError(e.Context, e.Message);
            }
            
            return false;
        }

        public static void ClearData()
        {
            sandboxDataRoot.Clear();
        }

        public static void ClearLocalData(long contextId)
        {
            IDialogueContext context;
            if ((context = dialogueMachine.LookupDialogue(contextId)) == null)
            {
                Console.WriteLine("Error: can't find context with id " + contextId);
                return;
            }

            context.DataContext.Clear();
        }

        public static void ClearLocalVariable(long contextId, string varName)
        {
            IDialogueContext context;
            if ((context = dialogueMachine.LookupDialogue(contextId)) == null)
            {
                Console.WriteLine("Error: can't find context with id " + contextId);
                return;
            }

            var dataContext = context.DataContext;

            if (varName[0] == 'b')
                dataContext.ClearValueBool(varName);
            else if (varName[0] == 'i')
                dataContext.ClearValueInt(varName);
            else // if (varName[0] == 's')
                dataContext.ClearValueSymbol(varName);
        }

        public static void ClearSubcontextData(string context)
        {
            var dataContext = sandboxDataRoot.LookupDataContext(context);

            if (dataContext != null)
                dataContext.Clear();
        }

        public static void ClearVariable (string context, string varName)
        {
            var dataContext = sandboxDataRoot.LookupOrCreateDataContext(context);

            if (varName[0] == 'b')
                dataContext.ClearValueBool(varName);
            else if (varName[0] == 'i')
                dataContext.ClearValueInt(varName);
            else // if (varName[0] == 's')
                dataContext.ClearValueSymbol(varName);
        }
        
        public static void SetSymbolData(string context, string varName, string value)
        {
            int p = 0;
            if (!TokenUtils.ParseSymbolString(value, ref p, out _))
            {
                //ShowErrorMessage("Invalid Symbol (symbols must start with an uppercase characters, and contain only uppercase characters, "." or "_")");
                return;
            }

            var dataContext = sandboxDataRoot.LookupOrCreateDataContext(context);
            dataContext.SetValueSymbol(varName, value);
        }

        public static void SetIntData(string context, string varName, long value)
        {
            var dataContext = sandboxDataRoot.LookupOrCreateDataContext(context);
            dataContext.SetValueInt(varName, value);
        }
        
        public static void SetBoolData(string context, string varName, bool value)
        {
            var dataContext = sandboxDataRoot.LookupOrCreateDataContext(context);
            dataContext.SetValueBool(varName, value);
        }

        public static void SetLocalSymbolData(long contextId, string varName, string value)
        {
            IDialogueContext context;
            if ((context = dialogueMachine.LookupDialogue(contextId)) == null)
            {
                Console.WriteLine("Error: can't find context with id " + contextId);
                return;
            }

            context.DataContext.SetValueSymbol(varName, value);
        }

        public static void SetLocalIntData(long contextId, string varName, long value)
        {
            IDialogueContext context;
            if ((context = dialogueMachine.LookupDialogue(contextId)) == null)
            {
                Console.WriteLine("Error: can't find context with id " + contextId);
                return;
            }

            context.DataContext.SetValueInt(varName, value);
        }
        
        public static void SetLocalBoolData(long contextId, string varName, bool value)
        {
            IDialogueContext context;
            if ((context = dialogueMachine.LookupDialogue(contextId)) == null)
            {
                Console.WriteLine("Error: can't find context with id " + contextId);
                return;
            }

            context.DataContext.SetValueBool(varName, value);
        }

        public static string GetSymbolData(string context, string varName)
        {
            var dataContext = sandboxDataRoot.LookupDataContext(context);

            if (dataContext == null)
                return "";

            return dataContext.GetValueSymbol(varName) ?? "";
        }

        public static string GetIntData(string context, string varName)
        {
            var dataContext = sandboxDataRoot.LookupDataContext(context);

            if (dataContext == null)
                return "0";

            return dataContext.GetValueInt(varName).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
        
        public static bool GetBoolData(string context, string varName)
        {
            var dataContext = sandboxDataRoot.LookupDataContext(context);

            if (dataContext == null)
                return false;

            return dataContext.GetValueBool(varName);
        }

        public static string GetLocalSymbolData(long contextId, string varName)
        {
            IDialogueContext context;
            if ((context = dialogueMachine.LookupDialogue(contextId)) == null)
                return "0";

            return context.DataContext.GetValueSymbol(varName) ?? "";
        }

        public static string GetLocalIntData(long contextId, string varName)
        {
            IDialogueContext context;
            if ((context = dialogueMachine.LookupDialogue(contextId)) == null)
                return "0";

            return context.DataContext.GetValueInt(varName).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
        
        public static bool GetLocalBoolData(long contextId, string varName)
        {
            IDialogueContext context;
            if ((context = dialogueMachine.LookupDialogue(contextId)) == null)
                return false;

            return context.DataContext.GetValueBool(varName);
        }

        public static string ImportVariables(string text)
        {
            var dialogues = ParseDialogues(text, out var errorString);

            if (!string.IsNullOrEmpty(errorString))
            {
                return "{ \"errors\": " + "Unable to import variables - " + errorString + "}";
            }

            if (dialogues != null)
            {
                for (int i=0; i<dialogues.Count; ++i)
                {
                    var dialogue = dialogues[i];

                    var dialogueContext = dialogueMachine.Start(dialogue);

                    if (!dialogueContext.IsEnded)
                        dialogueContext.Stop();
                }
            }
            
            return "";
        }

        public static string ExportVariables(string context)
        {
            var dataContext = sandboxDataRoot.LookupDataContext(context);

            if (dataContext == null)
                return "";

            StringBuilder csvData = new StringBuilder();
            using (StringWriter  sw = new StringWriter(csvData))
            {
                sw.WriteLine("§ (ExportedData)");

                ExportSubVariables(dataContext, sw);
                return sw.ToString();
            }
        }

        public static void Update()
        {
            if (dialogueMachine != null)
                dialogueMachine.Update();
        }

        public static string GetReferences(string fullSymbol)
        {
            string refOutput = "[";

            bool firstOne = true;
            var refs = codebaseDB.GetReferences(fullSymbol);
            if (refs != null)
                foreach (var entry in refs)
                {
                    if (!firstOne)
                        refOutput += ", ";
                    firstOne = false;
                    refOutput += "{" +
                        "\"file\": \"" + ProcessTextForSerialization(entry.file) + "\", " +
                        "\"line\": " + entry.line + 
                        "}";
                }
            else
                return "[]";

            return refOutput + "]";
        }

        public static string GetDialogueStats(string symbol)
        {
            Initialize();

            try
            {
                if (codebaseDB.GetDialogue(symbol, out var dialogue))
                {
                    Utilities.GetInfo(dialogue, out var nodeCount, out var wordCount);
                    return "{\"title\":\"" + ProcessTextForSerialization(dialogue.Title) + "\", \"nodes\":" + nodeCount + ", \"words\":" + wordCount + "}";
                } 
            }
            catch {}

            return "";
        }

        static void ExportSubVariables(IDataContext dataContext, StringWriter stringWriter)
        {
            foreach ((string, bool) d in dataContext.GetBoolVariables())
            {
                stringWriter.WriteLine("{ " + dataContext.Name + d.Item1 + " = " + (d.Item2 ? "true" : "false") + " }");
            }
            
            foreach ((string, long) d in dataContext.GetIntVariables())
            {
                stringWriter.WriteLine("{ " + dataContext.Name + d.Item1 + " = " + d.Item2 + " }");
            }
            
            foreach ((string, string) d in dataContext.GetSymbolVariables())
            {
                stringWriter.WriteLine("{ " + dataContext.Name + d.Item1 + " = " + d.Item2 + " }");
            }

            foreach (var subContexes in sandboxDataRoot.GetSubcontexes(dataContext))
                ExportSubVariables(subContexes, stringWriter);
        }


        static void Initialize()
        {
            if (dialogueMachine == null)
            {
                dialogueMachine = new DialogueMachine(codebaseDB, sandboxDataRoot, new DummyExternalCodeMachine());

                dialogueMachine.onDialogueContextStart += OnDialogueContextStart;
                dialogueMachine.onDialogueContextStop += OnDialogueContextStop;
                dialogueMachine.onDialogueContextEnd += OnDialogueContextEnd;

                dialogueMachine.onCaptionStart += OnCaptionStart;
                dialogueMachine.onCaptionEnd += OnCaptionEnd;
                dialogueMachine.onSpeechStart += OnSpeechStart;
                dialogueMachine.onSpeechEnd += OnSpeechEnd;
                dialogueMachine.onChoiceStart += OnChoiceStart;
                dialogueMachine.onChoiceEnd += OnChoiceEnd;
                dialogueMachine.onWaitTimeStart += OnWaitTimeStart;
                dialogueMachine.onWaitTimeEnd += OnWaitTimeEnd;

                dialogueMachine.onSpeechOptionStart += OnSpeechOptionStart;
                dialogueMachine.onChallengeStart += OnChallengeStart;

                dialogueMachine.onWaitForMissingDialogueStart += OnWaitForMissingDialogueStart;
                dialogueMachine.onWaitForMissingDialogueEnd += OnWaitForMissingDialogueEnd;

                sandboxDataRoot.onIntDataChanged += OnIntDataChanged;
                sandboxDataRoot.onBoolDataChanged += OnBoolDataChanged;
                sandboxDataRoot.onSymbolDataChanged += OnSymbolDataChanged;
                sandboxDataRoot.onLocalIntDataChanged += OnLocalIntDataChanged;
                sandboxDataRoot.onLocalBoolDataChanged += OnLocalBoolDataChanged;
                sandboxDataRoot.onLocalSymbolDataChanged += OnLocalSymbolDataChanged;
                sandboxDataRoot.onDataClear += OnDataClear;
                sandboxDataRoot.onContextClear += OnContextClear;
                sandboxDataRoot.onLocalDataClear += OnLocalDataClear;
                sandboxDataRoot.onLocalContextClear += OnLocalContextClear;
            }
        }

        static List<Dialogue> ParseDialogues(string text, out string errorString)
        {
            errorString = null;

            List<Dialogue> dialogues = new List<Dialogue>();
            SamwiseParser parser = new SamwiseParser(new DummyCodeParser());

            try
            {
                bool res = parser.Parse(dialogues, text);

                if (!res)        
                {
                    /*
                    Console.WriteLine("Unable to parse input text. Errors:");
                    foreach (var e in parser.Errors)
                    {
                        Console.WriteLine("- " + e);
                    }
                    */

                    errorString = ErrorsToJsonStringArray(parser.Errors);
                    return null;
                }
                return dialogues;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                Console.WriteLine(e.StackTrace);
                
            }
                
            return null;
        }

        static string ErrorsToJsonStringArray(List<ParseError> errors)
        {
            if (errors == null || errors.Count == 0)
                return null;

            string errorString = "[";

            for (int i=0; i < errors.Count; ++i)
            {
                errorString += "{\"line\":" + errors[i].Line +", \"message\":\"" + errors[i].Error.Replace('"', '\'') + "\"}";

                if (i < errors.Count - 1)
                    errorString += ",";
            }

            return errorString + "]";
        }

        static void OnDialogueContextStart(IDialogueContext context) 
        {
            var id = ++uid;

            string title;
           
            if (context.Current.GetDialogue() == null)
                title = "<error: no dialogue>";
            else
                title = context.Current.GetDialogue().Title;

            if (string.IsNullOrEmpty(title))
            {
                title = context.Current.GetDialogue().Label;
            }

            title = ProcessTextForSerialization(title);
            Uno.Foundation.WebAssemblyRuntime.InvokeJS("onDialogueContextStart(" + id + ", \"" + title + "\");");
        }

        static void OnDialogueContextStop(IDialogueContext context) 
        {
            Uno.Foundation.WebAssemblyRuntime.InvokeJS("onDialogueContextEnd(" + context.Uid + ");");
        }

        static void OnDialogueContextEnd(IDialogueContext context) 
        {
            Uno.Foundation.WebAssemblyRuntime.InvokeJS("onDialogueContextEnd(" + context.Uid + ");");
        }

        static void OnSpeechStart(IDialogueContext context, SpeechNode node) 
        {
            Uno.Foundation.WebAssemblyRuntime.InvokeJS("onSpeechStart(" + context.Uid + ", \"" + node.CharacterId + "\", \"" + ProcessTextForSerialization(node.Text) + "\"," + Serialize(new LocationInfo(codebaseDB.GetFilename(node.GetDialogue()), node.SourceLineStart, node.SourceLineEnd)) + ");");
        }
        static void OnSpeechEnd(IDialogueContext context, SpeechNode node) 
        {
            Uno.Foundation.WebAssemblyRuntime.InvokeJS("onSpeechEnd(" + context.Uid + ")");
        }

        static void OnCaptionStart(IDialogueContext context, CaptionNode node) 
        {
            Uno.Foundation.WebAssemblyRuntime.InvokeJS("onCaptionStart(" + context.Uid + ", \"" + ProcessTextForSerialization(node.Text) + "\"," + Serialize(new LocationInfo(codebaseDB.GetFilename(node.GetDialogue()), node.SourceLineStart, node.SourceLineEnd)) + ");");
        }

        static void OnCaptionEnd(IDialogueContext context, CaptionNode node) 
        {
            Uno.Foundation.WebAssemblyRuntime.InvokeJS("onCaptionEnd(" + context.Uid + ")");
        }

        static void OnChoiceStart(IDialogueContext context, IChoosableNode node) 
        {
            var invokeString = "onChoiceStart(" + context.Uid + ", \"" + node.CharacterId + "\", [";

            bool otherOptionsAvailable = false;
            for (int i=0; i < node.OptionsCount; ++i)
            {
                var option = node.GetOption(i);

                if (context.Evaluate(option))
                {
                    invokeString += (otherOptionsAvailable ? "," : "") + "{"
                        + "id: " + i + ","
                        + "text: \"" + ProcessTextForSerialization(option.Text) + "\","
                        + "mute: " + (option.MuteOption ? "true" : "false") + ","
                        + "time:" + (option.Time.HasValue ? option.Time.Value : -1).ToString(System.Globalization.CultureInfo.InvariantCulture)
                        + "}";

                    otherOptionsAvailable = true;
                }
            }

            invokeString += "]," + Serialize(new LocationInfo(codebaseDB.GetFilename(node.GetDialogue()), node.SourceLineStart, node.SourceLineEnd)) + ");";

            if (otherOptionsAvailable)
                Uno.Foundation.WebAssemblyRuntime.InvokeJS(invokeString);
            else
            {
                context.Choose(null);
            }
        }

        static void OnChoiceEnd(IDialogueContext context, IChoosableNode node) 
        {
            Uno.Foundation.WebAssemblyRuntime.InvokeJS("onChoiceEnd(" + context.Uid + ")");
        }

        static void OnWaitTimeStart(IDialogueContext context, WaitTimeNode node) 
        {
            Uno.Foundation.WebAssemblyRuntime.InvokeJS("onWaitTimeStart(" + context.Uid + ", " + node.Time + "," + Serialize(new LocationInfo(codebaseDB.GetFilename(node.GetDialogue()), node.SourceLineStart, node.SourceLineEnd)) + ");");
        }

        static void OnWaitTimeEnd(IDialogueContext context, WaitTimeNode node) 
        {
            Uno.Foundation.WebAssemblyRuntime.InvokeJS("onWaitTimeEnd(" + context.Uid + ")");
        }

        static void OnSpeechOptionStart(IDialogueContext context, Option option)
        {
            Uno.Foundation.WebAssemblyRuntime.InvokeJS("onSpeechOptionStart(" + context.Uid + ", \"" + option.Parent.CharacterId + "\", \"" + ProcessTextForSerialization(option.Text) + "\"," + Serialize(new LocationInfo(codebaseDB.GetFilename(option.Parent.GetDialogue()), option.SourceLineStart, option.SourceLineEnd)) + ");");
        } 

        static void OnChallengeStart(IDialogueContext context, IContent checkable, string name)
        {
            Uno.Foundation.WebAssemblyRuntime.InvokeJS("onChallengeStart(" + context.Uid + ", \"" + name + "\"," + Serialize(new LocationInfo(codebaseDB.GetFilename(checkable.GetDialogue()), checkable.SourceLineStart, checkable.SourceLineEnd)) + ")");
        } 

        static void OnWaitForMissingDialogueStart(IDialogueContext context, string name, IDialogueNode causingNode)
        {
            Uno.Foundation.WebAssemblyRuntime.InvokeJS("onWaitForMissingDialogueStart(" + context.Uid + ", \"" + name + "\"," + Serialize(new LocationInfo(codebaseDB.GetFilename(causingNode.GetDialogue()), causingNode.SourceLineStart, causingNode.SourceLineEnd)) + ")");
        }

        static void OnWaitForMissingDialogueEnd(IDialogueContext context, string name)
        {
            Uno.Foundation.WebAssemblyRuntime.InvokeJS("onWaitForMissingDialogueEnd(" + context.Uid + ", \""+ name + "\")");
        }

        static void OnBoolDataChanged(string context, string name, bool prevValue, bool value)
        {
            string stringValue = value ? "true" : "false";
            Uno.Foundation.WebAssemblyRuntime.InvokeJS($"onBoolDataChanged(\"{context}\", \"{name}\", {stringValue})");
        }

        static void OnIntDataChanged(string context, string name, long prevValue, long value)
        {
            Uno.Foundation.WebAssemblyRuntime.InvokeJS($"onIntDataChanged(\"{context}\", \"{name}\", {value.ToString(System.Globalization.CultureInfo.InvariantCulture)})");
        }

        static void OnSymbolDataChanged(string context, string name, string prevValue, string value)
        {
            Uno.Foundation.WebAssemblyRuntime.InvokeJS($"onSymbolDataChanged(\"{context}\", \"{name}\", \"{value.ToString()}\")");
        }

        static void OnLocalBoolDataChanged(IDialogueContext context, string name, bool prevValue, bool value)
        {
            string stringValue = value ? "true" : "false";
            Uno.Foundation.WebAssemblyRuntime.InvokeJS($"onLocalBoolDataChanged({context.Uid}, \"{name}\", {stringValue})");
        }
        
        static void OnLocalIntDataChanged(IDialogueContext context, string name, long prevValue, long value)
        {
            string stringValue = value.ToString();
            Uno.Foundation.WebAssemblyRuntime.InvokeJS($"onLocalIntDataChanged({context.Uid}, \"{name}\", {value.ToString(System.Globalization.CultureInfo.InvariantCulture)})");
        }
        
        static void OnLocalSymbolDataChanged(IDialogueContext context, string name, string prevValue, string value)
        {
            string stringValue = value.ToString();
            Uno.Foundation.WebAssemblyRuntime.InvokeJS($"onLocalSymbolDataChanged({context.Uid}, \"{name}\", \"{value.ToString()}\")");
        }

        static void OnDataClear(string context, string name)
        {
            Uno.Foundation.WebAssemblyRuntime.InvokeJS($"onDataClear(\"{context}\", \"{name}\")");
        }

        static void OnContextClear(string context)
        {
            Uno.Foundation.WebAssemblyRuntime.InvokeJS($"onContextClear(\"{context}\")");
        }

        static void OnLocalDataClear(IDialogueContext context, string name)
        {
            Uno.Foundation.WebAssemblyRuntime.InvokeJS($"onLocalDataClear({context.Uid}, \"{name}\")");
        }

        static void OnLocalContextClear(IDialogueContext context)
        {
            Uno.Foundation.WebAssemblyRuntime.InvokeJS($"onLocalContextClear({context.Uid})");
        }
        
        static void OnError(IDialogueContext context, string message)
        {
            Uno.Foundation.WebAssemblyRuntime.InvokeJS($"onError({context.Uid}, \"{message}\")");
        }

        static void Main(string[] args) 
        {
            Console.WriteLine("SamwiseWASM loaded.");
            
            Uno.Foundation.WebAssemblyRuntime.InvokeJS("onExtensionStarted()");
        }

        static string ProcessTextForSerialization(string text)
        {
            return text.Replace("\n", "↵").Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        static string Serialize(LocationInfo info)
        {
            return "{\"file\": \"" + ProcessTextForSerialization(info.file) + "\", \"lineStart\":"+ info.lineStart +", \"lineEnd\": " + info.lineEnd + "}";
        }

        static Dictionary<string, ParsedDialogueFile> fileToDialogues = new Dictionary<string, ParsedDialogueFile>();
        static CodebaseDatabase codebaseDB = new CodebaseDatabase();
        static DialogueMachine dialogueMachine;
        static DataRoot sandboxDataRoot = new DataRoot();

        static long uid;
    }
}