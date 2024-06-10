using System.Diagnostics;
using Xunit;

namespace Peevo.Samwise.Test
{
    public class NodesFunctionalityTest : BaseTest
    {
        [Fact]
        public void TestSpeechAndCaption()
        {
            string dialogues = 
@"character> This is a test!
other> Yeah, I know!
* it was really a test
";
            TestDialogueLines(dialogues,
                (a) => Assert.Equal("character said \"This is a test!\"", a),
                (a) => Assert.Equal("other said \"Yeah, I know!\"", a),
                (a) => Assert.Equal("[ it was really a test ]", a)
                );
        }

        // Goto, Label, Condition, Code
        [Fact]
        public void TestFlow()
        {
            string dialogues = 
@"character> Start here
(repeat) [iVar > 0] other> Again...
character> YO-HOO!
~ character> Skipped line
{iVar += 1}
[iVar < 5] -> repeat
* it was the end
";
            TestDialogueLines(dialogues,
                (a) => Assert.Equal("character said \"Start here\"", a),
                (a) => Assert.Equal("character said \"YO-HOO!\"", a),
                (a) => Assert.Equal("other said \"Again...\"", a),
                (a) => Assert.Equal("character said \"YO-HOO!\"", a),
                (a) => Assert.Equal("other said \"Again...\"", a),
                (a) => Assert.Equal("character said \"YO-HOO!\"", a),
                (a) => Assert.Equal("other said \"Again...\"", a),
                (a) => Assert.Equal("character said \"YO-HOO!\"", a),
                (a) => Assert.Equal("other said \"Again...\"", a),
                (a) => Assert.Equal("character said \"YO-HOO!\"", a),
                (a) => Assert.Equal("[ it was the end ]", a)
                );
        }
        

        [Fact]
        public void TestLoop()
        {
            string dialogues = 
@"(repeat) >> iVar
    -
        character> A
    -
        character> B
    -
        character> C
    -
        character> D
    -
        character> E
{ iTries += 1 }
[iTries < 15] -> repeat
";
            TestDialogueLines(dialogues,
                (a) => Assert.Equal("character said \"A\"", a),
                (a) => Assert.Equal("character said \"B\"", a),
                (a) => Assert.Equal("character said \"C\"", a),
                (a) => Assert.Equal("character said \"D\"", a),
                (a) => Assert.Equal("character said \"E\"", a),
                (a) => Assert.Equal("character said \"A\"", a),
                (a) => Assert.Equal("character said \"B\"", a),
                (a) => Assert.Equal("character said \"C\"", a),
                (a) => Assert.Equal("character said \"D\"", a),
                (a) => Assert.Equal("character said \"E\"", a),
                (a) => Assert.Equal("character said \"A\"", a),
                (a) => Assert.Equal("character said \"B\"", a),
                (a) => Assert.Equal("character said \"C\"", a),
                (a) => Assert.Equal("character said \"D\"", a),
                (a) => Assert.Equal("character said \"E\"", a)
                );
        }
        

        [Fact]
        public void TestClamp()
        {
            string dialogues = 
@"(repeat) >>> iVar
    -
        character> A
    -
        character> B
    -
        character> C
    -
        character> D
    -
        character> E
{ iTries += 1 }
[iTries < 15] -> repeat
";
            TestDialogueLines(dialogues,
                (a) => Assert.Equal("character said \"A\"", a),
                (a) => Assert.Equal("character said \"B\"", a),
                (a) => Assert.Equal("character said \"C\"", a),
                (a) => Assert.Equal("character said \"D\"", a),
                (a) => Assert.Equal("character said \"E\"", a),
                (a) => Assert.Equal("character said \"E\"", a),
                (a) => Assert.Equal("character said \"E\"", a),
                (a) => Assert.Equal("character said \"E\"", a),
                (a) => Assert.Equal("character said \"E\"", a),
                (a) => Assert.Equal("character said \"E\"", a),
                (a) => Assert.Equal("character said \"E\"", a),
                (a) => Assert.Equal("character said \"E\"", a),
                (a) => Assert.Equal("character said \"E\"", a),
                (a) => Assert.Equal("character said \"E\"", a),
                (a) => Assert.Equal("character said \"E\"", a)
                );
        }

