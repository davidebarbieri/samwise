# Samwise Design 

## Compromise between readability and simplicity

Often, readability stems from simplicity, yet it's not an absolute rule. Samwise is crafted to prioritize optimal readability while ensuring functionality, whether at the granular level (e.g., reading a single line) or at the conceptual level (e.g., grasping the structure of a text upon first glance, such as when opening a file).

This commitment means that occasionally, despite the potential for a more concise syntax for a feature, Samwise may demand a bit more effort from you to enhance readability. For instance, the redundant use of "-" and indentation in certain dialogue nodes.

!> To be honest, the choice of using both indentation and '-' is a clear solution to fix ambiguity in certain use cases of conditional nodes.

## Indentation is part of the language

In Samwise, indentation is part of the language, as it is in some programming languages (e.g., Python). Unlike these languages, which often recommend a standard way to indent code—through either the use of any amount of spaces or tabs—but then allow some inconsistencies, I've made a strong choice:

!> **Indentation must be consistent.** If you start using tabs for indentation in a file, then you must indent everything with tabs. While if you use N whitespaces, then you must always use N whitespaces.

Since the language necessarily encourages the use of various levels of indentation, and because incorrect indentation implies constructing a dialogue tree different from the intended one, I don't want there to be any possibility of inconsistency (e.g., a mix of spaces and tabs, or places where 2 spaces are used, others 4, etc.).

> If you're not sure  whether to use whitespaces or tabs, the recommended option is *using tabs*.

Unlike normal code scripting, dialogues written in Samwise are likely to have very long lines (thus making use of considerable horizontal space), so since using tabs allows the actual width of indentation to be modified at will, users can configure it to their liking at the editor level rather than at the source code level.

## Diff-friendly

The syntax of Samwise is designed to promote collaboration through version control software. Syntax choices are made to facilitate automatic merges or simplify the merge process overall.

## Stateless Design

The Samwise runtime is designed to maintain a stateless architecture wherever possible. Nodes within Samwise are stateless, with all state information explicitly stored in variables. Programmers have full freedom to manage these variables in any manner they deem fit, alongside the Samwise machine which orchestrates active dialogues and tracks current nodes.

The complete state of the Samwise runtime is thus limited to the set of running dialogues, their position in the dialogue (i.e. which is the current node), plus the local and global variables state.

## Discouraging the mix of text and code

In Samwise, text and behavior are kept separate. This means that when you send your dialogues for localization, there's no need to worry about code associated with a particular node breaking due to copy/paste mistakes made by the translator (which, in my experience, happen quite frequently) and potentially causing critical bugs. The only issues that localization might introduce to your build will be directly related to the localization process itself or, at most, the display of specific text.

Also, Samwise discourages the procedural generation of dialogue lines. The tool was designed for projects involving voice-over, so the text contained in each line should be predictable.

That said, the adventurous ones will still have the opportunity to handle procedural lines downstream. This can be achieved by processing the text with custom code or by utilizing features of localization systems - such as Unity's Smart String - to manage singular/plural forms or create more complex dynamic strings.

## Determinism

Samwise is deterministic. It does not support floating point numbers (although I'm considering incorporating fixed-point math). The runtime does provide the flexibility to define the behavior of the pseudo-random generator in generating its values.

### Locking up dangerous features

I've been thinking a lot about adding the ability to save/load in the middle of a dialogue; 
without the ability to save the exact state of a dialogue, it also comes the limit of not being able to save the state of the entire game at any given time.
The alternative is forcing the user (or the game) to save when no dialogues are running.

Anyway, even if the former feature can be easily added in Samwise, it must be taken with some pracautions.
All these reasons are connected to the eventuality of modifying dialogues after the release of a game (that is, in a patch).
For example, what happens if a player saves the game while a specific dialogue node is played, and that node is then removed or moved
in a patch? If cases like this are not handled properly, some blocking bugs could happen. And that's too much responsibility for a writer in my opinion.

I'd like to allow people using Samwise to freely develop their games, iteratively (e.g. early access games), without worrying to break their users' savegames.

This means that a release build cannot use anonymous variables and the game can save only if the current node of any running dialogue has a unique ID assigned.

## No Clobbering while prototyping

My tool has been developed with the idea of keeping the language concise and free from metadata until it becomes necessary. This aligns with the moment when the game is to be released, and thus, every identifier for dialogue lines (for translation, dubbing, or referencing purposes that need to be saved to disk) must become unique. 

At that point, all automatically generated variables or identifiers for each line receive a unique ID written in the .sam file in the form of a tag or explicit variable. At that moment, the actual syntax becomes slightly heavier to read, but this only occurs once the dialogues are finalized. This precaution allows for dialogue modifications in patches without the fear of breaking saved games.