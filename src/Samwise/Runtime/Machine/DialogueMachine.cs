// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

using System;
using System.Collections.Generic;
using System.IO;

namespace Peevo.Samwise
{
    public partial class DialogueMachine
    {
        public event System.Action<IDialogueContext, SpeechNode> onSpeechStart;
        public event System.Action<IDialogueContext, SpeechNode> onSpeechEnd;

        public event System.Action<IDialogueContext, CaptionNode> onCaptionStart;
        public event System.Action<IDialogueContext, CaptionNode> onCaptionEnd;

        public event System.Action<IDialogueContext, IChoosableNode> onChoiceStart;
        public event System.Action<IDialogueContext, IChoosableNode> onChoiceEnd;
        public event System.Action<IDialogueContext, WaitTimeNode> onWaitTimeStart;
        public event System.Action<IDialogueContext, WaitTimeNode> onWaitTimeEnd;
        public event System.Action<IDialogueContext, IOption> onSpeechOptionStart;
        public event System.Action<IDialogueContext, IContent, string> onChallengeStart;
        public event System.Action<IDialogueContext> onDialogueContextStart;
        public event System.Action<IDialogueContext> onDialogueContextStop;
        public event System.Action<IDialogueContext, Dialogue, Dialogue> onDialogueChange;
        public event System.Action<IDialogueContext> onDialogueContextEnd;

        public event System.Action<IDialogueContext, string, IDialogueNode> onWaitForMissingDialogueStart;
        public event System.Action<IDialogueContext, string> onWaitForMissingDialogueEnd;

        public IDialogueSet Dialogues { get; private set; }

        public int RunningContexesCount => runningContexes.Count;
        public int RootRunningContexesCount => rootRunningContexes.Count;

        public IReadOnlyList<IDialogueContext> RootRunningContexes => rootRunningContexes;
        public IReadOnlyList<IDialogueContext> RunningContexes => runningContexes;

        public System.Action<string> overrideLogger;

        public DialogueMachine(IDialogueSet dialogues, IDataRoot dataRoot, IExternalCodeMachine externalCodeMachine = null)
        {
            this.Dialogues = dialogues;
            this.dataRoot = dataRoot;
            this.externalCodeMachine = externalCodeMachine;
        }

        public IDialogueContext Start(Dialogue dialogue, object externalContext = null, IDialogueContext reuseContext = null)
        {
            if (dialogue == null || dialogue.ChildrenCount == 0)
                return null;

            return Start(dialogue.GetChild(0), externalContext, reuseContext);
        }

        public IDialogueContext Start(IDialogueNode node, object externalContext = null, IDialogueContext reuseContext = null)
        {
            if (node == null)
                return null;

            var context = reuseContext as DialogueContext;
            if (context != null)
            {
                context.Uid = ++lastContexUid;
                context.Clear();
            }

            var root = context ?? CreateContext();
            rootRunningContexes.Add(root);
            root.Start(null, node, null, externalContext);
            return root;
        }

        public void StopAll()
        {
            for (int i = rootRunningContexes.Count - 1; i >= 0; --i)
            {
                rootRunningContexes[i].Stop();
            }
            rootRunningContexes.Clear();
            runningContexes.Clear();
            lockedContexes.Clear();
        }

        public void Update()
        {
            // Check locked contexes in reverse order, so we'll skip newly locked ones
            for (int i = lockedContexes.Count - 1; i >= 0; --i)
            { 
                if (lockedContexes[i].Advance())
                {
                    lockedContexes.RemoveAt(i);
                }
            }
        }

        // Export full state (including data)
        // external resolver is used only if you use external contexes
        public bool SaveState(BinaryWriter writer, IExternalContextSaveResolver externalContextResolver = null, ISavestateFormat format = null)
        {
            if (format == null)
                format = defaultSaveFormat;

            writer.Write(format.Version);
            return format.SaveState(writer, this, externalContextResolver);
        }

        // Import full state (including data)
        // external resolver is used only if you use external contexes
        // onUnresolvedDialogue is called when, while loading, a referenced dialogue is not found (and must be loaded)
        public bool LoadState(BinaryReader reader, System.Func<string, Dialogue> onUnresolvedDialogue, IExternalContextLoadResolver externalContextResolver = null)
        {
            StopAll();
            dataRoot.Clear();

            var format = reader.ReadInt32();

            // TODO: put here format lookup, when we'll have more formats
            if (format != 1)
                return false;

            return defaultSaveFormat.LoadState(reader, this, onUnresolvedDialogue, externalContextResolver);
        }

        // Possibly useful for external serialization
        public IDialogueContext LookupDialogue(long uid)
        {
            if (uidToDialogue.TryGetValue(uid, out var dialogue))
                return dialogue;
            return null;
        }

        // Possibly useful for external serialization
        public IExternalCodeContext LookupExternalContext(long uid)
        {
            if (uidToDialogue.TryGetValue(uid, out var dialogue))
                return dialogue;
            return null;
        }

        void OnLocked(DialogueContext dialogueContext)
        {
            lockedContexes.Add(dialogueContext);
        }

        void OnDialogueContextEnd(DialogueContext dialogueContext)
        {
            runningContexes.Remove(dialogueContext);
            rootRunningContexes.Remove(dialogueContext);
            onDialogueContextEnd?.Invoke(dialogueContext);
        }

        void OnDialogueContextStop(DialogueContext dialogueContext)
        {
            runningContexes.Remove(dialogueContext);
            rootRunningContexes.Remove(dialogueContext);
            onDialogueContextStop?.Invoke(dialogueContext);
        }

        DialogueContext CreateContext()
        {
            // First, I thought it was a good idea to have a pool of dialogues,
            // then, I removed it as I prefer users can safely retain IDialogueContext references
            // and be sure that a Stopped context cannot return magically to Running status

            // Instead of implementing pool, allow users reuse DialogueContexts, so users can implement their own pool instead
            long uid = ++lastContexUid;
            var dialogue = new DialogueContext(this, uid);
            uidToDialogue[uid] = dialogue;
            return dialogue;
        }
        
        Dialogue Resolve(string destinationDialogueId)
        {
            if (!Dialogues.GetDialogue(destinationDialogueId, out var dialogue))
                return null;

            return dialogue;
        }

        void Log(string message)
        {
            if (overrideLogger != null)
                overrideLogger(message);
            else
                Console.WriteLine(message);
        }

        IDataRoot dataRoot;
        IExternalCodeMachine externalCodeMachine;
        long lastContexUid;

        List<DialogueContext> runningContexes = new List<DialogueContext>();
        List<DialogueContext> rootRunningContexes = new List<DialogueContext>();
        List<DialogueContext> lockedContexes = new List<DialogueContext>();
        Dictionary<long, DialogueContext> uidToDialogue = new Dictionary<long, DialogueContext>();

        // Put here newer formats later
        static ISavestateFormat defaultSaveFormat = new SavestateFormat1();
    }
}