# Unity Plug-in

!> This page is under development

## Download the package

To install the package:
1. Open Unity Package Manager

Start by opening your Unity project.
Navigate to Window > Package Manager to open the Package Manager.

2. Add Package from Git URL

In the Package Manager window, you will see a + button at the top left corner. Click on it.
From the dropdown menu, select Add package from git URL....

3. Enter the Git URL

A dialog box will appear asking for the Git URL. Enter the URL of the Git repository where the package is hosted.
Ensure the URL ends with .git. For example: https://github.com/davidebarbieri/samwiseUnity.git.
Click 'Add'.

4. Wait for Unity to Import the Package

Unity will now download and import the package into your project. This might take a few minutes depending on the size of the package and your internet connection.


## Dialogue Assets

The .sam files are automatically imported by a ScriptedImporter, thus it is possible to simply keep the Samwise files within the Unity project and modify them as desired.

## SamwiseDatabase

The SamwiseDatabase is a ScriptableObject asset that can be created in any arbitrary location within the Assets folder (right-click, Create -> Samwise -> AssetDatabase). After its creation, all .sam files imported into that folder and its subfolders will be added to the database's references.

The SamwiseDatabase is an IDialogueSet, and therefore can be passed to the constructor of DialogueMachine. These are its methods:

```C#
public bool GetDialogue(string dialogueLabel, out Dialogue dialogue)
```
Arguments:
- **dialogueLabel**: the label of the dialogue that needs to be found.
- *out* **dialogue**: the output of the method

Return value: true if found

```C#
public IDialogueNode GetNodeFromLabel(string dialogueLabel, string label) 
```
Arguments:
- **dialogueLabel**: the label of the dialogue that needs to be found.
- **label**: the name of the label local to [dialogueLabel]

Return value: the node referenced by [dialogueLabel].[label]

```C#
public bool Contains(Dialogue dialogue)
```
Arguments:
- **dialogue**: the dialogue to check.

Return value: true if the dialogue reference is already in the database.


## SamwiseReference

The SamwiseReference attribute can be used with string fields in MonoBehaviour and ScriptableObjects to provide a simple way to select a specific dialogue from the Inspector. When this attribute is applied, the Inspector will display a drop-down menu with all the dialogues present in the project.

```C#
[SamwiseReference]
public string DialogueToLaunch;
```