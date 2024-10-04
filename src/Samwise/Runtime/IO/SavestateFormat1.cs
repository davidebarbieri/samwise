// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

using System;
using System.Collections.Generic;
using System.IO;

namespace Peevo.Samwise
{
    public partial class DialogueMachine
    {
        public class SavestateFormat1: ISavestateFormat
        {
            public int Version => 1;

            public SavestateFormat1()
            {
                unindexedParser = new SamwiseParser(new DummyCodeParser());
            }

            public bool SaveState(BinaryWriter writer, DialogueMachine machine, IExternalContextSaveResolver externalContextResolver)
            {
                writer.Write(machine.rootRunningContexes.Count);

                HashSet<Dialogue> alreadySerializedNonIndexed = new HashSet<Dialogue>();

                bool res = true;
                foreach (var context in machine.rootRunningContexes)
                {
                    res = res && SaveStateRecursive(context, writer, machine, externalContextResolver, alreadySerializedNonIndexed);
                }

                // Save global data
                SaveGlobalContext(writer, machine.dataRoot);

                return res;
            }


            public bool LoadState(BinaryReader reader, DialogueMachine machine, System.Func<string, Dialogue> onUnresolvedDialogue, IExternalContextLoadResolver externalContextResolver)
            {
                var runningContexes = reader.ReadInt32();

                Dictionary<string, Dialogue> nonIndexedDialogues = new Dictionary<string, Dialogue>();

                bool res = true;
                for (int i=0; i<runningContexes; ++i)
                {
                    // read other state
                    DialogueContext dialogueContext = null;
                    res = res && LoadStateRecursively(out dialogueContext, null, reader, machine, externalContextResolver, onUnresolvedDialogue, nonIndexedDialogues);

                    if (res)
                        machine.rootRunningContexes.Add(dialogueContext);
                }
                
                // Load global data
                LoadGlobalContext(reader, machine.dataRoot);

                return res;
            }

            static bool SaveStateRecursive(DialogueContext context, BinaryWriter writer, DialogueMachine machine, IExternalContextSaveResolver externalContextResolver, HashSet<Dialogue> alreadySerializedNonIndexed)
            {
                bool isLocked = machine.lockedContexes.Contains(context);
                writer.Write((long)context.Uid);
                writer.Write((bool)isLocked);
                writer.Write((string)context.BranchName ?? "");
                writer.Write((string)(externalContextResolver == null || context.ExternalContext == null ? "" : externalContextResolver.GetUIDFromObject(context.ExternalContext)));
                
                context.GetInternalState(out DialogueStatus status, out IDialogueNode reenterNode, out IDialogueNode forkedFromNode, 
                    out IOption challengedOption, out DialogueContext waitingForJoin);

                // we save forked node before any other node, as if that is not resolved, then all the other nodes must be taken from the
                // embedded dialogue
                SaveNodeReference(writer, forkedFromNode, machine, alreadySerializedNonIndexed);
                // save current node
                SaveNodeReference(writer, context.Current, machine, alreadySerializedNonIndexed);

                // save local data
                SaveVariables(writer, context.DataContext);

                // save children
                writer.Write(context.Children.Count);
                foreach (var child in context.Children)
                {
                    SaveStateRecursive(child, writer, machine, externalContextResolver, alreadySerializedNonIndexed);
                }

                // save internal state
                writer.Write((byte)status);
                writer.Write((long)(waitingForJoin == null ? -1 : waitingForJoin.Uid));
                SaveNodeReference(writer, reenterNode, machine, alreadySerializedNonIndexed);
                SaveOptionReference(writer, challengedOption, machine, alreadySerializedNonIndexed);

                return true;
            }

