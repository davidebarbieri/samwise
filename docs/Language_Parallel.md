# Parallel Dialogues

In traditional video games, character dialogues often follow a linear, turn-based approach where characters speak one at a time. When the player is expected to contribute to the conversation, other characters wait in silence until a dialogue choice is made. This approach, while functional, can feel unnatural and disrupt the immersion.

To create a more dynamic and realistic dialogue experience, modern games are increasingly incorporating support for parallel dialogues. One notable example is the game Oxenfree, where dialogue choices are time-based and can be made while another character is speaking. This allows players to interrupt and influence the conversation in real-time, making the interactions feel more spontaneous and lifelike.

The ability to have multiple characters speak simultaneously or engage in overlapping conversations can significantly enhance the narrative depth and realism of a game. For instance, in scenarios where characters are reacting to surprising news, the cacophony of multiple voices can convey a sense of urgency and chaos that a turn-based system cannot. Similarly, during social gatherings or debates, overlapping dialogues can better reflect the flow of real conversations.

By supporting parallel dialogues, game developers can create more engaging and immersive storytelling experiences, where player choices have immediate and meaningful impacts on the narrative flow. This feature opens up new possibilities for character interactions, making the virtual world feel more alive and responsive to the player's actions.

There are three basic nodes used to achieve parallelism in Samwise: 
- Fork Node (=>)
- Join Node (<=)
- Cancel Node (<!=)

## Fork node

The Fork Node is used to initiate parallelism in dialogue sequences. When a Fork Node is encountered, it splits the dialogue flow into multiple parallel contexes, allowing different conversations or dialogue commands to happen simultaneously. Each branch created by the Fork Node operates independently of the others, enabling complex and dynamic interactions within the game. This is particularly useful in scenarios where multiple characters need to speak at the same time or when different events need to unfold concurrently.

What happens is that the initial dialogue continues past the fork point, while a brand new dialogue execution will start at the designated point.

```samwise
§ Initial
=> Target
character1> I'm continuing talking...

§ Target
character2> I'm talking over you!
character2> And you can't help it!
```
Target can be a local label, a dialogue or a label in another dialogue (DialogueName.labelName).

A fork node can optionally define a name for its fork point. Such name can be used later to identify the child context (read the next section).
```samwise
name => label 
name => Dialogue.label 
```

### Anonymous Fork node

Anonymous fork nodes are a way to write parallel dialogues, without the need to split the flow
in two different dialogue blocks (ie "Initial" and "Target" in the previous example).

```
name =>
    character2> I'm talking over you!
    character2> And you can't help it!
character1> I'm continuing talking...
```

## Join node

The Join Node is used to synchronize multiple parallel dialogues, bringing them back together into a single flow. When a Join Node is encountered, it waits for all parallel contexes initiated by a Fork Node to complete before continuing. This ensures that all concurrent dialogues or actions are properly concluded before the game progresses. The Join Node is essential for maintaining the coherence of the narrative and ensuring that the game's dialogue sequences are resolved in a structured manner.

```samwise
<=
```

If a name is specified, as in 

```samwise
name <=
```
then, the dialogue will wait just the dialogue associated to the previously named fork point, as described in the previous section.

## Cancel node

The Cancel Node is a control node that terminates the execution of one or more child dialogues, ensuring that no further actions or dialogues occur within them. Additionally, it acts as an implicit Join Node by waiting for all child contexts to conclude correctly before proceeding. The Cancel Node is useful for managing the flow of conversations and ensuring that the game can adapt dynamically to player choices or changing scenarios.

This is the syntax used to cancel a specific child dialogue context:
```samwise
name <!=
```
and this is the one used to cancel every child context:
```samwise
<!=
```

### Await node

There is an additional node, built on the concepts described so far, called Await. The Await Node is essentially a Fork immediately followed by a Join. 

```samwise
<=> Target
```

While this might seem useless in terms of parallel execution (and it is!), its utility lies in another area: structuring the narrative to be more understandable. Fundamentally, it is equivalent to embedding one dialogue within another. For those with some programming experience, the concept is similar to that of a function call.

It's probably easier to explain with examples:
```samwise
§ Initial
<=> CharactersMeet
<=> SomethingIsWrong
<=> Quarreling
<=> Breakup
<=> Epilogue
```
In this example, a long dialogue is divided into three conceptually separate parts.

```samwise
§ Initial
[Character.bIsHungry]
    character> You know, I'm a bit hungry...
    <=> TalkAboutHavingDinner
[else]
    <=> WatchTV
    character> Hey look what time it is!
    <=> TalkAboutHavingDinner
    character> Next time we should be a little more careful about the time
```
In this other example, it is shown how the Await Node can be very useful when you want to execute the same dialogue within multiple points.

Also, it can be used to write utility dialogues:
```samwise
<=> RandomSwearing
```

## Interruptible Sections

### Catch node

An additional tool for managing the interruption of dialogue sections is the Catch Node. It allows for the interruption of parts of a dialogue based on the value of a boolean variable. This node can be used to interrupt only a portion of a forked dialogue, rather than the entire dialogue. It is also useful for interrupting a dialogue section based on external events (e.g., events originating from the game).

During the execution of a dialogue, before any node within the node subtree is being processed, the system will monitor a specified variable's value. Should this value be true, the execution is immediately halted, and the system will skip the remainder of the subtree, moving on as defined.

```samwise
! @bVariable
    <nodes>
```

Given that the entire block subject to interruption is bypassed if the variable is already true, manually resetting the variable to false before its next use can become cumbersome. To streamline this process, the Reset and Catch node can be employed.
```samwise
!! @bVariable
    <nodes>
```
The behavior mirrors that of a Catch node, with the critical distinction being that the variable is automatically reset upon the node's execution.

#### Catch and subcontexes

If a fork occurs from a node within an interruptible block, the sub-context that is created will also be subjected to that check. The test check will be carried out at each node of the sub-context. If the check fails (i.e., the tested variable is true), the sub-context will be stopped.