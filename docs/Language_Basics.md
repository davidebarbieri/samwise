# Language Basics

## Dialogue

In Samwise, a **Dialogue** is the basic unit that composes the narrative of a game. Although it bears this name, a Dialogue is simply a structured set of **Nodes** (I will explain later what these are); it is up to the developer to determine whether this unit represents an actual conversation between two or more characters, or a brief series of dialogue lines triggered by an action (such as examining an object), or an entire chapter of the story, or even a series of utility instructions that have nothing narrative within them.

A Dialogue must have an identifying Label, and optionally a Title.

```samwise
§ (Label) Title of the Dialogue
```

The Label must be unique for each dialogue, it must start with a capital letter, and it's used by the game to refer to and launch them. The title, on the other hand, is a more conversational way to describe the dialogue and may make sense when the game needs to show a description of a certain narrative unit to the player (e.g., the title of a chapter).

'§' is the standard character used to tell Samwise a new dialogue is being defined. It's also the main reference of Samwise's logo.
Anyway, there are other characters that are accepted by the parser, like '¶'. Check the [Language Specification](Language.md) for other examples. The choice of using one character over another is at the user's discretion. 

## Speech Node

As mentioned earlier, the nodes of a dialogue are the elements that structure a dialogue. A node can be, for example, a line of spoken dialogue, a.k.a. **Speech Node**:

```samwise
Character_Name> An interesting line spoken by an interesting character
```

## Caption Node

Or, a **Caption node**, which is a line expressed directly by the narrator. A bit like rectangular balloons in comics:

```samwise
* Suddenly the room's temperature plummeted.
```

## Goto Node


A Goto Node allows the flow to move from one dialogue to another, or to a different point within the same dialogue.

And it follows this syntax:
```samwise
-> DialogueLabel
```

Once this node is reached, the flow will automatically shift to the first node of DialogueLabel.

### Labels

A label is a lowercase alphanumeric word that is unique within the same Dialogue (meaning the same word can be reused in different dialogues), and it can be added to any type of node like this:

```samwise
(label_name) <node>
```
where "<node>" is any type of node.

Goto nodes also support jumping to labels, making them useful when you need to redirect the flow to a specific point.

The following example demonstrates how adding a label allows you to jump from one point in the dialogue to another.

```samwise
Alice> Are you crazy, why you did that?
Alice> You stupid.
Bob> I know you are, but what am I?
Alice> What? Shut up, you idiot.
(loop) Bob> I know you are, but what am I?
Alice> I know you are, but what am I?
-> loop
```

In this case, once the Goto node is reached, flow will continue from the line labeled with "loop". 
Clearly, what's produced is an endlessly childish loop, which I must admit is quite painful, so please refrain from doing so, it was merely for illustrative purposes.

There is also a special label for when you want to end the dialogue:
```samwise
-> end
```

We've seen how to jump to the beginning of another dialogue and how to jump to a label within the same dialogue, but is it possible to jump to a label belonging to another dialogue? The answer is yes, and it's done like this:
```samwise
-> Dialogue_name.label_name
```

---


Using just the previous nodes, you could write an entire screenplay — which is fantastic, but I suppose you're not here for that.

What makes an experience interactive is precisely the fact that it varies based on the player's inputs. For this reason, the next node to be introduced is the cornerstone of every interactive dialogue system.

## Choice Node

A choice node allows the developer to define a series of options from which the player can choose, and each option leads the dialogue in a different direction — namely, to a specific set of nodes — before potentially converging again later on.

This is the syntax:
```samwise
character_id:
    - Option 1
        <optional nodes>
    - Option 2
        <optional nodes>
    - Option 3
        <optional nodes>
    <more options>
```

For example,

```samwise
Alice> Where do you want to go for dinner tonight?
Bob:
    - I'd like to go to the Japanese restaurant
        Alice> Sure, let's have some sushi.
        Bob> I love sushi!

    - I'd like to go to the Italian Restaurant
        Alice> Sure, I really miss eating some pasta.
        Bob> Me too.

Alice> Perfect, then let's make a reservation there. Good choice!
```

!> It's important to emphasize that indentation is integral to the syntax and must be included. The parser will deduce your indentation style from the first instance it encounters, so consistency is key.

Samwise considers options preceded by "-" as something that will act as speech nodes once chosen. However, sometimes we just want to offer a more abstract choice without an associated dialogue line, such as "Empty the bag" and "Say nothing". These types of options, *Action options*, can be added using a double dash "--".

The Action option also provide a way to avoid overly long texts within the in-game choice UI, or at least allows for expressing that choice across multiple dialogue lines, making the conversation feel more natural.

```samwise
Alice> Where do you want to go for dinner tonight?
Bob:
    -- Japanese Restaurant
        Bob> Hmm, I'm torn between the Italian restaurant and the Japanese one.
        Bob> But I think we'll go Japanese in the end. I feel like switching it up a bit.

    -- Italian Restaurant
        Bob> Italian sounds good to you?
        Alice> Sure, I really miss eating some pasta.
        Bob> Yeah, me too.
        
Alice> Perfect, then let's make a reservation there. Good choice!
```