            bool LoadStateRecursively(out DialogueContext loadedContext, DialogueContext parent, BinaryReader reader, DialogueMachine machine, IExternalContextLoadResolver externalContextResolver, System.Func<string, Dialogue> onUnresolvedDialogue, Dictionary<string, Dialogue> nonIndexedDialogues)
            {
                var uid = reader.ReadInt64();
                var locked = reader.ReadBoolean();
                var branchName = reader.ReadString(); if (branchName == "") branchName = null;
                var externalContextUid = reader.ReadString();

                loadedContext = machine.CreateContext();
                loadedContext.Uid = uid;

                // read current node
                (IDialogueNode forkedFromNode, bool wasForkEmbedded) = LoadNodeReference(reader, machine, onUnresolvedDialogue, nonIndexedDialogues);
                (IDialogueNode currentNode, bool wasCurrentEmbedded) = LoadNodeReference(reader, machine, onUnresolvedDialogue, nonIndexedDialogues);

                loadedContext.InitializeStarted(branchName, currentNode, parent, string.IsNullOrEmpty(externalContextUid) ? null : externalContextResolver.GetObjectFromUID(externalContextUid));

                if (locked)
                    machine.lockedContexes.Add(loadedContext);

                // read local data
                if (loadedContext.DataContext != null)
                    LoadVariables(reader, loadedContext.DataContext);

                // read children
                var children = reader.ReadInt32();

                bool res = true;
                for (int i=0; i<children; ++i)
                {
                    res = res && LoadStateRecursively(out _, loadedContext, reader, machine, externalContextResolver, onUnresolvedDialogue, nonIndexedDialogues);
                }

                // read internal state
                var status = (DialogueStatus)reader.ReadByte(); 
                long joiningUid = reader.ReadInt64();
                DialogueContext waitingForJoin = joiningUid >= 0 ? machine.uidToDialogue[joiningUid] : null;
                (IDialogueNode reenterNode, bool wasReenterEmbedded) = LoadNodeReference(reader, machine, onUnresolvedDialogue, nonIndexedDialogues);
                (IOption waitingOption, bool wasChallengeEmbedded) = LoadOptionReference(reader, machine, onUnresolvedDialogue, nonIndexedDialogues); 

                loadedContext.SetInternalState(status, reenterNode, forkedFromNode, waitingOption, waitingForJoin);

                return res;
            }


            static void SaveGlobalContext(BinaryWriter writer, IDataRoot dataRoot)
            {
                var dataContext = dataRoot.LookupDataContext(".");
                SaveGlobalContext(writer, dataRoot, dataContext);
            }

            static void SaveGlobalContext(BinaryWriter writer, IDataRoot dataRoot, IDataContext dataContext)
            {
                writer.Write(dataContext?.Name ?? "");

                if (dataContext == null)
                    return;

                SaveVariables(writer, dataContext);

                var subContexes = dataRoot.GetSubcontexes(dataContext);
                writer.Write(subContexes.Count);
                
                foreach (var subContext in subContexes)
                    SaveGlobalContext(writer, dataRoot, subContext);
            }

            static void LoadGlobalContext(BinaryReader reader, IDataRoot dataRoot)
            {
                var name = reader.ReadString();

                if (string.IsNullOrEmpty(name))
                    return;

                var dataContext = dataRoot.LookupOrCreateDataContext(name);

                LoadVariables(reader, dataContext);

                var count = reader.ReadInt32();
                
                for (int i=0; i<count; ++i) 
                    LoadGlobalContext(reader, dataRoot);
            }

            static void SaveVariables(BinaryWriter writer, IDataContext dataContext)
            {
                writer.Write((int)dataContext.BoolVariablesCount);
                foreach ((string, bool) d in dataContext.GetBoolVariables())
                {
                    writer.Write(d.Item1);
                    writer.Write(d.Item2);
                }
                
                writer.Write((int)dataContext.IntVariablesCount);
                foreach ((string, long) d in dataContext.GetIntVariables())
                {
                    writer.Write(d.Item1);
                    writer.Write(d.Item2);
                }
                
                writer.Write((int)dataContext.SymbolVariablesCount);
                foreach ((string, string) d in dataContext.GetSymbolVariables())
                {
                    writer.Write(d.Item1);
                    writer.Write(d.Item2);
                }
            }

            static void LoadVariables(BinaryReader reader, IDataContext dataContext)
            {
                int boolVariablesCount = reader.ReadInt32();
                for (int i=0; i<boolVariablesCount; ++i) 
                {
                    string name = reader.ReadString();
                    bool value = reader.ReadBoolean();
                    
                    dataContext.SetValueBool(name, value);
                }
                
                int intVariablesCount = reader.ReadInt32();
                for (int i=0; i<intVariablesCount; ++i) 
                {
                    string name = reader.ReadString();
                    long value = reader.ReadInt64();
                    
                    dataContext.SetValueInt(name, value);
                }
                
                int symbolVariablesCount = reader.ReadInt32();
                for (int i=0; i<symbolVariablesCount; ++i) 
                {
                    string name = reader.ReadString();
                    string value = reader.ReadString();
                    
                    dataContext.SetValueSymbol(name, value);
                }
            }

