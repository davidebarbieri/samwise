// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

import * as vscode from 'vscode';

const sentences = ["It is certain.", "It is decidedly so.", "Without a doubt.", "Yes definitely.", "You may rely on it.", "As I see it, yes.", "Most likely.", "Outlook good.", "Yes.", "Signs point to yes.", "Reply hazy, try again.", "Ask again later.", "Better not tell you now.", "Cannot predict now.", "Concentrate and ask again.", "Don't count on it.", "My reply is no.", "My sources say no.", "Outlook not so good.", "Very doubtful."];

import type { LocationInfo } from "./common";

export async function showTextLine(location: LocationInfo) {
	if (location) {
		const document = await vscode.workspace.openTextDocument(location.file);
		const editor = await vscode.window.showTextDocument(document);

		const position = new vscode.Position(location.lineStart - 1, 0);
		editor.selection = new vscode.Selection(position, position);
		editor.revealRange(new vscode.Range(position, position));
	}
}

export function getActiveIndentatedPosition(editor: vscode.TextEditor) {
	var position = editor.selection.active;

	const line = editor.document.lineAt(position.line);
	const indentation = line.firstNonWhitespaceCharacterIndex;

	return position.with(position.line, indentation);
}


export function getRandomSentence() {
	return sentences[Math.floor(Math.random() * sentences.length)];
}

export function addDialogue() {
	const editor = vscode.window.activeTextEditor;

	if (editor) {
		const snippetString = new vscode.SnippetString().appendText("§ ").appendPlaceholder("New Dialogue").appendText("\n");

		var position = editor.selection.active;
		position = position.with(position.line, 0);

		editor.insertSnippet(snippetString, position);
	}
}

export function addCarriageReturn() {
	const editor = vscode.window.activeTextEditor;

	if (editor) {
		const snippetString = new vscode.SnippetString().appendText("↵\n");

		var position = editor.selection.active;
		editor.insertSnippet(snippetString, position);
	}
}

export function addSpeech() {
	const editor = vscode.window.activeTextEditor;

	if (editor) {
		const snippetString = new vscode.SnippetString().appendPlaceholder("person").appendText("> ").appendPlaceholder(getRandomSentence()).appendText("\n");
		editor.insertSnippet(snippetString, getActiveIndentatedPosition(editor));
	}
}

export function addCaption() {
	const editor = vscode.window.activeTextEditor;

	if (editor) {
		const snippetString = new vscode.SnippetString().appendText("* ").appendPlaceholder(getRandomSentence()).appendText("\n");
		editor.insertSnippet(snippetString, getActiveIndentatedPosition(editor));
	}
}

export function addChoice() {
	const editor = vscode.window.activeTextEditor;

	if (editor) {
		const snippetString = new vscode.SnippetString().appendPlaceholder("person").appendText(":\n")
			.appendText("\t- ").appendPlaceholder(getRandomSentence()).appendText("\n")
			.appendText("\t- ").appendPlaceholder(getRandomSentence()).appendText("\n")
			.appendText("\t-- ").appendPlaceholder("This is an action")
			.appendText("\n");
		editor.insertSnippet(snippetString, getActiveIndentatedPosition(editor));
	}
}

export function addCheck() {
	const editor = vscode.window.activeTextEditor;

	if (editor) {
		const snippetString = new vscode.SnippetString().appendText("$ ").appendPlaceholder("checkName").appendText("\n")
			.appendText("\t+\n\t\t").appendPlaceholder("* Check Passed").appendText("\n")
			.appendText("\t-\n\t\t").appendPlaceholder("* Check Failed").appendText("\n")
			.appendText("\n");
		editor.insertSnippet(snippetString, getActiveIndentatedPosition(editor));
	}
}

export function addFallback() {
	const editor = vscode.window.activeTextEditor;

	if (editor) {
		const snippetString = new vscode.SnippetString().appendText("?\n")
			.appendText("\t- [").appendPlaceholder("bVar1").appendText("]\n")
			.appendText("\t\t* bVar1 was true, so this happened\n")
			.appendText("\t- [").appendPlaceholder("bVar2").appendText("]\n")
			.appendText("\t\t* bVar1 was false and bVar2 was true, so this happened\n")
			.appendText("\t-\n")
			.appendText("\t\t* bVar1 and bVar2 were both false, so this happened")
			.appendText("\n");
		editor.insertSnippet(snippetString, getActiveIndentatedPosition(editor));
	}
}

export function addRandom() {
	const editor = vscode.window.activeTextEditor;

	if (editor) {
		const snippetString = new vscode.SnippetString().appendText("%\n")
			.appendText("\t-\n\t\t* This can happen\n")
			.appendText("\t- [5x]\n\t\t* This is five times more probable to happen\n");
		editor.insertSnippet(snippetString, getActiveIndentatedPosition(editor));
	}
}

