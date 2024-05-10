# Variables and Flow Control

## Introduction to variables

This section is dedicated to one of the most powerful aspects of creating dynamic and engaging dialogues: variables. Understanding variables is essential for controlling the flow and logic of your dialogues, making your stories not just more interactive but also more immersive.

> Variables allow your narrative to remember and react to the choices and actions of the user. 

First, let's demystify what a variable is. Imagine a variable as **a box where you can store information**. This information can represent anything: a number, a name, or a decision made by the user. What makes variables incredibly useful is that the information inside this box can change or be used to make decisions as your story unfolds.

## Type of variables

In Samwise, each variable has a strict type. This means that the kind of information a variable can hold is determined and cannot be changed. Types help ensure that your dialogues run smoothly by preventing mix-ups, like confusing a number with a name or a yes/no decision.

To make working with types intuitive and efficient in Samwise, we've implemented a naming convention that allows you to infer the type of a variable directly from its name. This means that by simply looking at a variable's name, you can understand what kind of information it stores.

- Integer variables 
	- Start with 'i' (e.g. *iVarName*)
	- Contains a numeric value (-1, 0, 1, 2, ...)
    - These variables are used for mathematical operations or counting occurrences.
- Boolean variables 
	- Start with 'b' (e.g. *bVarName*)
	- Contains a boolean value (true, false)
    - These are critical for flow control and decision-making logic.
- Symbol variables 
	- Start with 's' (e.g. *sVarName*)
	- Contains a symbolic value (ON, OFF, OPEN, CLOSE, DONE, JACK, ...)
    - These named values help you manage the state of objects or conditions in your story, offering a clear and concise way to track progress and changes.

## Global and Local context

In addition to understanding the difference between variable types, it's crucial to grasp another fundamental aspect of variables: each variable has a context. 
There are two types of variable context in Samwise: **Global** and **Local**.

- **Global Variables** are the memory keepers of your dialogue system. Think of Global variables as a communal bulletin board where any dialogue can post a note. Once a note is posted, it can be seen and used by any other dialogue within the system, at any time. This means that Global variables retain their information throughout the entire interaction with the user, allowing for a continuity of experience and decision-making across different dialogues. For example, if the user makes a significant choice in one part of your story, you can use a Global variable to ensure that this choice impacts other dialogues, creating a cohesive narrative experience.

- **Local Variables** are like notes you make on a personal notepad. These notes are private to each dialogue and are thrown away once the dialogue is over. Local variables store temporary data relevant only to the current running dialogue. They're perfect for keeping track of decisions or information that only matter in the immediate context of the dialogue and should not influence or be influenced by other parts of the narrative. Once the dialogue concludes, the Local variables are erased, making them clean and ready for the next interaction without carrying over unnecessary baggage.

Global variables can be expressed in the following way:
```samwise
contextName.bVarName
```
!> If the user doesn't specify the context name, then the local context is used implicitely.

The following are all examples of global variables:
```samwise
Chapter1.bEnded
Characters.Loise.iTimesTalkedWith
Quests.HerosJourney.sState
.bIsBadEnding
```

while the following are local variables:
```samwise
iCount
bFirstTime
sWeaponChoice
```

### The @ Shortcut

Sometimes certain variables only make sense within a single dialogue and will never be read by the rest of the game. However, there could be still a need for them not to be deleted at the end of its execution, as they must continue to maintain their value in case of future executions of the same dialogue.

To avoid giving a utility variable a name that might be used by mistake in other dialogues, it's advisable to prefix the variable name with the '@' character.

```samwise
@bFirstTime
```

What actually happens is that the '@' character is translated by the language interpreter 
into the dialogue label plus '.'. 

Thus, @bFirstTime will be equal to:

```samwise
LabelOfTheDialogue.bFirstTime
```

## Code nodes

Before explaining how to use variables, we will first look at what Code nodes are.

Samwise supports the execution of commands to help the developer adding conditional behaviour to the dialogues and let the writer trigger events in game, like character animations.

### Built-in Code

The built-in code is a simple code language that allows the user to assign values to variables, do simple math operations and send commands to game.
Built-in code nodes are in the form
```samwise
{ code }
```

## Modifying variables

Variables can be assigned in the following way:
```samwise
 { bVarName = boolean_expression }
 { iVarName = integer_expression }
 { sVarName = SYMBOL_VALUE }
```
### Boolean Expressions

A Boolean expression is a logical statement that can only result in two values: true or false. At its core, a Boolean expression evaluates conditions you set within your narrative or program. These conditions can involve comparisons between variables, checks for specific states, or the results of other Boolean expressions.