            static void SaveNodeReference(BinaryWriter writer, IDialogueNode node, DialogueMachine machine, HashSet<Dialogue> alreadySerializedNonIndexed)
            {
                if (node == null)
                {
                    writer.Write((string)"");
                    return;
                }

                var dialogue = node.GetDialogue();
                writer.Write((string)dialogue.Label);

                var id = node.GetID();

                bool hasId = !string.IsNullOrEmpty(id);

                writer.Write((bool)hasId);

                if (hasId)
                {
                    writer.Write((string)id);
                }
                else
                {
                    if (alreadySerializedNonIndexed.Add(dialogue))
                    {
                        writer.Write((string)dialogue.ToString());
                    }
                    writer.Write((int)node.SourceLineStart);
                }
            }

            (IDialogueNode, bool) LoadNodeReference(BinaryReader reader, DialogueMachine machine, System.Func<string, Dialogue> onUnresolvedDialogue, Dictionary<string, Dialogue> nonIndexedDialogues)
            {
                string dialogueLabel = reader.ReadString();

                if (string.IsNullOrEmpty(dialogueLabel))
                    return (null, false);
            
                if (!machine.Dialogues.GetDialogue(dialogueLabel, out var dialogue))
                {
                    dialogue = onUnresolvedDialogue(dialogueLabel);

                    if (dialogue == null)
                        throw new IOException("Unable to resolve the dialogue named " + dialogueLabel + " while loading.");
                }

                bool hasNodeId = reader.ReadBoolean();

                if (hasNodeId)
                {
                    string id = reader.ReadString();

                    var node = dialogue.FindNodeFromId(id);

                    if (node == null)
                        throw new IOException("Unable to find the node " + id + " in dialogue " + dialogueLabel);

                    return (node, false);
                }
                else
                {
                    if (!nonIndexedDialogues.TryGetValue(dialogueLabel, out var embeddedDialogue))
                    {
                        var dialogueText = reader.ReadString();
                        int pos = 0;
                        int line = 1;
                        if (!unindexedParser.ParseDialogue(dialogueText, ref pos, ref line, out embeddedDialogue))
                        {
                            throw new IOException("Unable to parse non-indexed dialogue the node");
                        }

                        nonIndexedDialogues[dialogueLabel] = embeddedDialogue;
                    }

                    var lineId = reader.ReadInt32();
                    // get the node in the embedded dialogue
                    var embeddedNode = embeddedDialogue.FindNodeFromLine(lineId);
                    var dialogueNode = dialogue.FindNodeFromLine(lineId);

                    // find the most probable fallback
                    var mostProbableNode = FindMostProbableFallbackNode(embeddedNode, embeddedDialogue, dialogueNode, dialogue);
                    if (mostProbableNode != null)
                        return (mostProbableNode, false);
                        
                    // Can't find fallback option, stay on embedded dialogue
                    machine.Log("Warning: unable to find the saved node, the dialogue is continuing on saved dialogue version");
                    return (embeddedNode, true);
                }
            }

            static void SaveOptionReference(BinaryWriter writer, IOption option, DialogueMachine machine, HashSet<Dialogue> alreadySerializedNonIndexed)
            {
                if (option == null)
                {
                    writer.Write((string)"");
                    return;
                }

                var dialogue = option.GetDialogue();
                writer.Write((string)dialogue.Label);

                var id = option.GetID();

                bool hasId = !string.IsNullOrEmpty(id);

                writer.Write((bool)hasId);

                if (hasId)
                {
                    writer.Write((string)id);
                }
                else
                {
                    if (alreadySerializedNonIndexed.Add(dialogue))
                    {
                        writer.Write((string)dialogue.ToString());
                    }
                    writer.Write((int)option.SourceLineStart);
                }

            }
            
