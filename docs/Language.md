# Samwise Scripting Language Specifications

> Current Version: 0.9

This document displays the structure and the full list of features provided by the **Samwise™ Language**.  

The way the syntax is delined here is pretty programmer-oriented and probably it's not what a writer would want to read (unless they have some programming knowledge). For a more practical insight into this tool, I suggest reading the pages in the **Writing in Samwise** section.

!> The current specifications haven't yet reached their first stable milestone, so they're likely to be modified.

---

## 1 Samwise file format

* Each file can store one or more Samwise dialogues.  
* The text file must be encoded in UTF8 format (with or without BOM preamble).  
* The default file extension used to store Samwise scripts is ***.Sam***.

## 2 Dialogue

Each dialogue is made by 
- a title header and 
- a tree of dialogue nodes.

## 3 Dialogue Title header

The title can be written in a rather personal and creative way. In fact, 
* the title must be single-line but it must preceded by a mandatory *adornment* on the same line, and can have a set of optional *adornments* in the previous and the following lines.
* the first character of a title must be upper-case

An adornment is a text made by one or more characters from this set:
* § ¶
* ‗ ¯ « » ±
* ─ ┬ ┼ │ ┤ ├ ┴ ┐ ┘ ┌ └ 
* ═ ╦ ╬ ║ ╣ ╠ ╩ ╗ ╝ ╔ ╚ 
* ░ ▒ ▓ █ ▀ ■ ▄

In other words, a *Dialogue Title Header* is structured as following:
```
<optional adornment>*
<mandatory adornment> This is a title sample <optional adornment>
<optional adornment>*
```

The following are all valid examples of a dialogue title:
```
§ Dialogue Title
```

```
─────────── Dialogue Title ───────────
```

```
┌───────────────────┐
│   Dialogue Title  │▒
└───────────────────┘▒
 ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
```

* Dialogues can be referenced in the rest of the script or in other scripts by their *label*, which is by default equal to the title itself after removing all the whitespaces.  
For example, the label of the following dialogue title
```
§ My Dialogue Title
```
is **MyDialogueTitle**.
* Users can override the default label by prefixing the title with *(LabelName)* in this way:
```
========= (MyLabel) Dialogue Title =========
```
here, MyLabel will be the label used to reference such dialogue.
* the override label must have the first character upper-case.

Further specifications:
* Each dialogue title header can be preceded and/or succeeded by white lines.
* A white line is a line made just by whitespaces and/or tab characters.


## 4 Dialogue Nodes

A Samwise dialogue can be made by one or more nodes.

There are different types of node: **Speech, Caption, Choice, Fallback, Random, Clamp, Loop, Challenge, Goto, Fork, Join, Await,** and **Code**.
Generally a node is structured in the following way
```
(customLabel) <node> # customTag1, customTag2, "Custom comment 1", "Custom comment 2"
```
where ```<node>``` is defined in the following way.

Each node can have an optional label. Label names must be lower case.
```
(labelName) node
```

### Attributes

Each node can have a list of attributes which can be used for multiple purposes.
```
[attribute1, attribute2, attribute2, ...] <node>
```

Some attributes make sense only in a certain context, as delined in the following sections.
Anyway, there's an attribute that can be always used, which is the Condition attribute.
Conditions are used to execute dialogue nodes based on the resulting value of a boolean expression.
For example, the following line
```
[bVar1] Someone> I'm telling something
```
is executed only if the variable bVar1 is true. Otherwise it's skipped.

Conditions are boolean expression, so they can integrate the common operators & (and), | (or) and ! (not).
```
    [bVar1 & !iVar2 > 2]
```

There's also the once(boolVariable) operator, which is true only the first time the node is visited and false the following times.
```
    [once(bVar2)]
```
The boolean variable bVar2 (in the example) is used to store the information that the node was already visited. 
The semantic of bVar2 is that it's true if the node was already visited. So once(bVar2) will be true, if bVar2 is false.

The variable is updated only if the node is actually visited, if it's skipped due to any other condition, then its value will not be updated.
Also, if the same variable is reused in multiple nodes, then it means that it will evaluate false if any of these nodes are visited.




### 4.1 Speech node
A Speech node describes someone that says something. Each narrative character is identified by a string made by alpha-numeric characters (plus "_" and '.').

```
Someone> I'm telling something
```

### 4.2 Caption node
A Caption node describes a narrative event that is not directly connected to a specific narrative character.
```
* Something happened
```

### 4.3 Choice node
A Choice node states that someone can say or choose something based on the player selection.