        [Fact]
        public void TestPingPong()
        {
            string dialogues = 
@"(repeat) >< iVar
    -
        character> A
    -
        character> B
    ~ -
        character> Skipped
/~
    -
        character> Skipped
~/
    -
        character> C
    -
        character> D
    -
        character> E
{ iTries += 1 }
[iTries < 15] -> repeat
";
            TestDialogueLines(dialogues,
                (a) => Assert.Equal("character said \"A\"", a),
                (a) => Assert.Equal("character said \"B\"", a),
                (a) => Assert.Equal("character said \"C\"", a),
                (a) => Assert.Equal("character said \"D\"", a),
                (a) => Assert.Equal("character said \"E\"", a),
                (a) => Assert.Equal("character said \"D\"", a),
                (a) => Assert.Equal("character said \"C\"", a),
                (a) => Assert.Equal("character said \"B\"", a),
                (a) => Assert.Equal("character said \"A\"", a),
                (a) => Assert.Equal("character said \"B\"", a),
                (a) => Assert.Equal("character said \"C\"", a),
                (a) => Assert.Equal("character said \"D\"", a),
                (a) => Assert.Equal("character said \"E\"", a),
                (a) => Assert.Equal("character said \"D\"", a),
                (a) => Assert.Equal("character said \"C\"", a)
                );
        }

        [Fact]
        public void TestFallback()
        {
            string dialogues = 
@"(repeat) ?
    - [iVar == 0]
        character> A
    - [iVar == 1]
        character> B
    - [iVar == 2, once]
        character> C
    - [iVar == 3]
        character> D
    -
        character> E
{ iTries += 1 }
{ iVar += 1 }
[ iVar == 5] { iVar = 0 }
[iTries < 15] -> repeat
";
            TestDialogueLines(dialogues,
                (a) => Assert.Equal("character said \"A\"", a),
                (a) => Assert.Equal("character said \"B\"", a),
                (a) => Assert.Equal("character said \"C\"", a),
                (a) => Assert.Equal("character said \"D\"", a),
                (a) => Assert.Equal("character said \"E\"", a),
                (a) => Assert.Equal("character said \"A\"", a),
                (a) => Assert.Equal("character said \"B\"", a),
                (a) => Assert.Equal("character said \"E\"", a),
                (a) => Assert.Equal("character said \"D\"", a),
                (a) => Assert.Equal("character said \"E\"", a),
                (a) => Assert.Equal("character said \"A\"", a),
                (a) => Assert.Equal("character said \"B\"", a),
                (a) => Assert.Equal("character said \"E\"", a),
                (a) => Assert.Equal("character said \"D\"", a),
                (a) => Assert.Equal("character said \"E\"", a)
                );
        }

        [Fact]
        public void TestTags()
        {
            string dialogues = 
@"character> This is a test!
sam> Yeah, I know! # wise, sam=wise, ""woa"", comment=""that's incredible""
";
            var output = TestDialogueLines(dialogues,
                (a) => Assert.Equal("character said \"This is a test!\"", a),
                (a) => Assert.Equal("sam said \"Yeah, I know!\"", a)
                );

            Assert.NotEmpty(output[0].GetChild(1).TagData.Tags);
            Assert.NotEmpty(output[0].GetChild(1).TagData.GetNamedTags());
            Assert.NotEmpty(output[0].GetChild(1).TagData.Comments);
            Assert.NotEmpty(output[0].GetChild(1).TagData.GetNamedComments());

            Assert.Equal("wise", output[0].GetChild(1).TagData.Tags[0]);
            Assert.Equal("woa", output[0].GetChild(1).TagData.Comments[0]);
            Assert.Equal("wise", output[0].GetChild(1).TagData.GetNamedTag("sam"));
            Assert.Equal("that's incredible", output[0].GetChild(1).TagData.GetNamedComment("comment"));
        }