            (IOption, bool) LoadOptionReference(BinaryReader reader, DialogueMachine machine, System.Func<string, Dialogue> onUnresolvedDialogue, Dictionary<string, Dialogue> nonIndexedDialogues)
            {
                string dialogueLabel = reader.ReadString();
            
                if (string.IsNullOrEmpty(dialogueLabel))
                    return (null, false);

                if (!machine.Dialogues.GetDialogue(dialogueLabel, out var dialogue))
                {
                    dialogue = onUnresolvedDialogue(dialogueLabel);

                    if (dialogue == null)
                        throw new IOException("Unable to resolve the dialogue named " + dialogueLabel + " while loading.");
                }

                bool hasOptionId = reader.ReadBoolean();

                if (hasOptionId)
                {
                    string id = reader.ReadString();

                    var option = dialogue.FindOptionFromId(id);

                    if (option == null)
                        throw new IOException("Unable to find the option " + id + " in dialogue " + dialogueLabel);

                    return (option, false);
                }
                else
                {
                    machine.Log("Warning: saved option has no unique ID set, finding a fallback");
                    if (!nonIndexedDialogues.TryGetValue(dialogueLabel, out var embeddedDialogue))
                    {
                        var dialogueText = reader.ReadString();
                        int pos = 0;
                        int line = 0;
                        if (!unindexedParser.ParseDialogue(dialogueText, ref pos, ref line, out embeddedDialogue))
                        {
                            throw new IOException("Unable to parse non-indexed dialogue the node");
                        }

                        nonIndexedDialogues[dialogueLabel] = embeddedDialogue;
                    }

                    var lineId = reader.ReadInt32();
                    // get the option in the embedded dialogue
                    var embeddedOption = (Option)embeddedDialogue.FindOptionFromLine(lineId);
                    var dialogueOption = (Option)dialogue.FindOptionFromLine(lineId);

                    var embeddedNode = (ChoiceNode)embeddedOption.Parent;

                    if (embeddedOption.OptionGroup.GroupId == dialogueOption.OptionGroup.GroupId &&
                        embeddedOption.Text == dialogueOption.Text &&
                        CheckSameCheck(embeddedOption, dialogueOption)) // Same group id, text and check
                        return (dialogueOption, false);

                    // if same group id, search same text and check within it
                    if (embeddedOption.OptionGroup.GroupId == dialogueOption.OptionGroup.GroupId &&
                        FindOptionWithSameTextAndCheck(embeddedOption, dialogueOption.OptionGroup, out var sameTextOption))
                    {
                        return (sameTextOption, false);
                    }

                    IDialogueNode dialogueNode;
                    if (dialogueOption != null)
                        dialogueNode = dialogueOption.Parent;
                    else
                        dialogueNode = dialogue.FindNodeFromLine(lineId);

                    var mostProbableFallback = FindMostProbableFallbackNode(embeddedNode, embeddedDialogue, dialogueNode, dialogue) as ChoiceNode;

                    if (mostProbableFallback != null)
                    {
                        var fallbackOption = FindMostProbableFallbackOption(embeddedOption, embeddedNode, mostProbableFallback);
                        if (fallbackOption != null)
                            return (fallbackOption, false);
                    }

                    // Can't find fallback option, stay on embedded dialogue
                    machine.Log("Warning: unable to find the saved challenged option, the dialogue is continuing on saved dialogue version");
                    return (embeddedOption, true);
                }
            }

            private bool FindOptionWithSameTextAndCheck(Option embeddedOption, OptionGroup optionGroup, out Option sameTextOption)
            {
                for (int i=0; i<optionGroup.OptionsCount; ++i)
                {
                    var option = optionGroup.GetOption(i);

                    if (CheckSameCheck(embeddedOption, option) &&
                        embeddedOption.Text == option.Text)
                        {
                            sameTextOption = option;
                            return true;
                        }
                }

                sameTextOption = null;
                return false;
            }

            private Option FindMostProbableFallbackOption(Option embeddedOption, ChoiceNode embeddedNode, ChoiceNode mostProbableFallback)
            {
                // to be noted that we cannot fallback on an option without a check (even if it has the same text), as it would break the machine
                
                var embeddedGroup = embeddedOption.OptionGroup;
                int embeddedGroupId = embeddedGroup.GroupId;

                var optionGroupSameId = mostProbableFallback.GetOptionGroup(embeddedGroupId);

                // search same id, text and check
                if (optionGroupSameId != null && FindOptionWithSameTextAndCheck(embeddedOption, optionGroupSameId, out var sameTextOption))
                    return sameTextOption;

                // search same text and check
                for (int i=0; i<mostProbableFallback.OptionsGroupsCount; ++i)
                {
                    var optionGroup = mostProbableFallback.GetOptionGroup(i);

                    if (FindOptionWithSameTextAndCheck(embeddedOption, optionGroup, out sameTextOption))
                        return sameTextOption;
                }

                // search same previous and next text and check
                var embeddedPrevious = GetPrevious(embeddedOption, embeddedNode);
                var embeddedNext = GetNext(embeddedOption, embeddedNode);

                if (FindOptionWithSamePrevAndNextAndCheck(embeddedPrevious, embeddedNext, mostProbableFallback, out var samePrevNextOption))
                    return samePrevNextOption;

                return null;
            }