```
Someone:
    - Choice 1
    - Choice 2
    -- Mute Choice 3
```

* The character '-' is used for something that must be said by Someone
* The characters '--' represent a choice, but it doesn't mean someone is actually saying something. For example:
```
You:
	-- Save him
	-- Leave him
```
* It's possible to specify a boolean expression as condition for each choice to show up. To be noticed that a condition can change, and so an option could appear/disappear, while the choice is running.
```
Someone:
    - [bValue1 & bValue2] Choice 1
    - [bValue2] Choice 2
```
* It's possible to specify time deadlines. In this way:
```
Someone:
    - Choice 1
    - Choice 2
    - [1.2s] Choice 3
```
Here it means the deadline is 1.2 seconds before Choice 3 is removed from possible options.
Thus,
```
Someone:
    - [1.2s] Choice 1
    - [1.2s] Choice 2
    - [1.2s] Choice 3
```
means selection is completely skipped after 1.2s.
Time deadlines and boolean conditions can be mixed:
```
Someone:
    - [1.2s, bValue1 & bValue2] Choice 1
    -- Choice 2
```

* To be noted that the indentation is part of the language and it's mandatory. Indentation can be done by either using the tab character or N-whitespaces tabulation, but this choice must be consistent across the source file.

Such a node implies a branching narrative, as the selection will make the dialogue continue through the selected path.
Indentation again is used to define such branches:
```
Someone:
    - Choice 1
        <nodes>
        ...
    - Choice 2
        <nodes>
        ...
```

### 4.4 Fallback node
A Fallback node, as opposed to Choice node, generates branching narrative based on logical conditions, instead of user selection.
```
?
    - [if_condition]
        <nodes>
    - [elseif_condition]
        <nodes>
    - [elseif_condition]
        <nodes>
    -
        <nodes>
```
* Conditions are boolean expressions

For example,
```
?
    - [(bVariable1 & bVariable2) | !bVariable3]
        <nodes>
    - [iVariable1 >= iVariable2]
        <nodes>
    -
        <nodes>
```
is a valid fallback node.

When this node is reached, the first condition is tested. If the condition is true, then the related branch is selected, 
otherwise the second condition is tested, and so on until reaching the last available condition.
As you can see in the previous sample, the node can provide an optional else branch in case all the previous conditions are false.

### 4.5 Loop node

The Loop node directs a dialogue towards a different child node each time it's visited.
Each child node can have an optional condition. If the selected node's condition is false,
the next child node is evaluated, and so on.
When the last node is selected/evaluated, the process will restart from the first child node.

```
>> iVariable
    - [bCond1]
        <nodes>
    - [bCond2]
        <nodes>
```

The iVariable is an integer variable that stores the incremental counter used to loop between nodes.

### 4.5 Clamp node

The Clamp node works in the same way as a Loop node. The only difference is that once the last node is reached, the node will continue to select/evaluate the last child node. 

```
>>> iVariable
    - [bCond1]
        <nodes>
    - [bCond2]
        <nodes>
```

### 4.6 Score node

The Random node, as the name implies, selects a random child node.
Each child has an optional attribute that allows the writer to specify how much probable
is a selection.  
A value of **5x** means that a child node is five times more probable than one with **1x** (which is the default value).

```
%
    - [5x]
        <nodes>
    - [2x]
        <nodes>
    -
        <nodes>
```

### 4.7 Goto node

The Goto node allows to jump directly to the specified node in a dialogue.
A target can be a simple label, in case the target is another node in the same dialogue,
or in the format {dialogue name}.{label}, if the node is part of a different dialogue.

```
-> labelName
-> DialogueName
-> DialogueName.labelName
```

There's a special jump node that makes the dialogue complete its execution:
```
-> end
```

### 4.8 Fork node

The execution model of Samwise allows the possibility to run multiple dialogues in the same time. When the execution of a dialogue reaches a fork node, it branches off in parallel. What happens is that the former dialogue continues past the fork point, while a brand new dialogue execution will start at the designated point.

```
=> label
=> DialogueName
=> DialogueName.label
```

A fork node can optionally define a name for its fork point. Such name can be used later in Join/Await nodes (further explanation in their related sections).

```
name => label 
name => DialogueName 
name => DialogueName.label 
```

#### 4.8.1 Anonymous Fork node

It's possible to have anonymous fork nodes, that is you can create a whole subtree in the mid of a dialogue in this way:
```
name =>
    <nodes A>
<nodes B>
```
This will make the flow fork: the main flow will continue executing "nodes B",
while the forked dialogue will execute the subtree in the "nodes A" block.