export function addScore() {
	const editor = vscode.window.activeTextEditor;

	if (editor) {
		const snippetString = new vscode.SnippetString().appendText("%\n")
			.appendText("\t- [100 * Stats.iDragonsKilled]\n\t\tNpc> Hail to you, Dragon Slayer!\n")
			.appendText("\t- [3 * Stats.iOrcsKilled]\n\t\tNpc> Greetings, Orcbuster!\n")
			.appendText("\t- [Stats.iImpsKilled]\n\t\tNpc> Ahoy, Impcatcher!\n")
			.appendText("\t- \n\t\tNpc> Hi.\n");
		editor.insertSnippet(snippetString, getActiveIndentatedPosition(editor));
	}
}

export function addSequenceSelection() {
	const editor = vscode.window.activeTextEditor;

	if (editor) {
		const snippetString = new vscode.SnippetString().appendText(">>> ").appendPlaceholder("@iCounter").appendText("\n")
			.appendText("\t-\n\t\t* Select this first\n")
			.appendText("\t-\n\t\t* Then this\n")
			.appendText("\t-\n\t\t* finally this, and continue to select this for all other times\n");
		editor.insertSnippet(snippetString, getActiveIndentatedPosition(editor));
	}
}

export function addPingPong() {
	const editor = vscode.window.activeTextEditor;

	if (editor) {
		const snippetString = new vscode.SnippetString().appendText(">< ").appendPlaceholder("@iCounter").appendText("\n")
			.appendText("\t-\n\t\t* Ping\n")
			.appendText("\t-\n\t\t* Pong\n")
			.appendText("\t-\n\t\t* Ping\n");
		editor.insertSnippet(snippetString, getActiveIndentatedPosition(editor));
	}
}

export function addLoopSelection() {
	const editor = vscode.window.activeTextEditor;

	if (editor) {
		const snippetString = new vscode.SnippetString().appendText(">> ").appendPlaceholder("@iCounter").appendText("\n")
			.appendText("\t-\n\t\t* Select this first\n")
			.appendText("\t-\n\t\t* Then this\n")
			.appendText("\t-\n\t\t* finally this, but the next time start over\n");
		editor.insertSnippet(snippetString, getActiveIndentatedPosition(editor));
	}
}


export function addGoto() {
	const editor = vscode.window.activeTextEditor;

	if (editor) {
		const snippetString = new vscode.SnippetString().appendText("-> ").appendPlaceholder("labelName").appendText("\n");
		editor.insertSnippet(snippetString, getActiveIndentatedPosition(editor));
	}
}

export function addFork() {
	const editor = vscode.window.activeTextEditor;

	if (editor) {
		const snippetString = new vscode.SnippetString().appendPlaceholder("forkPointName").appendText(" => ").appendPlaceholder("labelName").appendText("\n");
		editor.insertSnippet(snippetString, getActiveIndentatedPosition(editor));
	}
}

export function addJoin() {
	const editor = vscode.window.activeTextEditor;

	if (editor) {
		const snippetString = new vscode.SnippetString().appendPlaceholder("forkPointName").appendText(" <=").appendText("\n");
		editor.insertSnippet(snippetString, getActiveIndentatedPosition(editor));
	}
}

export function addCancel() {
	const editor = vscode.window.activeTextEditor;

	if (editor) {
		const snippetString = new vscode.SnippetString().appendPlaceholder("forkPointName").appendText(" <!=").appendText("\n");
		editor.insertSnippet(snippetString, getActiveIndentatedPosition(editor));
	}
}

export function addAwait() {
	const editor = vscode.window.activeTextEditor;

	if (editor) {
		const snippetString = new vscode.SnippetString().appendText("<=> ").appendPlaceholder("labelName").appendText("\n");
		editor.insertSnippet(snippetString, getActiveIndentatedPosition(editor));
	}
}


export function addCode() {
	const editor = vscode.window.activeTextEditor;

	if (editor) {
		const snippetString = new vscode.SnippetString().appendText("{ ").appendPlaceholder("bVariable = true").appendText(" }\n");
		editor.insertSnippet(snippetString, getActiveIndentatedPosition(editor));
	}
}


export function addComment() {
	const editor = vscode.window.activeTextEditor;

	if (editor) {
		const snippetString = new vscode.SnippetString().appendText("// ").appendPlaceholder("This is a comment").appendText("\n");
		editor.insertSnippet(snippetString, getActiveIndentatedPosition(editor));
	}
}


export function addLabel() {
	const editor = vscode.window.activeTextEditor;

	if (editor) {
		const snippetString = new vscode.SnippetString().appendText("(").appendPlaceholder("labelName").appendText(") ");
		editor.insertSnippet(snippetString, getActiveIndentatedPosition(editor));
	}
}

export function addTags() {
	const editor = vscode.window.activeTextEditor;

	if (editor) {
		const snippetString = new vscode.SnippetString().appendText(" # ").appendPlaceholder("tag1").appendText(", ").appendPlaceholder("tag2");

		var position = editor.selection.active;
		editor.insertSnippet(snippetString, editor.document.lineAt(position.line).range.end);
	}
}

export function addWait() {
	const editor = vscode.window.activeTextEditor;

	if (editor) {
		const snippetString = new vscode.SnippetString().appendText("{ wait ").appendPlaceholder("2.5").appendText("s }\n");
		editor.insertSnippet(snippetString, getActiveIndentatedPosition(editor));
	}
}