            Option GetPrevious(Option embeddedOption, ChoiceNode embeddedNode)
            {
                var embeddedGroup = embeddedOption.OptionGroup;
                int embeddedGroupId = embeddedGroup.GroupId;

                OptionGroup prevGroup;
                return embeddedOption.IdWithinGroup > 0 ? embeddedGroup.GetOption(embeddedOption.IdWithinGroup - 1) :
                    embeddedGroupId == 0 ? null :
                    (prevGroup = embeddedNode.GetOptionGroup(embeddedGroupId - 1)).GetOption(prevGroup.OptionsCount - 1);
            }

            Option GetNext(Option embeddedOption, ChoiceNode embeddedNode)
            {
                var embeddedGroup = embeddedOption.OptionGroup;
                int embeddedGroupId = embeddedGroup.GroupId;
                
                OptionGroup nextGroup;
                return embeddedOption.IdWithinGroup < embeddedGroup.OptionsCount - 1 ? 
                    embeddedGroup.GetOption(embeddedOption.IdWithinGroup + 1) :
                    embeddedGroupId == embeddedNode.OptionsGroupsCount - 1 ? null :
                    (nextGroup = embeddedNode.GetOptionGroup(embeddedGroupId + 1)).GetOption(0);
            }

            bool FindOptionWithSamePrevAndNextAndCheck(Option embeddedPrev, Option embeddedNext, ChoiceNode mostProbableFallback, out Option mostProbableOption)
            {
                for (int i=0; i<mostProbableFallback.OptionsGroupsCount; ++i)
                {
                    var group = mostProbableFallback.GetOptionGroup(i);

                    for (int ii=0; ii< group.OptionsCount; ++ii)
                    {
                        var option = group.GetOption(ii);
                        var prevOption = GetPrevious(option, mostProbableFallback);
                        var nextOption = GetNext(option, mostProbableFallback);

                        if (prevOption != null && embeddedPrev != null && prevOption.Text == embeddedPrev.Text && CheckSameCheck(prevOption, embeddedPrev))
                        {
                            mostProbableOption = option;
                            return true;
                        }    

                        if (nextOption != null && embeddedNext != null && nextOption.Text == embeddedNext.Text && CheckSameCheck(nextOption, embeddedNext))
                        {
                            mostProbableOption = option;
                            return true;
                        }  
                    }
                }

                mostProbableOption = null;
                return false;
            }

