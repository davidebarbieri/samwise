using Xunit;

using System.Diagnostics;

namespace Peevo.Samwise.Test
{
    public class RuntimeTest : BaseTest
    {
        [Fact]
        static void TestParse()
        {
            string filename = "test.sam";
            string text = System.IO.File.ReadAllText(filename);

            List<Dialogue> dialogues = new List<Dialogue>();
            SamwiseParser parser = new SamwiseParser(new DummyCodeParser());
    
            Stopwatch stopWatch = new Stopwatch();

            int testSize = 2000;

            GC.Collect();
            long allocatedMemory = GC.GetTotalMemory(true);

            stopWatch.Start();
            for (int i=0; i<testSize; ++i)
            {
                dialogues.Clear();

                try
                {
                    bool res = parser.Parse(dialogues, text);

                    if (!res)        
                    {
                        var message = "Unable to parse input text. Errors:\n";
                        foreach (var e in parser.Errors)
                            message += ("- " + e + "\n");

                        Assert.Fail(message);
                        return;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }

            stopWatch.Stop();

            GC.Collect();
            allocatedMemory = GC.GetTotalMemory(true) - allocatedMemory;

            TimeSpan ts = stopWatch.Elapsed;
            float elapsedTime = ts.Hours*3600 + ts.Minutes*60 + ts.Seconds + ts.Milliseconds * 0.001f;

            Assert.InRange(elapsedTime, 0f, 1f); // Less than 1 second on any target machine
            Console.WriteLine("Test time: " + elapsedTime + ". Files per second: " + (testSize/elapsedTime));
            
            Assert.NotEmpty(dialogues);

# if DEBUG
            Assert.InRange(allocatedMemory, 5000, 400000); // 5KB to 400KB
#else
            Assert.InRange(allocatedMemory, 5000, 60000); // 5KB to 60KB
#endif
        }   


        [Fact]
        static void TestExecution()
        {  
            string filename = "test.sam";
            try
            {
                string text = System.IO.File.ReadAllText(filename);

                // Test single parsing
                var dialogues = ParseDialogues(text);
                DialogueSet dialogueSet = new DialogueSet();
                Dialogue? firstDialogue = null;

                if (dialogues != null)
                {
                    // Write back dialogues
                    foreach (var dialogue in dialogues)
                    {
                        dialogueSet.AddDialogue(dialogue);

                        if (firstDialogue == null)
                            firstDialogue = dialogue;
                    }    

                    Assert.NotNull(firstDialogue);

                    
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    int rounds = 100;

                    for (int i=0; i<rounds; ++i)
                        AutoPlay(dialogues[0], dialogueSet, true);
                    
                    TimeSpan ts = stopWatch.Elapsed;
                    float elapsedTime = ts.Hours*3600 + ts.Minutes*60 + ts.Seconds + ts.Milliseconds * 0.001f;

#if DEBUG
                    Assert.InRange(elapsedTime/rounds, 0f, 0.01f); // Less than 10 millisecond on any target machine
#else
                    Assert.InRange(elapsedTime/rounds, 0f, 0.001f); // Less than 1 millisecond on any target machine
#endif
                }
            }
            catch (System.IO.FileNotFoundException e )
            {
                Assert.Fail("Error: Could not find file " + e.FileName);
            }
        }

        static List<Dialogue>? ParseDialogues(string text)
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
                        Console.WriteLine("- " + e.Error + " at line " + e.Line);
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
    }
}