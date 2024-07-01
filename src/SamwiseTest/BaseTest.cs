using Xunit;

namespace Peevo.Samwise.Test
{
    public class BaseTest
    {
        protected static void Parse(bool assertResult, string text, List<Dialogue> dialogues, IExternalCodeParser? externalParser = null)
        {
            SamwiseParser parser = new SamwiseParser(externalParser ?? new DummyCodeParser());

            try
            {
                bool res = parser.Parse(dialogues, text);

                if (!res)        
                {
                    var message = "Unable to parse input text. Errors:";
                    foreach (var e in parser.Errors)
                        message += "- " + e.Error + " at line " + e.Line + "\n";

                    if (assertResult)
                        Assert.Fail(message);
                }
            }
            catch (Exception e)
            {
                var message = "Error: " + e.Message + "\n";
                message += (e.StackTrace);
                
                if (assertResult)
                        Assert.Fail("Unable to parse file: " + message);
            }
        }

        protected void TestResults(List<string> output, params System.Action<string>[] checkFunctions)
        {
            Assert.Equal(checkFunctions.Length, output.Count);

            for (int i=0; i<output.Count; ++i)
                checkFunctions[i](output[i]);
        }
        
        protected static List<string> AutoPlay(Dialogue dialogueToTest, DialogueSet fullDialogueSet, bool skipWaits)
        {
            List<string> output = new List<string>();
            AutoPlay(dialogueToTest, fullDialogueSet, output, skipWaits);
            return output;
        }

        static void AutoPlay(Dialogue dialogueToTest, DialogueSet fullDialogueSet, List<string> output, bool skipWaits = false)
        {
            var dataRoot = new DataRoot();

            DialogueMachine dm = new DialogueMachine(fullDialogueSet, dataRoot);

            var rand = new Random();

            List<System.Action> deferredActions = new List<Action>();


            if (skipWaits)
                dm.onWaitTimeStart += (context, node) => deferredActions.Add(() => context.Advance());
            else
                dm.onWaitTimeStart += (context, node) => { deferredActions.Add(() => {System.Threading.Thread.Sleep((int)(node.Time * 1000)); context.Advance();}); };

            dm.onCaptionStart += (context, node) => { output.Add("[ " + node.Text + " ]"); deferredActions.Add(() => context.Advance()); };
            dm.onSpeechStart += (context, node) => { output.Add(node.CharacterId + " said \"" + node.Text + "\""); deferredActions.Add(() => context.Advance()); };
            dm.onSpeechOptionStart += (context, option) => { output.Add(option.Parent.CharacterId + " said \"" + option.GetTextContent(context).Text + "\""); deferredActions.Add(() => context.Advance()); };
            dm.onChallengeStart += (context, check, s) => { output.Add("Challenge: " + s); deferredActions.Add(() => context.CompleteChallenge(true)); };
            dm.onChoiceStart += (context, node) => {
                var rnd = rand.Next();

                for (int i=0; i<node.OptionsCount; ++i)
                {
                    int id = (rnd + i) % node.OptionsCount;
                    var option = node.GetOption(id);
                    if (option.IsAvailable(context, out _))
                    {
                        context.Choose(option);
                        return;
                    }   
                }

                context.Choose(null);
            };
            dm.Start(dialogueToTest);

            while (deferredActions.Count > 0)
            {
                deferredActions[0]();
                deferredActions.RemoveAt(0);
            }

            Assert.True(dm.RunningContexesCount == 0);
        }
    }
}