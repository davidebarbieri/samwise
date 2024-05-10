# Selection Nodes

!> [ This page is under development ]

## Fallbacks List Node

A Fallback node, as opposed to Choice node, generates branching narrative based on logical conditions, instead of user selection.
```samwise
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
```samwise
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

## Score Node

The Score node stands out for its versatility as a selection node. It offers the flexibility to choose the child block based on the highest integer expression, select a child block at random, or employ a combination of both methods.

For the first purpose (highest score selection), it takes an integer expression as attribute. Such expression is called *score*:

```samwise
%
    - [iTalkedWithJohn + iHitCount]
        <nodes>
    - [iTalkedWithJohn, bCondition]
        <nodes>
    -
        <nodes>
```

By default, if the node lacks an integer expression, it is automatically assigned a score of 0.

This is how the selection operates:

1. All elements with a true condition are collected.
2. The score of each of these elements is evaluated.
3. Among those with the highest score (the same value), one is randomly selected.

> This means that the node can easily be used as a pure **Random Node**: simply by not using any integer expression. In that case, all elements will have an equal score of 0, and will be included for step 3.

Each child has an optional attribute that allows the writer to specify how much probable is a selection.  
A value of **5x** means that a child node is five times more probable than one with **1x** (which is the default value).

```samwise
%
    - [5x]
        <nodes>
    - [2x]
        <nodes>
    -
        <nodes>
```

!> However, it's important to note that the actual probability of selection is influenced by which other elements have been selected during steps 1 and 2.

## Loop Node

The Loop node directs a dialogue towards a different child node each time it's visited.
Each child node can have an optional condition. If the selected node's condition is false,
the next child node is evaluated, and so on.
When the last node is selected/evaluated, the process will restart from the first child node.

```samwise
>> iVariable
    - [bCond1]
        <nodes>
    - [bCond2]
        <nodes>
```

The iVariable is an integer variable that stores the incremental counter used to loop between nodes.

## Clamp Node
The Clamp node works in the same way as a Loop node. The only difference is that once the last node is reached, the node will continue to select/evaluate the last child node. 

```samwise
>>> iVariable
    - [bCond1]
        <nodes>
    - [bCond2]
        <nodes>
```