### 4.9 Join node

If a Fork node makes the execution of a dialogue to split up in two parallel directions, the Join node is used to make two dialogues to restore their sequential execution. More precisely, a dialogue that reaches a Join node will be paused until the execution (forked using the name <i>name</i>) is completed.

```
name <=
```

If no name is specified, as in 

```
<=
```
then, the dialogue will wait all the previously fork nodes.

#### 4.9.1 Cancel node

The Cancel Node is a node that cancels the execution of one or more parallel dialogues within the current context. It also implicitly acts as a join, as it waits for all child contexts to be properly concluded before proceeding.

```
name <!=
```

If no name is specified, as in 

```
<!=
```
then, the dialogue will cancel all the previously fork nodes.

### 4.10 Await node (Fork + Join)

The Await node is equivalent to a Fork node followed by a Join node. When a dialogue execution reaches this node, it will pause, the target dialogue will be started, and the former dialogue will be restored only once the child dialogue is completed.

```
<=> label
<=> DialogueName
<=> DialogueName.label
```

### 4.11 Challenge Check node

The challenge check is a node that allows the dialogue to ask the game to check the skills of the player,
and branching the dialogue based on the result of the check.
Such challenge can be anything, for example a QTE, the verification of some player statistics, or just a random result.
The check has a *name*, which is used by the game to select which kind of challenge must be provided.

```
$ name
	+
		<nodes>
	-
		<nodes>
```

The node will branch to the plus (+) block if passed, or the minus (-) block if failed.

A more simplified check - similar to a condition test - can be used, when one or more nodes need to be executed
only if the check is passed:
```
[precheck(name)] <node>
```

Challenge Checks can be used in choices too, using the *check* or *precheck* attributes.
A *precheck* node will be entered only if the challenge is passed, while a *check* will be entered any case.
In order to branch the dialogue based on the result of the challenge check, the bPass/bFail variables can be used.
Of course, bPass is equal to !bFail.

```
Someone:
    - [check(name)] Choice 1
		[bPass] <node>
		[bFail] <node>
    - [precheck(name)] Choice 2
		<nodes>
```

### 4.12 Interruptible node

Interruptible nodes are mostly useful when there are multiple dialogues running.
While a dialogue is currently executing any node which is in the node subtree, it will continously check the value of a given variable,
and if it's true the whole subtree is skipped.

```
! bVariable
    <nodes>
```

## 5 Code nodes

Samwise supports the execution of commands to help the developer adding conditional behaviour to the dialogues and let the writer trigger events in game, like character animations.

Samwise supports two types of code: built-in and embeddable.
The built-in code is a very simple code language that allows the user to assign values to variables,
do simple math operations and send commands to game.

In addition to that, the user can embedd their own scripting language to be used within code nodes.

### 5.1 Built-in Code

Built-in code nodes are in the form
```
{ code }
```

#### 5.1.1 Variables

Samwise built-in code is strictly typed, and the type can be inferred from the variable name:
- integer variables 
	- start with 'i' (iVarName)
	- contains a numeric value (-1, 0, 1, 2, ...)
- bool variables 
	- start with 'b' (bVarName)
	- contains a boolean value (true, false)
- symbol variables 
	- start with 's' (sVarName)
	- contains a symbolic value (ON, OFF, DONE, JACK, ...)

- Assignment
```
 { bVarName = expression }
```

A variable can be used to create conditions.
Variables can optionally have context, which can be expressed in the following way:
```
 contextName.bVarName
```
The context is used to allow sharing data across multiple dialogues. If the user doesn't specify a context, that the local context is used implicitely.
The local context has the scope of the running dialogue and it's erased when the dialogue ends.

It's possible to associate a variable to a specific dialogue, but still prevent to be erased when the dialogue ends.
This can be done by adding the character '@' before the variable name. What actually happens is that the '@' character
is translated into the dialogue label. Thus @iVar1 will be the same as writing NameOfTheDialogue.iVar1.

The runtime API allows the programmers to customize how such global contexts are stored.

### 5.2 Embeddable Code (Custom-defined code)

Embedded code nodes are similar to built-in code nodes, but they use double curly brackets.

!> In order to support this kind of node, the game must provide a Custom Code Parser using the Runtime API.

#### Code statements

Such statements are executed synchonously.

```
{{ external code }}
```

#### Embeddable Condition

The API allows the user to define conditions in their custom language as well.

```
[{{ external code }}]
```

#### Fork/Join/Await on Embeddable Code

