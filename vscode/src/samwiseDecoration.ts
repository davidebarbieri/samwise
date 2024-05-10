import * as vscode from 'vscode';
import * as samwiseVM from './samwiseVM';

import { LocationInfo } from './common';
import { titleRegex, contentRegex, wholeTagRegex, idTagRegex } from './common';

export enum HideMetaState { show, hideId, hideAll }
let hideMetaState: HideMetaState = HideMetaState.show;

export function activate(context: vscode.ExtensionContext) {
	dialoguePositionBalloonDecorationType = vscode.window.createTextEditorDecorationType({
		isWholeLine: true,
		gutterIconPath: context.asAbsolutePath('webview-ui\\gutterDialogue.png')
	});
}

export function deactivate() {
}

export function setHideMetaState(state: HideMetaState) {
	hideMetaState = state;
	refreshCodeDecoration();
}

const topTitleDecorationType = vscode.window.createTextEditorDecorationType({
	isWholeLine: true,
	rangeBehavior: vscode.DecorationRangeBehavior.ClosedClosed,
	borderStyle: 'solid',
	borderColor: '#66f',
	borderWidth: '1px 0px 0px 0px',
});

const titleDecorationType = vscode.window.createTextEditorDecorationType({
	isWholeLine: true,
	rangeBehavior: vscode.DecorationRangeBehavior.ClosedClosed,
	backgroundColor: '#6666FF15',
	overviewRulerColor: '#6666FF',
	overviewRulerLane: 7,
});

const endNodeDecorationType = vscode.window.createTextEditorDecorationType({
	isWholeLine: false,
	rangeBehavior: vscode.DecorationRangeBehavior.ClosedClosed,
	borderStyle: 'dashed',
	borderColor: '#dd3',
	borderWidth: '0px 0px 1.4px 0px',
	overviewRulerColor: '#dd3',
	overviewRulerLane: 2
});

const partialHideDecorationType = vscode.window.createTextEditorDecorationType({
	isWholeLine: false,
	rangeBehavior: vscode.DecorationRangeBehavior.ClosedOpen,
	textDecoration: "none; display: none",
	before: {
		contentText: " …",
		color: '#0a0',
	}
});

const hideDecorationType = vscode.window.createTextEditorDecorationType({
	isWholeLine: false,
	rangeBehavior: vscode.DecorationRangeBehavior.ClosedOpen,
	textDecoration: "none; display: none",
	before: {
		contentText: ""
	}
});

const dialoguePositionDecorationType = vscode.window.createTextEditorDecorationType({
	isWholeLine: true,
	backgroundColor: "#ffff0011",
	overviewRulerColor: "#ffff00",
	overviewRulerLane: vscode.OverviewRulerLane.Full
});

let dialoguePositionBalloonDecorationType: vscode.TextEditorDecorationType;

export function refreshRunningDialoguesDecoration(contexesPosition: Map<number, LocationInfo>) {
	const editor = vscode.window.activeTextEditor;

	if (editor === undefined || editor.document.languageId !== "samwise") {
		return;
	}

	if (!samwiseVM.isInitialized()) {
		return;
	}

	let runningArray: vscode.DecorationOptions[] = [];
	let runningBalloonArray: vscode.DecorationOptions[] = [];

	for (let pos of contexesPosition) {
		const location = pos[1];

		if (editor.document.fileName === location.file) {
			let firstRange = new vscode.Range(
				new vscode.Position(location.lineStart - 1, 0),
				new vscode.Position(location.lineStart - 1, 0)
			);

			let range = new vscode.Range(
				new vscode.Position(location.lineStart - 1, 0),
				new vscode.Position(location.lineEnd - 1, 0)
			);

			runningBalloonArray.push({ range: firstRange });
			runningArray.push({ range });
		}
	}

	editor.setDecorations(dialoguePositionBalloonDecorationType, runningBalloonArray);
	editor.setDecorations(dialoguePositionDecorationType, runningArray);
}

