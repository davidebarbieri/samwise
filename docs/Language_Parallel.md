# Parallel Dialogues

!> [ This page is under development ]

### Fork node

The execution model of Samwise allows the possibility to run multiple dialogues in the same time. When the execution of a dialogue reaches a fork node, it branches off in parallel. What happens is that the former dialogue continues past the fork point, while a brand new dialogue execution will start at the designated point.

```
=> label
=> Dialogue.label
```

A fork node can optionally define a name for its fork point. Such name can be used later in Join/Await nodes (further explanation in their related sections).

```
name => label 
name => Dialogue.label 
```

#### Anonymous Fork node

It's possible to have anonymous fork nodes, that is you can create a whole subtree in the mid of a dialogue in this way:
```
name =>
    <nodes A>
<nodes B>
```
This will make the flow fork: the main flow will continue executing "nodes B",
while the forked dialogue will execute the subtree in the "nodes A" block. Of course, the name is optional as in normal fork nodes.


### Join node

If a Fork node makes the execution of a dialogue to split up in two parallel directions, the Join node is used to make two dialogues to restore their sequential execution. More precisely, a dialogue that reaches a Join node will be paused until the execution (forked using the name <i>name</i>) is completed.

```
name <=
```

If no name is specified, as in 

```
<=
```
then, the dialogue will wait all the previously fork nodes.

### Await node (Fork + Join)

The Await node is equivalent to a Fork node followed by a Join node. When a dialogue execution reaches this node, it will pause, the target dialogue will be started, and the former dialogue will be restored only once the child dialogue is completed.

```
<=> label
<=> Dialogue.label
```






cite Oxenfree



```samwise

```




### Catch node

Catch nodes are particularly beneficial in scenarios where multiple dialogues are active simultaneously. During the execution of a dialogue, if any node within the node subtree is being processed, the system will continuously monitor a specified variable's value. Should this value be true, the execution is immediately halted, and the system will skip the remainder of the subtree, moving on as defined.

```samwise
! @bVariable
    <nodes>
```

!> Warning: A Catch node supports global variables only

Given that the entire block subject to interruption is bypassed if the variable is already true, manually resetting the variable to false before its next use can become cumbersome. To streamline this process, the Reset and Catch node can be employed.
```samwise
!! @bVariable
    <nodes>
```
The behavior mirrors that of a Catch node, with the critical distinction being that the variable is automatically reset upon the node's execution.

#### Catch and subcontexes

If a fork occurs from a node within an interruptible block, the sub-context that is created will also be subjected to that check. The test check will be carried out at each node of the sub-context. If the check fails (i.e., the tested variable is true), the sub-context will be stopped.