I've mentioned that indentation is part of the syntax. There's a reason for it, and as you might have guessed, nodes that belong to a block associated with a choice don't have to be just Speech Nodes. They can, for example, contain further choice nodes themselves.


```samwise
Bob:
    - I'd like to go to the Japanese restaurant
        Alice> Do you prefer an all-you-can-eat buffet or a traditional restaurant?
        Bob:
            -- All-you-can-eat
                Bob> I prefer an all-you-can-eat buffet; I'm very hungry and don't want to spend a fortune.

            -- Traditional
                Bob> I prefer a traditional restaurant; we might pay more, but the quality is usually better.
        Alice> All right then.
```

In general, what you're constructing is a tree of nodes (the so-called Dialogue tree). 
Whenever the flow "ascends" through the branches of an option block, its nodes are executed, and when they are completed, the flow descends by one branching level and continues from where it left off (in this case from the node following the choice node).

```samwise
Bob> A
Bob:
    - B
        Bob> C
        Bob:
            - D
                Bob> E
        Bob> F
Bob> G
```

In other words, the previous flow will be necessarily: A &rarr; B &rarr; C &rarr; D &rarr; E &rarr; F &rarr; G

### Nested Choices

For those old enough (sigh) to remember the dialogues from LucasArts graphic adventures, they might recall encountering multi-level dialogues where at the base level there were topics, and upon selecting a topic, one could choose questions related to that specific topic. Once those questions were exhausted, you could go back and select another topic. With a sense of nostalgia, I'll refer to that type of dialogue as Monkey Island-like, or, with less romanticism, Nested Dialogue Trees.

It's possible to create this mechanic very easily in Samwise using a feature of the language:

By adding a "<" character before the dashes in the choice options, you instruct the game that once the subtree related to that choice is exhausted, instead of seeking the next node forward in the dialogue, the flow should return to the parent node (essentially repeating the choice).


```samwise
Bob:
    <- Let's talk about business
        Alice> Sure.  What do you want to know?
        Bob:
            <- Do you have any idea what the budget is?
                <more nodes>
            <- How do I convince your boss that I'm the right person for this job?
                <more nodes>
            - Enough with business
    <- Let's talk about weather
        <more nodes> 
    <-- Nods in approval
    - Bye
```

As you can see, the option "Enough with business" doesn't use the '<' character, so choosing that option results in the completion of the subtree, and therefore a return to the list of topics. the same goes for "Bye" which ends the dialogue

### Alternative Options

It is possible to provide multiple alternative texts for the same option. This is because it can feel more natural for the text to slightly change when the player, for example, asks a character the same question they had asked before upon returning to a choice node.

It is possible to add alternatives to an option in the following way:
```samwise
galbroom:
    <- [once] Share more about becoming a sword master
        | Tell me once more about becoming proficient in swordplay
        pirate_A> First, thou need a sword.
        // ...

    <- [once] Share more about mastering the art of thievery
        | Tell me once more about stealing the idol.
        // ...

    <- [once] Share more about the treasure-huntery.
        | Tell me once more about the Lost Treasure.
        // ...
        
   - I'll be on my way now.
```

If the condition of the option is false, all the conditions of the alternatives will be tested, and the first one to evaluate as true will be selected.

```samwise
bob:
    <- [once, TheQuest.bDone] I completed the Quest.
        | [once, TheQuest.bDone] Did I tell you I completed the Quest?
        | [once, TheQuest.bDone] No, really. I completed the Quest.
        | [once, TheQuest.bDone] Hey, I finished that Quest.
        | [once, TheQuest.bDone] Just to remind you, the Quest is done.
        alice> I don't care.
```


!> No condition is equal to a condition that's always true.
The option is skipped only if all the alternatives conditions are false.

!> Only the main option condition can have check or time attributes.

## Comments

It is possible to add comments between nodes using the following syntax:
```samwise
// This is a comment
```
The parser will skip such lines for good.
This type of comment is only useful for the person writing the Samwise file to annotate something that they will need to remember. 

!> There will be no trace of these comments either in-game or in the documents sent to translators.

### Multi-line Comments

Multi-line comments can be written using the special character ↵.
To produce this character, press *Shift+Enter*.

in this way:
```samwise
// This is a comment↵
that spans over multiple lines
```

## Disabled lines

In Samwise, commenting and disabling lines are two separate features and they are highlighted with a different color in the editor.

It's possible to disable single nodes in the following way:
```samwise
~ character> this line is disabled
```

!> When you disable a node, you'll disable its whole subtree.


It's possible to disable sections of a script in the following way:
```samwise
/~
character> this line is disabled
character> this too!
~/
character> this is still enabled!
```

Even if the syntax is similar to the syntax used in C to write multi-line comments (/* */), I must stress that in Samwise you should not use this syntax to produce multi-line comments (see the previous section for multi-line comments).

Unlike C multi-line comments, disabled sections can be nested in the following way:
```samwise
/~
/~
character> this line is disabled
character> this too!
~/
character> now this is disabled!
~/
```

You can the shortcut Shift+Ctrl+A to quickly disable/reenable lines and nodes.