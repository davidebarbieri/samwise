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

const disableDecorationType = vscode.window.createTextEditorDecorationType({
	light: {
		color: "#AAAAAA"
	},
	dark: {
		color: "#666666"
	}
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
	let disableArray: vscode.DecorationOptions[] = [];

	const sourceCodeArr = sourceCode.split('\n');

	let prevWasTitle = false;
	//let prevDialogue = "";
	for (let line = 0; line < sourceCodeArr.length; line++) {
		let filename: string = editor.document.fileName;
		//let dialogue = samwiseVM.getDialogueSymbolFromLine(filename, line);

		// Skip disabled lines
		let multiDisableStartRegex = /^[\t ]*\/~[\t ]*[\r\n]?$/;
		let multiDisableEndRegex = /^[\t ]*~\/[\t ]*[\r\n]?$/;

		let lineString = "";

		let openedBrackets = 0;
		let disableStart = line;
		let disableEnd = line;
		let hasDisabledText = false;

		// Check multi-line disablers
		do {
			lineString = sourceCodeArr[line];

			let multiStartMatch = lineString.match(multiDisableStartRegex);
			if (multiStartMatch !== null && multiStartMatch.index !== undefined) {
				++openedBrackets;
				hasDisabledText = true;
			}

			if (openedBrackets > 0 && line < sourceCodeArr.length - 1) {

				let multiEndMatch = lineString.match(multiDisableEndRegex);
				if (multiEndMatch !== null && multiEndMatch.index !== undefined) {
					--openedBrackets;
				}
			}

			disableEnd = line;

		} while (openedBrackets > 0 && (++line) < sourceCodeArr.length);

		// Check single-line disablers
		let indentRegex = /^[\t ]*/;
		let singleDisableStartRegex = /^([\t ]*)~/;
		let singleDisableContinueRegex = /↵[\t ]*[\r\n]?$/;
		if (!hasDisabledText) {
			let singleStartMatch = lineString.match(singleDisableStartRegex);
			if (singleStartMatch !== null && singleStartMatch.index !== undefined) {
				hasDisabledText = true;

				// disable all multi-line nodes
				let continued = false;

				if (line < sourceCodeArr.length - 1) {
					do {
						// Check if ↵ or indent or empty line
						const nextLine = sourceCodeArr[line + 1];
						let continueIndentedMatch;
						const isIndentedOrEmpty = !nextLine.trim() || 
							(continueIndentedMatch = nextLine.match(indentRegex)) !== null && continueIndentedMatch.includes !== undefined &&
							continueIndentedMatch[0].length > singleStartMatch[1].length;

						let singleContinueMatch;
						if (isIndentedOrEmpty || ((singleContinueMatch = lineString.match(singleDisableContinueRegex)) !== null && singleContinueMatch.index !== undefined)) {
							++disableEnd;
							++line;
							lineString = sourceCodeArr[line];
							continued = true;
						}
						else {
							continued = false;
						}
					} while (continued);
				}
			}
		}

		if (hasDisabledText) {
			let range = new vscode.Range(
				new vscode.Position(disableStart, 0),
				new vscode.Position(disableEnd + 1, 0)
			);

			disableArray.push({ range });
		}

		// Match Title
		let titleMatch = lineString.match(titleRegex);

		if (titleMatch !== null && titleMatch.index !== undefined) {
			let range = new vscode.Range(
				new vscode.Position(line, titleMatch.index),
				new vscode.Position(line, titleMatch.index + titleMatch[1].length)
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
				titleMatch = lineString.match(wholeTagRegex);
				if (titleMatch !== null && titleMatch.index !== undefined) {
					let range = new vscode.Range(
						new vscode.Position(line, tagsStart = titleMatch.index + titleMatch[1].length),
						new vscode.Position(line, titleMatch.index + titleMatch[1].length + titleMatch[2].length)
					);
					hideArray.push({ range });
				}
			}
			else if (hideMetaState === HideMetaState.hideId) {
				titleMatch = lineString.match(idTagRegex);
				if (titleMatch !== null && titleMatch.index !== undefined) {

					// Whole line if ID is the only tag
					if (titleMatch[3].length === 0 && titleMatch[5].length === 0) {
						let matchWhole = lineString.match(wholeTagRegex);
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
							new vscode.Position(line, (tagsStart = titleMatch.index + titleMatch[1].length) + titleMatch[2].length),
							new vscode.Position(line, titleMatch.index + titleMatch[1].length + titleMatch[2].length + titleMatch[4].length)
						);
						partialHideArray.push({ range });
					}
				}
			}

			if (samwiseVM.isExitingNode(filename, line + 1)) {
				if (line !== editor.selection.active.line - 1) // nor previous
				{
					let match = lineString.match(contentRegex);
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
	editor.setDecorations(disableDecorationType, disableArray);
}