        [Fact]
        public void TestParallel()
        {
string dialogues = 
@"character> This is a test!
=>
    !! .bVar
        other> Yeah, I know!
        * it was really a test
        => Test2
        * and it continued
        * but this was skipped
        * but this was skipped
        * but this was skipped
        * but this was skipped
character> woa
character:
    - [ bVar5 ] I do believe it
    - I can't believe it
        { .bVar = true }

§ Test2
character2> Yaya
* but this was skipped
";
            TestDialogueLines(dialogues,
                (a) => Assert.Equal("character said \"This is a test!\"", a),
                (a) => Assert.Equal("other said \"Yeah, I know!\"", a),
                (a) => Assert.Equal("character said \"woa\"", a),
                (a) => Assert.Equal("[ it was really a test ]", a),
                (a) => Assert.Equal("character said \"I can't believe it\"", a),
                (a) => Assert.Equal("character2 said \"Yaya\"", a),
                (a) => Assert.Equal("[ and it continued ]", a)
                );

string dialogues2 = 
@"<=> Test2
<=> {{ Ext Code }}

§ Test2
character> Hey!";

            TestDialogueLines(dialogues2,
                (a) => Assert.Equal("character said \"Hey!\"", a));

string dialogues3 = 
@"a => Test2
character> Oh my.
a <!=
character> Stop it.
Loise> Don't you dare

§ Test2
Loise> Are you crazy?
Loise> Do you think I don't understand?
Loise> Skipped

";

            TestDialogueLines(dialogues3,
                (a) => Assert.Equal("Loise said \"Are you crazy?\"", a),
                (a) => Assert.Equal("character said \"Oh my.\"", a),
                (a) => Assert.Equal("Loise said \"Do you think I don't understand?\"", a),
                (a) => Assert.Equal("character said \"Stop it.\"", a),
                (a) => Assert.Equal("Loise said \"Don't you dare\"", a)
                
                );
        }

        [Fact]
        public void TestScore()
        {
string dialogues = 
@"
{ bVar1 = false }
{ bVar2 = true }

%
    - [bVar1, iVar1]
        sam> A
    - [bVar2, iVar1]
        sam> B
    - [bVar2, iVar1]
        sam> C
    - [bVar2, iVar1 - 1]
        sam> D
";
            TestDialogueLines(dialogues,
                (a) => Assert.True(a.Equals("sam said \"B\"") || a.Equals("sam said \"C\""))
                
                );

        }
        
        [Fact]
        public void TestChoice()
        {
string dialogues = 
@"
sam:
    - A
";
            TestDialogueLines(dialogues,
                (a) => Assert.Equal("sam said \"A\"", a)
                
                );
        }

        [Fact]
        public void TestAutoChoice()
        {
string dialogues = 
@"
{ bVar1 = true }

sam:
    - [bVar1] A
    - [bVar2] B
    - [bVar2] C
    - [bVar2] D
";
            TestDialogueLines(dialogues,
                (a) => Assert.Equal("sam said \"A\"", a)
                
                );
        }

        [Fact]
        public void TestCheck()
        {

string dialogues = 
@"
$ test
    +
        sam> Success
    -
        sam> Fail
";
            TestDialogueLines(dialogues,
                (a) => Assert.Equal("Challenge: test", a),
                (a) => Assert.Equal("sam said \"Success\"", a)
                
                );
        }

        [Fact]
        public void TestWait()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            string dialogues = @"{ wait 2.567s }";
            TestDialogueLines(dialogues);
            
            TimeSpan ts = stopWatch.Elapsed;
            float elapsedTime = ts.Hours*3600 + ts.Minutes*60 + ts.Seconds + ts.Milliseconds * 0.001f;
            Assert.InRange(elapsedTime, 2.567f, 2.7f);
        }

        List<Dialogue> TestDialogueLines(string dialogueLines, params System.Action<string>[] checkFunctions)
        {
            List<Dialogue> dialogues = new List<Dialogue>();

            var text = "§ Test\n" + dialogueLines;

            Parse(true, text, dialogues);
            DialogueSet dialogueSet = new DialogueSet(dialogues);
            
            Assert.NotEmpty(dialogues);

            var output = AutoPlay(dialogues[0], dialogueSet, false);
            
            Assert.Equal(checkFunctions.Length, output.Count);

            for (int i=0; i<output.Count; ++i)
                checkFunctions[i](output[i]);

            return dialogues;
        }
    }
}