export function refreshCodeDecoration() {

	const editor = vscode.window.activeTextEditor;

	if (editor === undefined || editor.document.languageId !== "samwise") {
		return;
	}

	if (!samwiseVM.isInitialized()) {
		return;
	}

	let sourceCode = editor.document.getText();

	let topTitlesArray: vscode.DecorationOptions[] = [];
	let titlesArray: vscode.DecorationOptions[] = [];
	let endNodesArray: vscode.DecorationOptions[] = [];
	let hideArray: vscode.DecorationOptions[] = [];
	let partialHideArray: vscode.DecorationOptions[] = [];

	const sourceCodeArr = sourceCode.split('\n');

	let prevWasTitle = false;
	//let prevDialogue = "";
	for (let line = 0; line < sourceCodeArr.length; line++) {
		let filename: string = editor.document.fileName;
		//let dialogue = samwiseVM.getDialogueSymbolFromLine(filename, line);

		let match = sourceCodeArr[line].match(titleRegex);

		if (match !== null && match.index !== undefined) {
			let range = new vscode.Range(
				new vscode.Position(line, match.index),
				new vscode.Position(line, match.index + match[1].length)
			);

			let decoration = { range };

			titlesArray.push(decoration);
			if (!prevWasTitle) {
				topTitlesArray.push(decoration);
			}

			prevWasTitle = true;
		}
		else {
			prevWasTitle = false;
		}

		// don't apply to selected line
		if (line !== editor.selection.active.line) {
			let tagsStart = -1;

			if (hideMetaState === HideMetaState.hideAll) {
				match = sourceCodeArr[line].match(wholeTagRegex);
				if (match !== null && match.index !== undefined) {
					let range = new vscode.Range(
						new vscode.Position(line, tagsStart = match.index + match[1].length),
						new vscode.Position(line, match.index + match[1].length + match[2].length)
					);
					hideArray.push({ range });
				}
			}
			else if (hideMetaState === HideMetaState.hideId) {
				match = sourceCodeArr[line].match(idTagRegex);
				if (match !== null && match.index !== undefined) {

					// Whole line if ID is the only tag
					if (match[3].length === 0 && match[5].length === 0) {
						let matchWhole = sourceCodeArr[line].match(wholeTagRegex);
						if (matchWhole !== null && matchWhole.index !== undefined) {
							let range = new vscode.Range(
								new vscode.Position(line, tagsStart = matchWhole.index + matchWhole[1].length),
								new vscode.Position(line, matchWhole.index + matchWhole[1].length + matchWhole[2].length)
							);
							hideArray.push({ range });
						}
					}
					else {
						let range = new vscode.Range(
							new vscode.Position(line, (tagsStart = match.index + match[1].length) + match[2].length),
							new vscode.Position(line, match.index + match[1].length + match[2].length + match[4].length)
						);
						partialHideArray.push({ range });
					}
				}
			}

			if (samwiseVM.isExitingNode(filename, line + 1)) {
				if (line !== editor.selection.active.line - 1) // nor previous
				{
					let match = sourceCodeArr[line].match(contentRegex);
					if (match !== null && match.index !== undefined) {
						const endLinePosition = match.index + match[1].length + match[2].length;

						let range = new vscode.Range(
							new vscode.Position(line, match.index + match[1].length),
							new vscode.Position(line, tagsStart >= 0 ? Math.min(tagsStart - 1, endLinePosition) : endLinePosition)
						);
						endNodesArray.push({ range });
					}
				}
			}
		}
	}

	editor.setDecorations(topTitleDecorationType, topTitlesArray);
	editor.setDecorations(titleDecorationType, titlesArray);
	editor.setDecorations(endNodeDecorationType, endNodesArray);
	editor.setDecorations(hideDecorationType, hideArray);
	editor.setDecorations(partialHideDecorationType, partialHideArray);
}