For example, if you have a variable that tracks whether a door is locked (*bIsDoorLocked*), a Boolean expression to check if the door is open might look like *bIsDoorLocked == false*. This expression evaluates to true if isDoorLocked is false (meaning the door is not locked and therefore, can be considered open).

Boolean expressions can also use logical operators such as AND (&), OR (|), and NOT (!) to combine multiple conditions. For instance, if you needed to check if a character is alive and has a key, your Boolean expression might be
```samwise
 { bResult = bIsAlive & bHasKey }
```
which only evaluates to true if both conditions are true.

This also means that the first example (*bIsDoorLocked == false*) can be simplified into:
```samwise
 { bResult = !bIsDoorLocked }
```
even though in this case, some prefer the longer form because it's less prone to mistakes.

#### The "once" Operator

Any boolean expression can contain a special operator: *once(boolVariable)*
Such operator is true only the first time the node is visited and false the following times.
```samwise
[once(bVar2)] Character> Ahoy there!
```
The boolean variable bVar2 (in the example) is used to store the information that the node was already visited. 
The semantic of bVar2 is that it's true if the node was already visited. So once(bVar2) will be true, if bVar2 is false.

Of course, I used a local variable for this example for the sake of simplicity, while you would probably use this more often with a global variable. 

The variable is updated only if the node is actually visited, if it's skipped due to any other condition, then its value will not be updated.

!> If desired, it's possible to omit the variable name. In that case, a global variable with a unique name will be generated directly by the interpreter. However, be aware that it's not advisable to leave anonymous variables in the final build of the video game. It's better, before releasing the game to the public, to explicitly assign a name to all anonymous variables (this can be done through the VSCode extension).


### Integer Expressions

Integer expressions deal with numbers and allow for the performance of arithmetic operations like addition, subtraction, multiplication, and division. These expressions evaluate to a numeric value.

For instance, if you're keeping score in a game, you might have an expression to calculate the new score after adding points for a task completion like
```samwise
 { iScore = iScore + 10 }
```
Here, if iScore was 90, the expression would evaluate to 100.

To be noted, that also exists a shortcut form of this operation:
```samwise
 { iScore += 10 }
```

Numeric expressions can be used for more than just arithmetic. They can also be part of conditions in Boolean expressions. For example, to check if a player has collected enough points to advance to the next level, you might use an expression like *iScore >= 100*, which evaluates to a Boolean value (true or false) depending on whether the score is sufficient.

## Conditional Nodes

Every node can have a condition, and it can be expressed in this way:
```samwise
[ iScore < 50 & Items.bHasCellarKey ] Character> Well, maybe I haven't killed so many orcs, but at least I've got the cellar's key!
```

## Conditional Options

Even individual choice options can have conditions; in that case, the condition determines whether a choice should be available or not. 

```samwise
Character:
    - [bIsAvailableA] Choice A
        <nodes>
    - [bIsAvailableB] Choice B
        <nodes>
```

Ideally, as long as the choice is active, the game should cyclically check whether the condition associated with each option is true or false, and hide or display the option accordingly. The reason for doing this cyclically, rather than only at the start of the choice, is due to the fact that this condition could change at any moment.


## Conditional Blocks

We've seen how to make a single line conditional. But what if we wanted to apply the same condition to an entire block of nodes, rather than just one line? Would we need to repeat the same condition for all the lines? Fortunately, no! By using indentation in the following manner, an entire block of nodes will only be executed if the condition of the first line is true.

```samwise
[condition] character> Ahoy!
    <nodes>
```
The same example can also be rewritten in the following way:

```samwise
[condition]
    character> Ahoy!
    <nodes>
```

### Else condition

There's just one piece missing to create if-then-else chains with this syntax: the 'else' keyword.

It's possible to add the 'else' keyword to conditions in this way:

```samwise
[bVar1] dave> Hey!
    elly> What?
[else bVar2] dave> Ahoy!
    elly> ...
[else] dave> Howdy!
    elly> No.
```

By doing so, you instruct the interpreter to evaluate the condition and possibly execute the block of nodes only if the condition of the preceding node is false.

Obviously, the syntax of the following examples is also valid:

```samwise
[bVar1] 
    dave> Hey!
    elly> What?
[else bVar2] 
    dave> Ahoy!
    elly> ...
[else] 
    dave> Ayo!
    elly> No.
```

and

```samwise
[bVar1] dave> Hey!
[else bVar2] dave> Ahoy!
[else] dave> Ayo!
```