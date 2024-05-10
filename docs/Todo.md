# Roadmap

!> !! means *High Priority*

## Language

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
- <=> b   	    &rarr; {await b}

## Features

## !! Code

- integer expressions
    - module "%" "%%"

- boolean expressions
    - even(int)
    - odd(int)

- custom functions ?

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

- Panel for workspace statistics
- Keyboard shortcuts for each node type
- Time-based auto-play;
- Panel to configure characters colors/avatars
- States History (a bar with automatic snapshots)
- Export Html preview player
- Export dialogues history;

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
- Current Data Inspector in example

## Documentation

- Add the missing pages
- Switch to Docusaurus

## Performance/Memory Optimizations. 

At the moment, I'm still working to provide functionality to Samwise. 
Parsing could be still heavily optimized but, since the amount of text data to be processed is usually relatively small compared to the rest of the game data, 
I'll leave this phase until all the functionality is almost done.

- using spans (string fullText, int from, int len) instead of allocating strings in some points when strings are allocated needlessly

- use StringBuilder when printing code

- The current "Reparse whole file and substitute line" in vscode, when you assign IDs, is a bit time-consuming


## Tools

- Samwise Savestate inspector / editor