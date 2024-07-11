# Runtime

The Samwise runtime library enables integration of the dialogue system into your own game engine. The language used on this page is C#, but runtimes written in other languages will provide the same interface.

!> Note: If the application in which to integrate Samwise uses the **Unity engine**, it is advisable to use the official [Unity Plug-In](Plugin_Unity.md) instead of the runtime library, which provides an additional layer for automated import and management of .sam files as custom assets. However, using the official Unity plugin is not mandatory, and it is possible to directly use this library to write your own plugin.

The essential steps for executing dialogues are as follows:

- Parsing
    1. Instantiation of the **SamwiseParser** class
    2. **Parsing** of one or more .sam files
- Configuration
    3. Creation of a **DataRoot**
    4. Creation of the **DialogueMachine**
    5. Configuration of the DialogueMachine **events**
- Execution
    6. **Starting a dialogue** on the DialogueMachine, which creates a DialogueContext
    7. Calling DialogueContext methods, like **Advance()** and **Choose()**, based on the dialogue events and user input

## Parsing


```C#
SamwiseParser parser = new SamwiseParser();

var dialogues = new List<Dialogue>();
string textToParse = System.IO.File.ReadAllText(filename);
bool result = parser.Parse(dialogues, textToParse);

if (!result)        
{
    // print parser.Errors
}

DialogueSet dialogueSet = new DialogueSet(dialogues);
```
**SamwiseParser** is the class used for reading and processing text files (as Strings). It's possible to reuse the same instance of SamwiseParser to parse multiple text files.

A **DialogueSet** is simply a class that contains a list of dialogues and a dictionary that allows retrieving the dialogue associated with each label.


## Configuration

Before executing dialogues, it's necessary to properly prepare the program. This means creating a DataRoot and a DialogueMachine, and configuring a series of events to understand what to do when, for example, a speech, caption, or choice node is reached.

```C#
var dataRoot = new DataRoot();

DialogueMachine dialogueMachine = new DialogueMachine(dialogueSet, dataRoot);

// Speech
dialogueMachine.onSpeechStart += ShowSpeech;
dialogueMachine.onSpeechEnd += HideSpeech;

// Caption
dialogueMachine.onCaptionStart += ShowCaption;
dialogueMachine.onCaptionEnd += HideCaption;

// Choice
dialogueMachine.onChoiceStart += ShowChoice;
dialogueMachine.onChoiceEnd += HideChoice;
dialogueMachine.onSpeechOptionStart += ShowSpeechOption;

// Wait
dialogueMachine.onWaitTimeStart += StartWaitTime;
dialogueMachine.onWaitTimeEnd += StopWaitTime;

// Challenge
dialogueMachine.onChallengeStart += ShowChallenge;

// Dialogue Start/End/Stop
dialogueMachine.onDialogueContextStart += OnDialogueStart;
dialogueMachine.onDialogueContextEnd += OnDialogueEnd;
dialogueMachine.onDialogueContextStop += OnDialogueCancelled;

// A Dialogue context jumped to another dialogue
dialogueMachine.onDialogueChange += OnDialogueChanged;
```

The **DataRoot** represents the database where all the updated values of global and local variables are contained. It's possible to implement your own DataRoot (by extending **IDataRoot**) when, for example, you want to use a database shared with other scripting languages or provide your own serializable data structure as desired.

The **Dialogue Machine** is the heart of the dialogue system, keeping track of all ongoing dialogues and triggering events based on the nodes that are traversed.

## Execution

```C#
var dialogueToStart = dialogues[0];
IDialogueContext dialogueContext = dialogueMachine.Start(dialogueToStart);    
```

While the **Dialogue** class represents the structure of a dialogue, an **IDialogueContext** is an instance of a dialogue in execution. This context keeps track of the current node and awaits inputs from the program to:

- Advance to the next node
- Check the list of available options
- Choose an option
- Report the outcome of a Challenge Check
- Cancel the dialogue execution

### Advance

Advance must be called by the program while it is on a Speech, Caption, or Wait node.
```C#
dialogueContext.Advance();    
```

### Check the list of options

When the dialogue context is on a Choice Node, it's possible to check which option is available:
```C#
int availableOptions = 0;
for (int i=0; i<choiceNode.OptionsCount; ++i)
{
    var option = choiceNode.GetOption(id);
    if (context.Evaluate(option))
    {
        // Add a button to UI (and associate 'option' to such button)
        ++availableOptions;
    }   
}
```

### Choose an option
If no options are available, then you can either decide to wait for any option to become available (e.g. due to other dialogues modifying the state of the global variables), or *choose the null option*. This means skipping the choice node for good.

```C#
if (availableOptions == 0)
    context.Choose(null);
```

On the other hand, if there are available options, and the user selects one of them (for example, by clicking on a button), then the method must be called in this way:
```C#
context.Choose(option);
```

When the user choose a speech option like
```samwise
character:
    - This is a speech option, I'm actually saying this.
    -- This is not
```
the Dialogue Machine will raise the *onSpeechOptionStart* event. In that case, the program should show the line to the user, and then call
```C#
context.Advance();
```

### Complete a Challenge

After a challenge check (e.g., a dice roll or a Quick Time Event) has been completed, the program must notify the context, also providing the result of the challenge:

```C#
context.CompleteChallenge(checkPassed);
```

### Cancel a dialogue

You can cancel a running context any time with:
```C#
context.Stop();
```

In this case, the Dialogue Machine will raise the *onDialogueContextStop* event instead of *onDialogueContextEnd*.

### Cancel all dialogues

You can cancel all running contextes any time with:
```C#
dialogueMachine.StopAll();
```