# Unity Plug-in

!> This page is under development

## SamwiseDatabase



## SamwiseReference

The SamwiseReference attribute can be used with string fields in MonoBehaviour and ScriptableObjects to provide a simple way to select a specific dialogue from the Inspector. When this attribute is applied, the Inspector will display a drop-down menu with all the dialogues present in the project.

```C#
[SamwiseReference]
public string DialogueToLaunch;
```