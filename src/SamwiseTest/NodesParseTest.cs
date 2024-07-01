using Xunit;

namespace Peevo.Samwise.Test
{
    public class NodesParseTest : BaseTest
    {
        [Fact]
        public void TestSpeech()
        {
            TestParseDialogueLines("character> This is a test!");
        }

        [Fact]
        public void TestCaption()
        {
            TestParseDialogueLines("* That was a test!");
        }

        [Fact]
        public void TestMoreLines()
        {
            TestParseDialogueLines(@"char1> It's a test!
* That was a test!
char2> I told you!
");
        }

        [Fact]
        public void TestMultiLineSpeech()
        {
            TestParseDialogueLines(@"char1> It's a test!↵
A test!↵
A Magnific Test!
* This too↵
    Yuhuuu!
char:
    - and this↵
    this too...
");
        }
        

        [Fact]
        public void TestGoto()
        {
            TestParseDialogueLines(@"(restart) char> says
-> restart
-> end"
);
        }

        [Fact]
        public void TestLabel()
        {
            TestParseDialogueLines("(this_is_a_label) * That was a test!");
        }

        [Fact]
        public void TestCondition()
        {
            TestParseDialogueLines("[(bValue & (iValue > 4))] * That was a test!");
            
            TestParseDialogueLines("[(once & (bValue & (iValue > 4)))] * That was a test!");
        }

        [Fact]
        public void TestCode()
        {
            TestParseDialogueLines("{ global.wow.iVar3 = 5 }");
        }

        [Fact]
        public void TestTags()
        {
            TestParseDialogueLines("loise> Rumors tell the Room is feeded by our fear. # face_palm, \"NO WHY?\", \"YEAH WHY\", id=yeeehE23, comment=\"Yahoo!!\"");
        }
        

        [Fact]
        public void TestWait()
        {
            TestParseDialogueLines("{ wait 1s }");
            TestParseDialogueLines("{ wait 14.5s }");
            TestParseDialogueLines("{ wait (bVar1 & bVar2) }");
        }
        

        [Fact]
        public void TestPrecheck()
        {
            TestParseDialogueLines("[precheck perception.hard] perception> She's a spy");
        }

        [Fact]
        public void TestCheck()
        {
            TestParseDialogueLines(@"$ quick
    +
        captain> FIRE!
    -
        captain> Run!
        captain> For the love of gods, run!"
);
        }

        [Fact]
        public void TestForksAndJoins()
        {
            TestParseDialogueLines(@"=> TheTest.test
symA => TheTest.test1
symA <=
<=> TheTest
symB =>
    char> parallel
");
        }

        [Fact]
        public void TestConditionNodes()
        {
            TestParseDialogueLines(
@"[bVal]
    jack> Gosh!
[!bVal]
    jack> Non-Gosh!
[bVarA] dave> Hey!
[else bVarB] dave> Ahoy!
    [bVal]
        elly> that's cool
        [bKKA] elly> No
        [else]
            elly> YEAH!
    [else]
        dave> C'mon!
        dave> C'mooon!
[else] dave> Ayo!
");
        }

        [Fact]
        public void TestChoice()
        {
            TestParseDialogueLines(
@"(cc) [10s] david:
    -- [once(Test.bHello)] Try Something
        elly> Nooooooo
        davide:
            <- [10.5s] Test 1
            <-- Test 2
                elly> wooooa
    - [once(bTTs), 0.5s] You know the answer, Jack # yoyo
    - [once(Test.bHello)] Hello there
        elly> Hello!
    - [check bOne] YOO
    - [(bValue & once(.bYaa)), precheck bTwo] YOo-ooh!"
);
        }

        [Fact]
        public void TestOptionAlternatives()
        {
            TestParseDialogueLines(
@"(cc) [10s] david:
    -- [once(Test.bHello)] Try Something
        | [once(Test.bHello2)] Try Something alternative
        | [once(Test.bHello3)] Try Something alternative 2
        elly> Nooooooo
        davide:
            <- [10.5s] Test 1
            <-- Test 2
                elly> wooooa
    - [once(bTTs), 0.5s] You know the answer, Jack # yoyo
    - [once(Test.bHello)] Hello there
        elly> Hello!
    - [check bOne] YOO
        | [once] YOO 2
    - [(bValue & once(.bYaa)), precheck bTwo] YOo-ooh!"
);
        }

        [Fact]
        public void TestSelection()
        {
            TestParseDialogueLines(
@">> iOne
    -
        char> first
    -
        char> second
>>> iTwo
    -
        char> first
    -
        char> second
>< iThree
    -
        char> first
    -
        char> second
?
    - [bVal1]
        char> first
    - [bVal2]
        char> second
    -
        char> third"
);
        }

        [Fact]
        public void TestScore()
        {
            TestParseDialogueLines(
@"%
    - [2x]
        char> first
    -
        char> second
    - [bValue, iValue1 + iValue2, 5x]
        char> third"
);
        }

        [Fact]
        public void TestInterruptible()
        {
            TestParseDialogueLines(
@"! Test.bVar1
    char> hey!
!! Test.bVar2
    char> hey!
    { bVar = true }
    char> Ahoy!
"
);
        }

        [Fact]
        public void TestPrintLines()
        {
            InterruptibleNode interruptibleNode = new InterruptibleNode(true, null, 0, "test.", "bVar");
            SpeechNode speechNode = new SpeechNode(1,1,"char", "what");
            speechNode.Block = interruptibleNode.InterruptibleBlock;

            Assert.Equal("\tchar> what", speechNode.PrintLine("\t"));

            ChoiceNode choiceNode = new ChoiceNode(0, "char");
            var option = new Option(0, 1, 1, choiceNode, "A test", true, false, null, null, null, null, false);
            choiceNode.AddOption(option);
            Assert.Equal("\t-- A test", option.PrintLine("\t"));
        }

        private static void TestParseDialogueLines(string dialogueLines)
        {
            List<Dialogue> dialogues = new List<Dialogue>();

            var text = "§ Test\n" + dialogueLines;

            Parse(true, text, dialogues);
            
            var outputText = dialogues[0].ToString();

            Assert.Equal(text.ReplaceLineEndings().Trim(), outputText.ReplaceLineEndings().Trim());
        }
    }
}