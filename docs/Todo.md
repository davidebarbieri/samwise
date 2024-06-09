# Roadmap

!> !! means *High Priority*

## Language

### !! Multi-line comments

I'll add multi-line comments in this form: 
```samwise
/*  
*/
```
or 
```samwise
/[
]/
```

Multi-line comments, unlike C/C++/C#, will have a hierarchy.

So if you open multi-line comments two times: /* /*, you must also close them */ */
This will allow you to comment large section of the script possibly containing internal multi-line comments
(in C, the comment would stop at the first */ occurrence).

### !! Alternative Options

I'll add the ability to add alternative options in choices. That is, if the condition fails,
the next alternative's condition is tested (no condition means "true"). And only if all the alternatives
are false the option is skipped.
This will allow you to modify the text (or even mute) of the option based on any condition.

| is a speech option

|| is a muted option

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

### Alternative Syntax

I want to add an additional alternative syntax for symbol-based nodes:

- "?"			&rarr; {if}
- "%"			&rarr; {score}
- ">> iVar"	    &rarr; {loop iVar} 
- ">>> iVar"	&rarr; {clamp iVar} 
- ">< iVar" 	&rarr; {ping iVar} 
- "$ name"	    &rarr; {check name}
- "! bVar" 	    &rarr; {catch bVar}
- "!! bVar" 	&rarr; {recatch bVar}
- a => b 	    &rarr; {a fork b}
- a <=   	    &rarr; {a join}
- a <!=   	    &rarr; {a cancel}
- <=> b   	    &rarr; {await b}

## Features

## !! Code

- integer expressions
    - module "%" "%%"

- boolean expressions
    - even(int)
    - odd(int)

- custom functions ?

- not sure if I want to introduce fixed point numbers

## Assert

Skipped in release mode
{assert <boolean expression>}

onAssertFailed

## Breakpoint (?)
Not sure if that's actually useful.

Skipped in release mode

In code:
{break}
{break <boolean expression>}

In runtime:
SetBreakpoint(Node, boolean func?, callback)
RemoveBreakpoint(Node)

onBreakpoint

## Additional Selection Nodes

## VS-Code Extension improvements

- Keyboard shortcuts for each node type
- Time-based auto-play;
- Panel to configure characters colors/avatars
- States History (a bar with automatic and custom snapshots)
- Export Html preview player
- Export dialogues history;
- Panel for workspace statistics

## Runtime

Runtime features:
- Save/Load state for DialogueMachine and data
- DialogueMachine.Reload()
    - For each dialogue context, if the dialogue doesn't contain their dialogue anymore, 
    then try to recover the position on a new dialogue
        - Fire a reload automatically on Parse
        - Try to migrate to node with same Id
        - Try to migrate on same line
             When not found:
                - Migrate to Node with same Previous (or Parent) and Next
                - Search node with the same type and content
                - Migrate to Node with same Parent

Native runtime library for:
- C
    - Integration in C/C++, e.g. Unreal and custom engines 
- Javascript
    - Temporarely the javascript backend is WASM from C#, which produces 10 MB of library (too much)
    - Rewriting a native runtime library will make it a few KBs
    - Used to export to Html preview and make web games
    - Alternatively, produce WebAssembly from C

Engine runtime:
- Godot
- Unreal
- Defold

## Runtime Unity

- Current State/Data Inspector
- Multiple SamwiseDatabase

## Documentation

- Add the missing pages
- Switch to Docusaurus
- API docs

## Performance/Memory Optimizations. 

At the moment, I'm still working to provide functionality to Samwise. 
Parsing could be still heavily optimized but, since the amount of text data to be processed is usually relatively small compared to the rest of the game data, 
I'll leave this phase until all the functionality is almost done.

- using spans (string fullText, int from, int len) instead of allocating strings in some points when strings are allocated needlessly

- use StringBuilder when printing code

- The current "Reparse whole file and substitute line" in vscode, when you assign IDs, is a bit time-consuming


## Tools

- Samwise Savestate inspector / editor
