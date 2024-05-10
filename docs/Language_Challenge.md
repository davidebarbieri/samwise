# Advanced Features

!> [ This page is under development ]


## Challenge Checks


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