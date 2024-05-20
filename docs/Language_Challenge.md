# Advanced Features

## Challenge Checks

The challenge check is a node that allows the dialogue to ask the game to check the skills of the player,
and branching the dialogue based on the result of the check.
Such challenge can be anything, for example a QTE, the verification of some player statistics, the throw of some dices, or just a random result.
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

In Disco Elysium, when a Challenge Check occurs, it involves rolling two six-sided dice (2d6) and adding the relevant skill level to the roll. The total is then compared to a target difficulty number (DC) that determines success or failure. The target number varies depending on the complexity and difficulty of the task at hand.

Several factors can influence the outcome of Challenge Checks:

- Skill Levels: Your character's skill levels are crucial in determining the success of a check. Higher skill levels increase your chances of rolling a successful outcome.
- Modifiers: Certain items, thoughts, and character states can provide bonuses or penalties to your rolls, impacting your overall performance in checks.
- Luck: The inherent randomness of rolling dice means that luck plays a significant role in the outcome of checks, adding unpredictability to the game.


The results of Challenge Checks significantly influence the narrative and gameplay:

- Success: Successfully passing a check can open up new dialogue options, provide valuable information, unlock new areas, or grant other benefits that advance the story.
- Failure: Failing a check can lead to missed opportunities, negative consequences, or alternative story paths. In some cases, failure can also create interesting and unexpected narrative developments.