            // Fallbacks should not be a reliable method for a release build, apply UIDs to every node instead
            // This is just a largerly improvable heuristic
            private IDialogueNode FindMostProbableFallbackNode(IDialogueNode embeddedNode, Dialogue embeddedDialogue, IDialogueNode samelineDialogueNode, Dialogue dialogue)
            {
                // search node with same uid first
                var embeddedID = embeddedNode.GetID();

                if (!string.IsNullOrEmpty(embeddedID))
                {
                    var node = dialogue.FindNodeFromId(embeddedID);

                    if (node != null)
                        return node;
                }

                // check same line first
                if (samelineDialogueNode != null && samelineDialogueNode.GetType() == embeddedNode.GetType())
                {
                    var sameLineText = samelineDialogueNode.ToString();
                    var embeddedLineText = embeddedNode.ToString();

                    if (sameLineText == embeddedLineText)
                        return samelineDialogueNode;
                }

                // Find exact same position signature and same content
                IDialogueNode sameSignatureNode = FindNodeWithSameSignature(embeddedNode, dialogue);
                if (sameSignatureNode != null)
                {
                    if (embeddedNode is ITextContent embeddedTextContent)
                    {
                        // same text, but different check (but still present)
                        ITextContent textContent = sameSignatureNode as ITextContent;
                        if (embeddedTextContent.Text == textContent.Text && 
                            string.IsNullOrEmpty(embeddedNode.PreCheck) == string.IsNullOrEmpty(sameSignatureNode.PreCheck))
                            return sameSignatureNode;
                    }
                    else if (sameSignatureNode.ToString() == embeddedNode.ToString() && 
                            string.IsNullOrEmpty(embeddedNode.PreCheck) == string.IsNullOrEmpty(sameSignatureNode.PreCheck))
                        return sameSignatureNode;
                }

                // Find same text (+ check)
                if (samelineDialogueNode != null && samelineDialogueNode.GetType() == embeddedNode.GetType())
                {
                    if (embeddedNode is ITextContent embeddedTextContent)
                    {
                        foreach (var t in dialogue.Traverse(embeddedNode.GetType()))
                        {
                            // same text, but different check (but still present)
                            ITextContent textContent = t as ITextContent;
                            if (embeddedTextContent.Text == textContent.Text && 
                                string.IsNullOrEmpty(embeddedNode.PreCheck) == string.IsNullOrEmpty(t.PreCheck))
                                return t;
                        }
                    }
                }

                // Find exact same position signature and check
                if (sameSignatureNode != null)
                {
                    if (string.IsNullOrEmpty(embeddedNode.PreCheck) == string.IsNullOrEmpty(sameSignatureNode.PreCheck))
                        return sameSignatureNode;
                }

                // same line, type and check
                if (!string.IsNullOrEmpty(embeddedNode.PreCheck))
                {
                    if (samelineDialogueNode != null && 
                        samelineDialogueNode.GetType() == embeddedNode.GetType() &&
                        samelineDialogueNode.PreCheck != embeddedNode.PreCheck)
                    {
                        return samelineDialogueNode;
                    }

                }
                
                return null;
            }

            IDialogueNode FindNodeWithSameSignature(IDialogueNode embeddedNode, Dialogue dialogue)
            {
                int embeddedId = CheckSameTypeId(embeddedNode);
                IDialogueBlock sameSignatureBlock = FindBlockWithSameSignature(embeddedNode.Block, dialogue);

                int id = 0;
                for (int i=0, count=sameSignatureBlock.ChildrenCount; i<count; ++i)
                {
                    var sibling = sameSignatureBlock.GetChild(i); 

                    if (sibling.GetType() == embeddedNode.GetType())
                    {
                        if (embeddedId == id++)
                            return sibling;
                    }
                }

                return null;
            }

            IDialogueBlock FindBlockWithSameSignature(IDialogueBlock embeddedBlock, Dialogue dialogue)
            {
                if (embeddedBlock.Parent == null)
                    return dialogue;

                int embeddedId = CheckSameTypeId(embeddedBlock);
                var sameSignatureNode = FindNodeWithSameSignature(embeddedBlock.Parent, dialogue) as IBlockContainerNode;

                int id = 0;
                for (int i=0, count=sameSignatureNode.ChildrenCount; i<count; ++i)
                {
                    var sibling = sameSignatureNode.GetChild(i); 

                    if (embeddedId == id++)
                        return sibling;
                }

                return null;
            }

            static bool CheckSameCheck(ICheckable a, ICheckable b)
            {
                bool hasACheck = a.HasCheck(out var isPreCheckA, out var checkNameA);
                bool hasBCheck = b.HasCheck(out var isPreCheckB, out var checkNameB);

                if (!hasACheck && !hasBCheck)
                    return true;

                return hasACheck && hasBCheck && checkNameA == checkNameB;

            }

            int CheckSameTypeId(IDialogueNode node)
            {
                var parent = node.Block;

                int id = 0;
                for (int i=0, count=parent.ChildrenCount; i<count; ++i)
                {
                    var sibling = parent.GetChild(i); 

                    if (sibling == node)
                            return id;

                    if (sibling.GetType() == node.GetType())
                        ++id;
                }

                return -1;
            }
            
            int CheckSameTypeId(IDialogueBlock block)
            {
                var parent = block.Parent;

                if (parent == null)
                    return -1;

                for (int i=0, count=parent.ChildrenCount; i<count; ++i)
                {
                    var sibling = parent.GetChild(i); 

                    if (sibling == block)
                            return i;
                }

                return -1;
            }

            SamwiseParser unindexedParser;
        }
    }
}