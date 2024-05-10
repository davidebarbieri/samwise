# Quick start

## Installation

### Visual Studio Code
1. If you haven't already, download and install [Visual Studio Code](https://code.visualstudio.com/).
2. Open Visual Studio Code.

### Samwise Dialogue System Extension
Install Samwsie extension by clicking [here](vscode:extension/Peevo.samwise).

Alternatively:
1. In Visual Studio Code, go to the Extensions view by clicking on the square icon on the left sidebar or pressing `Ctrl+Shift+X`.
2. Search for "Samwise Dialogue System" in the Extensions Marketplace.
3. Click on the "Install" button next to the "Samwise Dialogue System" extension.

## Getting Started

### Creating a .sam File
1. Create a new file in Visual Studio Code.
2. Save the file with a `.sam` extension (e.g., `quickstart.sam`).

### Accessing the Extension
1. Look for the Samwise logo on the Activity Bar (left sidebar) of Visual Studio Code.
2. Click on the Samwise logo to access the extension controls.

### Writing Dialogues
1. Samwise's basic narrative unit is a Dialogue, which consists of a title, a label, and a set of nodes.
2. To create a Dialogue, use the following format:
```samwise
§ (Label) Title
```
- `§` The character used to tell Samwise that this is a Dialogue.
- `(Label)`: The label used to reference the dialogue in the game's code and in other dialogues.
- `Title`: An optional descriptive title for the Dialogue.

### Adding Dialogue Nodes
After defining the dialogue header, you can add nodes that compose the dialogue.
Nodes can include simple dialogue lines, choice nodes, flow control nodes, and more.

### Toolbox Shortcuts
1. In the Toolbox panel, you can find shortcuts to create basic dialogue nodes quickly. As you'll notice, there's a fair number of nodes available in Samwise. Each one offers you a different tool for building your interactive narrative.
2. Click on the corresponding button to paste a node into your text.

### Example Dialogue

This example demonstrates a simple dialogue with multiple dialogue nodes.

```samwise
§ (MyDialogue) My first dialogue

Rose> I'm not going without you.
Jack> Get in the boat, Rose.
* Cal walks up just then.
Cal> Yes. Get in the boat, Rose.
```

The nodes in this form are spoken dialogue nodes:

```samwise
Character> Spoken line
```

- `Character`: The character speaking the line
- `Spoken line`: The dialogue spoken by the character.

While the nodes in the following form represent a narration that does not originate from a specific character:

```samwise
* This is a caption 
```

### Running a Dialogue

When a dialogue is selected in the editor, the 'Player' panel in the Samwise extension allows
you to launch that dialogue. You can launch the dialogue from the start or right from the selected point.

In order to do this click on the "Run" button, or the "Run from node" button.

In the same panel, you'll see chat *balloons* appearing, requesting input from the user.
Those balloons are the preview of the dialogue being executed; by clicking on "Advance", when requested, the flow is progressed. Obviously, upon reaching a choice node, instead of having the simple "Advance" there will be a set of buttons that represent the available options.

### Conclusion
You're now ready to create basic dialogues using the Samwise Dialogue System in Visual Studio Code. Now, the only thing remaining is for you to master the Samwise language to craft your interactive story: [The Samwise Language](Language_Basics.md).