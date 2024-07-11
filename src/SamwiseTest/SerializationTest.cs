using System.Diagnostics;
using Xunit;

namespace Peevo.Samwise.Test
{
    public class SerializationTest : BaseTest
    {
        const string SAVED_STRING = "== SAVED ==";
        const string CHOICE_STRING = "== CHOICE START ==";

        [Fact]
        public void SaveBasic()
        {
            var output = TestSaveAndReload(
@"character> This is a test!
{ bVar = true }
{ Global.iVar = 5 }
other> Yeah, I know!
* it was really a test
[bVar] * and this was true
[Global.iVar > 4] * and this too
", 1);

            TestResults(output,
                (a) => Assert.Equal("character said \"This is a test!\"", a),
                (a) => Assert.Equal(SAVED_STRING, a),
                (a) => Assert.Equal("other said \"Yeah, I know!\"", a),
                (a) => Assert.Equal("[ it was really a test ]", a),
                (a) => Assert.Equal("[ and this was true ]", a),
                (a) => Assert.Equal("[ and this too ]", a)
                );
        }
        
        [Fact]
        public void SaveAmidLoop()
        {
            var output = TestSaveAndReload(
@"character> This is a test!
{ iLoops = 5 }
(restart) character> Yeaah
{ iLoops = iLoops - 1 }
[iLoops > 0] -> restart
character> End!
", 4);

            TestResults(output,
                (a) => Assert.Equal("character said \"This is a test!\"", a),
                (a) => Assert.Equal("character said \"Yeaah\"", a),
                (a) => Assert.Equal("character said \"Yeaah\"", a),
                (a) => Assert.Equal("character said \"Yeaah\"", a),
                (a) => Assert.Equal(SAVED_STRING, a),
                (a) => Assert.Equal("character said \"Yeaah\"", a),
                (a) => Assert.Equal("character said \"Yeaah\"", a),
                (a) => Assert.Equal("character said \"End!\"", a)
                );
        }
        
        [Fact]
        public void SaveAmidChallenge()
        {
            var output = TestSaveAndReload(
@"character> This is a test!
(restart) character> Yeaah
{ bTry = true }
$ blabla
    +
        [ bTry ] character> I Win!
    -
        character> I Lose!
character> End!
", 3);
        
            TestResults(output,
                (a) => Assert.Equal("character said \"This is a test!\"", a),
                (a) => Assert.Equal("character said \"Yeaah\"", a),
                (a) => Assert.Equal("Challenge: blabla", a),
                (a) => Assert.Equal(SAVED_STRING, a),
                (a) => Assert.Equal("character said \"I Win!\"", a),
                (a) => Assert.Equal("character said \"End!\"", a)
                );
        }
        
        [Fact]
        public void SaveAmidWait()
        {
            var output = TestSaveAndReload(
@"character> This is a test!
(restart) character> Yeaah
=>
    other> Yuuhuu
    other> Eee-eee
    other> Ooo-ooo
    { @bParallel = true}
    other> Iii-iii
{ wait @bParallel }
character> End!
", 4);
        
            TestResults(output,
                (a) => Assert.Equal("character said \"This is a test!\"", a),
                (a) => Assert.Equal("character said \"Yeaah\"", a),
                (a) => Assert.Equal("other said \"Yuuhuu\"", a),
                (a) => Assert.Equal("other said \"Eee-eee\"", a),
                (a) => Assert.Equal(SAVED_STRING, a),
                (a) => Assert.Equal("other said \"Ooo-ooo\"", a),
                (a) => Assert.Equal("other said \"Iii-iii\"", a),
                (a) => Assert.Equal("character said \"End!\"", a)
                );
        }
        
        [Fact]
        public void SaveAmidChoice()
        {
            var dialogue =
@"character> This is a test!
character:
    - Oh no
        character> anyway...
    - This is terrible
character> End!
";
        
        var output = TestSaveAndReload(dialogue, 2, 1, 1);
        TestResults(output,
            (a) => Assert.Equal("character said \"This is a test!\"", a),
            (a) => Assert.Equal(CHOICE_STRING, a),
            (a) => Assert.Equal(SAVED_STRING, a),
            (a) => Assert.Equal("character said \"Oh no\"", a),
            (a) => Assert.Equal(SAVED_STRING, a),
            (a) => Assert.Equal("character said \"anyway...\"", a),
            (a) => Assert.Equal(SAVED_STRING, a),
            (a) => Assert.Equal("character said \"End!\"", a)
            );
        }

        [Fact]
        public void SaveChangeSourceAndReloadReplaceNext()
        {
            var dialogue =
@"character> This is a test!
character:
    - Oh no
        character> anyway...
        character> bla bla
    - This is terrible
character> End!
";
            var newDialogue =
@"character> This is a test!
character:
    - Oh no
        character> anyway...
        character> bla bla replaced
    - This is terrible
character> End!
";
        
        var output = TestSaveChangeAndReload(dialogue, 4, newDialogue);
        TestResults(output,
            (a) => Assert.Equal("character said \"This is a test!\"", a),
            (a) => Assert.Equal(CHOICE_STRING, a),
            (a) => Assert.Equal("character said \"Oh no\"", a),
            (a) => Assert.Equal("character said \"anyway...\"", a),
            (a) => Assert.Equal(SAVED_STRING, a),
            (a) => Assert.Equal("character said \"bla bla replaced\"", a),
            (a) => Assert.Equal("character said \"End!\"", a)
            );
        }

        [Fact]
        public void SaveChangeSourceAndReloadReplacePrevious()
        {
            var dialogue =
@"character> This is a test!
character:
    - Oh no
        character> eyoo
        character> anyway...
        character> bla bla
    - This is terrible
character> End!
";
            var newDialogue =
@"character> This is a test!
character:
    - Oh no
        character> neyoo
        character> anyway...
        character> bla bla replaced
    - This is terrible
character> End!
";
        
        var output = TestSaveChangeAndReload(dialogue, 5, newDialogue);
        TestResults(output,
            (a) => Assert.Equal("character said \"This is a test!\"", a),
            (a) => Assert.Equal(CHOICE_STRING, a),
            (a) => Assert.Equal("character said \"Oh no\"", a),
            (a) => Assert.Equal("character said \"eyoo\"", a),
            (a) => Assert.Equal("character said \"anyway...\"", a),
            (a) => Assert.Equal(SAVED_STRING, a),
            (a) => Assert.Equal("character said \"bla bla replaced\"", a),
            (a) => Assert.Equal("character said \"End!\"", a)
            );
        }

        [Fact]
        public void SaveChangeSourceAndReloadReplaceCurrent()
        {
            var dialogue =
@"character> This is a test!
character:
    - Oh no
        character> eyoo
        character> anyway...
        character> bla bla
    - This is terrible
character> End!
";
            var newDialogue =
@"character> This is a test!
character:
    - Oh no
        character> eyoo
        character> anyway, I'd like to tell you two things...
        character> bla bla replaced
    - This is terrible
character> End!
";
        
        var output = TestSaveChangeAndReload(dialogue, 5, newDialogue);
        TestResults(output,
            (a) => Assert.Equal("character said \"This is a test!\"", a),
            (a) => Assert.Equal(CHOICE_STRING, a),
            (a) => Assert.Equal("character said \"Oh no\"", a),
            (a) => Assert.Equal("character said \"eyoo\"", a),
            (a) => Assert.Equal("character said \"anyway...\"", a),
            (a) => Assert.Equal(SAVED_STRING, a),
            (a) => Assert.Equal("character said \"bla bla replaced\"", a),
            (a) => Assert.Equal("character said \"End!\"", a)
            );
        }



[Fact]
        public void SaveChangeSourceAndReloadRemoveAndAddLines()
        {
            var dialogue =
@"character> This is a test!
character:
    - Oh no
        character> eyoo
        character> anyway...
        character> bla bla
    - This is terrible
character> End!
";
            var newDialogue =
@"character> This is a test!
character:
    - Oh no
        character> anyway...
        character> bla bla replaced
    - This is terrible
character> End!
";
            var newDialogue2 =
@"character> This is a test!
character:
    - Oh no
        character> eyoo
        character> oh wow
        character> anyway...
        character> bla bla replaced
    - This is terrible
character> End!
";
        
        var output = TestSaveChangeAndReload(dialogue, 5, newDialogue);
        TestResults(output,
            (a) => Assert.Equal("character said \"This is a test!\"", a),
            (a) => Assert.Equal(CHOICE_STRING, a),
            (a) => Assert.Equal("character said \"Oh no\"", a),
            (a) => Assert.Equal("character said \"eyoo\"", a),
            (a) => Assert.Equal("character said \"anyway...\"", a),
            (a) => Assert.Equal(SAVED_STRING, a),
            (a) => Assert.Equal("character said \"bla bla replaced\"", a),
            (a) => Assert.Equal("character said \"End!\"", a)
            );

            output = TestSaveChangeAndReload(dialogue, 5, newDialogue2);
        TestResults(output,
            (a) => Assert.Equal("character said \"This is a test!\"", a),
            (a) => Assert.Equal(CHOICE_STRING, a),
            (a) => Assert.Equal("character said \"Oh no\"", a),
            (a) => Assert.Equal("character said \"eyoo\"", a),
            (a) => Assert.Equal("character said \"anyway...\"", a),
            (a) => Assert.Equal(SAVED_STRING, a),
            (a) => Assert.Equal("character said \"bla bla replaced\"", a),
            (a) => Assert.Equal("character said \"End!\"", a)
            );
        }

        List<string> TestSaveAndReload(string dialogues, params int[] saveAfterStep)
        {
            DataRoot dataRoot = new DataRoot();
            DialogueSet dialogueSet = new DialogueSet();
            Parse(dialogues, dialogueSet);
            DialogueMachine dm = new DialogueMachine(dialogueSet, dataRoot);
            List<string> output = new List<string>();
            List<System.Action> deferredActions = new List<Action>();
            var rand = new Random();
            
            dm.onWaitTimeStart += (context, node) => deferredActions.Add(() => { foreach (var c in dm.RunningContexes.ToArray()) c.Advance(); });
            dm.onCaptionStart += (context, node) => { output.Add("[ " + node.Text + " ]"); deferredActions.Add(() => { foreach (var c in dm.RunningContexes.ToArray()) c.Advance(); }); };
            dm.onSpeechStart += (context, node) => { output.Add(node.CharacterId + " said \"" + node.Text + "\""); deferredActions.Add(() => { foreach (var c in dm.RunningContexes.ToArray()) c.Advance(); }); };
            dm.onSpeechOptionStart += (context, node) => { output.Add(node.Parent.CharacterId + " said \"" + node.Text + "\""); deferredActions.Add(() => { foreach (var c in dm.RunningContexes.ToArray()) c.Advance(); }); };
            dm.onChallengeStart += (context, check, s) => { output.Add("Challenge: " + s); deferredActions.Add(() => { foreach (var c in dm.RunningContexes.ToArray()) c.CompleteChallenge(true); }); };
            dm.onChoiceStart += (context, node) => {
                output.Add(CHOICE_STRING); 

                deferredActions.Add(() => { 

                    foreach (var c in dm.RunningContexes.ToArray()) 
                    {
                        if (c.Current is IChoosableNode choiceNode)
                        {
                            for (int i=0; i<choiceNode.OptionsCount; ++i)
                            {
                                int id = (i) % choiceNode.OptionsCount;
                                var option = choiceNode.GetOption(id);
                                if (option.IsAvailable(c))
                                {
                                    c.Choose(option);
                                    return;
                                }   
                            }

                            c.Choose(null);
                        }
                    }
                });
            };

            dialogueSet.GetDialogue("Test", out var dialogue);
            var dialogueContext = dm.Start(dialogue);

            for (int s=0; s<saveAfterStep.Length; ++s)
            {
                for (int i=0; i<saveAfterStep[s] - (s == 0 ? 1 : 0); ++i)
                    ExecuteNextAction(deferredActions, dm);

                SaveAndReload(dm);
                output.Add(SAVED_STRING);
            }

            int steps = 0;
            while (ExecuteNextAction(deferredActions, dm))
                if (steps++ > 1000) Assert.Fail("Endless loop");

            return output;
        }

        List<string> TestSaveChangeAndReload(string dialogues, int changeAfterStep, string newDialogues)
        {
            DataRoot dataRoot = new DataRoot();
            DialogueSet dialogueSet = new DialogueSet();
            Parse(dialogues, dialogueSet);
            DialogueMachine dm = new DialogueMachine(dialogueSet, dataRoot);
            List<string> output = new List<string>();
            List<System.Action> deferredActions = new List<Action>();
            var rand = new Random();
            
            dm.onWaitTimeStart += (context, node) => deferredActions.Add(() => { foreach (var c in dm.RunningContexes.ToArray()) c.Advance(); });
            dm.onCaptionStart += (context, node) => { output.Add("[ " + node.Text + " ]"); deferredActions.Add(() => { foreach (var c in dm.RunningContexes.ToArray()) c.Advance(); }); };
            dm.onSpeechStart += (context, node) => { output.Add(node.CharacterId + " said \"" + node.Text + "\""); deferredActions.Add(() => { foreach (var c in dm.RunningContexes.ToArray()) c.Advance(); }); };
            dm.onSpeechOptionStart += (context, node) => { output.Add(node.Parent.CharacterId + " said \"" + node.Text + "\""); deferredActions.Add(() => { foreach (var c in dm.RunningContexes.ToArray()) c.Advance(); }); };
            dm.onChallengeStart += (context, check, s) => { output.Add("Challenge: " + s); deferredActions.Add(() => { foreach (var c in dm.RunningContexes.ToArray()) c.CompleteChallenge(true); }); };
            dm.onChoiceStart += (context, node) => {
                output.Add(CHOICE_STRING); 

                deferredActions.Add(() => { 

                    foreach (var c in dm.RunningContexes.ToArray()) 
                    {
                        if (c.Current is IChoosableNode choiceNode)
                        {
                            for (int i=0; i<choiceNode.OptionsCount; ++i)
                            {
                                int id = (i) % choiceNode.OptionsCount;
                                var option = choiceNode.GetOption(id);
                                if (option.IsAvailable(c))
                                {
                                    c.Choose(option);
                                    return;
                                }   
                            }

                            c.Choose(null);
                        }
                    }
                });
            };

            dialogueSet.GetDialogue("Test", out var dialogue);
            var dialogueContext = dm.Start(dialogue);

            for (int i=0; i<changeAfterStep - 1; ++i)
                ExecuteNextAction(deferredActions, dm);

            SaveReparseAndReload(dm, newDialogues, dialogueSet);
            output.Add(SAVED_STRING);

            int steps = 0;
            while (ExecuteNextAction(deferredActions, dm))
                if (steps++ > 1000) Assert.Fail("Endless loop");

            return output;
        }

        void SaveAndReload(DialogueMachine dm)
        {
            MemoryStream memoryStream = new MemoryStream();
            var writer = new BinaryWriter(memoryStream);
            dm.SaveState(writer);
            
            memoryStream.Position = 0;
            var reader = new BinaryReader(memoryStream);
            dm.LoadState(reader, (name) => { Assert.Fail(name + " was not found"); return null; });
        }

        void SaveReparseAndReload(DialogueMachine dm, string newDialogues, DialogueSet dialogueSet)
        {
            MemoryStream memoryStream = new MemoryStream();
            var writer = new BinaryWriter(memoryStream);
            dm.SaveState(writer);

            Parse(newDialogues, dialogueSet);
            
            memoryStream.Position = 0;
            var reader = new BinaryReader(memoryStream);
            dm.LoadState(reader, (name) => { Assert.Fail(name + " was not found"); return null; });
        }

        bool ExecuteNextAction(List<Action> actions, DialogueMachine machine)
        {
            machine.Update();

            if (actions.Count > 0)
            {  
                actions[0]();
                actions.RemoveAt(0);
            }
                
            return machine.RunningContexesCount > 0;
        }

        void Parse(string dialogueLines, DialogueSet dialogueSet)
        {
            List<Dialogue> dialogues = new List<Dialogue>();

            var text = "§ Test\n" + dialogueLines;

            Parse(true, text, dialogues);
            
            Assert.NotEmpty(dialogues);

            dialogueSet.AddDialogues(dialogues);
        }
    }
}