In case of asynchonous code, the following syntax must be used:

```
=> {{ code }}
name => {{ code }}
<=> {{ code }}
```
forking, joining or awaiting embeddable code is similar to what happens with regular fork/join/await nodes.
The difference is that instead of executing other dialogues, custom code will be issues asynchronously.

## 6 Line Comments

It is possible to add comments between nodes using the following syntax:
```
// This is a comment
```
The parser will skip such lines for good.

## 7 Tags

Meaningful metadata can be incorporated into dialogue nodes by utilizing tags.

```
character> This is a random line # tag1, tag2, "This is comment 1", "Comment 2"
```

 Tags, such as #happy and #skippable, serve as markers that can be leveraged by the game's code to implement tailored behaviors associated with specific nodes. For instance, these tags may trigger unique character reactions or prompt specific in-game events. 
 
 Additionally, tags offer a valuable space for insights related to a dialogue line. Use comments to provide voiceover tips, offer guidance for effective delivery, or even explain the nuances of a particular word to assist translators.

Tags can be added to Dialogue Titles block too:
```
§ Title # tag1, tag2, "Comment"
```

# Samwise Grammar

The Samwise parser is not generated from a formal grammar but is hand-written. In this section, just for descriptive purposes, I add a possible grammar definition. However, the recommended way to parse a Samwise file is by using the provided runtime - since rewriting a parser from the grammar reference alone (instead of taking as reference the official parser's code) could lead to incompatibilities.

!> Warning: the following grammar definition is just for descriptive purposes. It wasn't tested in any parser generator. 

!> Warning: NOT UPDATED TO REFLECT NEW FEATURES

```
NAME                : [a-zA-Z0-9_\-];
NUMBERS             : [0-9];
NUMBER_INT          : '-' | NUMBERS;
BOOLEAN_VARIABLE    : 'b' NAME;
INTEGER_VARIABLE    : 'i' NAME;
SYMBOL_VARIABLE     : 's' NAME;
TITLE_LABEL         : UPPERCASE (UPPERCASE | LOWERCASE | NUMBERS)*;
NODE_LABEL          : LOWERCASE (UPPERCASE | LOWERCASE | NUMBERS)*;
SYMBOL              : UPPERCASE;
MULTIPLICITY        : NUMBER_INT 'x';
TIME                : NUMBER_INT ('.' NUMBER_INT)? 's';
ENDL                : '\n';
EQUAL               : '==';
DIFFERENT           : '!=';
LESS                : '<';
LESSEQUAL           : '<=';
GREATER             : '>';
GREATEREQUAL        : '>=';
AND                 : '&';
OR                  : '|';
OP_ADD              : '+';
OP_SUB              : '-';
OP_MUL              : '*';
OP_DIV              : '/';
DASH                : '-';
DASH_DOUBLE         : '--';
DASH_RETURN         : '<';

// INDENT           this token should be emitted when there's an indentation advance compared to the previous line
// DEDENT           this token should be emitted when there's a return to the previous line's indentation
// FREE_STRING      any character except '\n' and # (unless escaped)
// QUOTED_STRING    string inside " ", supporting \"
// EXTERNAL_CODE    any character until unmatched '}'
// COMMENT          // and trailing characters

samwise_file        : (dialogue)*;

dialogue            : title_header node_block;

node_block          : E | node (ENDL node)* ENDL?;

title_header        : (adornment ENDL)* adornment title_label? title? adornment? ENDL (adornment ENDL)*;
adornment           : [§¶‗~¯«»±─┬┼│┤├┴┐┘┌└═╦╬║╣╠╩╗╝╔╚░▒▓█▀■▄]+;

title_label         : '(' TITLE_LABEL ')';
label               : '(' NODE_LABEL ')';
title               : TITLE_STRING;

node                : label? attributes? node_payload;

node_payload        : speech_node 
                    | caption_node
                    | choice_node
                    | check_node
                    | catch_node
                    | recatch_node
                    | fallback_node
                    | score_node
                    | clamp_node
                    | loop_node
                    | pingpong_node
                    | jump_node
                    | fork_node
                    | anon_fork_node
                    | join_node
                    | await_node
                    | wait_node
                    | code_node
                    | external_code_node
                    | conditional_node
                    ;

speech_node         : NAME '>' FREE_STRING tags?;
caption_node        : '*' FREE_STRING tags?;
choice_node         : NAME ':' tags? ENDL INDENT choice_option+ DEDENT;
choice_option       : DASH_RETURN? (DASH | DASH_DOUBLE) attributes tags? ENDL INDENT node_block DEDENT;
check_node          : '$' NAME tags? ENDL INDENT check_case* DEDENT;
check_case          : pass_case | fail_case;
pass_case           : '+' tags? ENDL INDENT node_block DEDENT;
fail_case           : DASH tags? ENDL INDENT node_block DEDENT;
catch_node          : '!' boolean_variable tags? ENDL INDENT node_block* DEDENT;
recatch_node        : '!!' boolean_variable tags? ENDL INDENT node_block* DEDENT;
fallback_node       : '?' tags? ENDL INDENT election_case* DEDENT;
score_node          : '%' tags? ENDL INDENT selection_case* DEDENT;
loop_node           : '>>' integer_variable tags? ENDL INDENT selection_case* DEDENT;
clamp_node          : '>>>' integer_variable tags? ENDL INDENT selection_case* DEDENT;
pingpong_node       : '><' integer_variable tags? ENDL INDENT selection_case* DEDENT;
selection_case      : DASH attributes tags? ENDL INDENT node_block DEDENT;
jump_node           : "->" target_label tags?;
fork_node           : NAME? "=>" target_label tags?;
anon_fork_node      : NAME? "=>" tags? ENDL INDENT node_block DEDENT;
join_node           : NAME? "<=" tags?;
await_node          : "<=>" target_label tags?
                    | "<=>" external_code_node tags?;
external_code_node  : "{{" EXTERNAL_CODE "}} tags?";
code_node           : "{" code "} tags?";

target_label        : (NODE_LABEL | TITLE_LABEL | (TITLE_LABEL '.' NODE_LABEL));

code                : assignment
                    ;

assignment          : boolean_variable '=' boolean_expression
                    | integer_variable '=' integer_expression
                    ;

 // attributes here is repeated because it's mandatory
conditional_node    : attributes node_line ENDL INDENT node_block // inline
                    | attributes ENDL INDENT node_block
                    ;

attributes          : "else"? boolean_expression
                    | integer_expression
                    | "check" NAME
                    | "precheck" NAME
                    | MULTIPLICITY
                    | TIME
                    ;

wait_node           : "wait " TIME;

tags                : '#' tags_elements;
tags_elements       : tag (',' tags_elements)?;

tag                 : NAME | QUOTED_STRING | named_tag | named_quoted_tag;
named_tag           : NAME '=' NAME;
named_quoted_tag    : NAME '=' QUOTED_STRING;

boolean_expression  : "true" 
                    | "false" 
                    | boolean_variable
                    | once_expression 
                    | NOT boolean_expression
                    | number_comparison
                    | symbol_comparison
                    | boolean_expression binary_op_bool boolean_expression
                    | '(' boolean_expression ')'
                    ;

integer_expression  : NUMBER_INT
                    | integer_variable
                    | OP_SUB integer_expression
                    | integer_expression binary_op_number integer_expression
                    | '(' integer_expression ')'
                    ;

symbol_expression   : SYMBOL
                    | symbol_variable
                    ;

symbol_comparison   : SYMBOL binary_op_symbol SYMBOL
                    | SYMBOL binary_op_symbol symbol_variable
                    | symbol_variable binary_op_symbol SYMBOL
                    ;

number_comparison   : number_expression binary_op_comparison number_expression
                    | number_expression binary_op_comparison variable_expression
                    | variable_expression binary_op_comparison number_expression
                    ;

binary_op_comparison: EQUAL 
                    | DIFFERENT 
                    | LESS 
                    | LESSEQUAL 
                    | GREATER 
                    | GREATEREQUAL
                    ; 

binary_op_bool      : EQUAL 
                    | DIFFERENT 
                    | AND 
                    | OR
                    ; 

binary_op_number    : OP_ADD 
                    | OP_SUB 
                    | OP_MUL 
                    | OP_DIV
                    ;

binary_op_symbol    : EQUAL 
                    | DIFFERENT
                    ; 

once_expression     : 'once' ( '(' boolean_variable ')' )?;

variable            : boolean_variable
                    | integer_variable
                    | symbol_variable
                    ;

boolean_variable    : (NAME '.')* BOOLEAN_VARIABLE;
integer_variable    : (NAME '.')* INTEGER_VARIABLE;
symbol_variable     : (NAME '.')* SYMBOL_VARIABLE;
```

Notes:
- the different syntax between "once(bVar)" and nodes/commands (e.g. ">> iVar") is not a inconsistency: the former is a boolean expression, the second is not.
Parenthesis are only used in expressions.
