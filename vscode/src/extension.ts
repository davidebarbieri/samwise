// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

const fs = require('fs');

import * as vscode from 'vscode';
import { SamwiseToolViewProvider } from './samwiseToolView';
import { SamwisePlayerViewProvider } from './samwisePlayerView';
import { SamwiseDataViewProvider } from './samwiseDataView';
import { SamwiseCompletionItemProvider } from './samwiseCompletion';
import { SamwiseDefinitionProvider } from './samwiseDefinitions';
import { SamwiseCodelensProvider } from './samwiseCodelens';
import { SamwiseFoldingRangeProvider, IndentFoldingRangeProvider } from './samwiseFolding';
import { SamwiseHoverProvider } from './samwiseHover';
import { HideMetaState } from './samwiseDecoration';
import * as decoration from './samwiseDecoration';

import type { Choice, ErrorInfo, ErrorsInfo, ReferenceInfo, SymbolInfo, LocationInfo } from "./common";
import { parseValue } from "./common";
import { registerEvent, unregisterEvent } from "./common";
import { SamwiseDocumentSymbolProvider } from './samwiseSymbols';

import * as samwiseVM from './samwiseVM';
import {
	addDialogue, addSpeech, addCaption, addFallback, addRandom, addScore, addChoice,
	addCheck, addSequenceSelection, addLoopSelection, addPingPong, addGoto, addTags, addWait,
	addLabel, addComment, addCode, addAwait, addJoin, addFork, addCancel, addCarriageReturn, toggleDisabledLines
} from './samwiseEditing';

let toolView: SamwiseToolViewProvider;
let playerView: SamwisePlayerViewProvider;
let dataView: SamwiseDataViewProvider;

let diagnosticCollection: vscode.DiagnosticCollection;

let contexesPosition: Map<number, LocationInfo> = new Map<number, LocationInfo>();

let lastSelectedDialogue: string = "";
let lastSelectedLine = -1;


let updateInterval: NodeJS.Timeout | undefined;

function parseCurrentFile(context: vscode.ExtensionContext) {
	const editor = vscode.window.activeTextEditor;

	if (editor === null || editor?.document.languageId !== "samwise") { return; }
	const filename = editor.document.fileName;

	if (!samwiseVM.parsedFiles.has(filename)) {
		processResult(editor, editor.document.uri, filename, samwiseVM.parseText(filename, editor.document.getText()));
	}

	decoration.refreshCodeDecoration();

	context.workspaceState.update('cachedSymbols', samwiseVM.serializeCachedSymbols());
}

export function activate(context: vscode.ExtensionContext) {
	console.log("Activating Samwise extension");

	registerEvent(samwiseVM.onDialogueContextEndEvent, onDialogueContextEnd);
	registerEvent(samwiseVM.onCaptionStartEvent, onCaptionStart);
	registerEvent(samwiseVM.onSpeechStartEvent, onSpeechStart);
	registerEvent(samwiseVM.onChoiceStartEvent, onChoiceStart);
	registerEvent(samwiseVM.onWaitTimeStartEvent, onWaitTimeStart);
	registerEvent(samwiseVM.onSpeechOptionStartEvent, onSpeechOptionStart);
	registerEvent(samwiseVM.onChallengeStartEvent, onChallengeStart);
	registerEvent(samwiseVM.onWaitForMissingDialogueStartEvent, onWaitForMissingDialogueStart);

	decoration.activate(context);

	samwiseVM.deserializeCachedSymbols(context.workspaceState.get('cachedSymbols'));

	function update() {
		samwiseVM.update();
	}

	updateInterval = setInterval(update, 100);

	samwiseVM.waitUntilInitialized().then(() => {
		parseCurrentFile(context);

	});

	context.subscriptions.push(vscode.workspace.onDidOpenTextDocument((e) => {
		if (e.languageId === 'samwise') {
			samwiseVM.parsedFiles.delete(e.fileName);

			playerView.onElementSelected("", false);
			parseCurrentFile(context);
		}
	}));

	context.subscriptions.push(vscode.workspace.onDidChangeTextDocument((e) => {
		if (e.document.languageId === 'samwise') {
			samwiseVM.parsedFiles.delete(e.document.fileName);

			//editor.onElementSelected("", false);

			parseCurrentFile(context);
			decoration.refreshRunningDialoguesDecoration(contexesPosition);
		}
	}));

	context.subscriptions.push(vscode.window.onDidChangeActiveTextEditor((e) => {
		playerView.onElementSelected("", false);
		refreshEditingStatus();

		parseCurrentFile(context);
		decoration.refreshRunningDialoguesDecoration(contexesPosition);
	}));


	context.subscriptions.push(vscode.window.onDidChangeTextEditorSelection((e) => {
		if (e.textEditor.document.languageId === 'samwise') {
			if (!samwiseVM.isInitialized()) {
				return;
			}

			let filename: string = e.textEditor.document.fileName;

			if (!samwiseVM.parsedFiles.has(e.textEditor.document.fileName)) {
				processResult(e.textEditor, e.textEditor.document.uri, filename, samwiseVM.parseText(filename, e.textEditor.document.getText()));

				decoration.refreshCodeDecoration();
			}

			const line = e.textEditor.selection.active.line + 1;
			const dialogueData = samwiseVM.getDialogueSymbolFromLine(filename, line);
			let hasSelectedNode: boolean = samwiseVM.isDialogueNodeFromLine(filename, line);
			let hasSelectedRef: boolean = samwiseVM.getDestinationSymbol(filename, line) !== "";

			vscode.commands.executeCommand('setContext', 'samwise.hasSelectedDialogue', (dialogueData !== ""));
			vscode.commands.executeCommand('setContext', 'samwise.hasSelectedNode', hasSelectedNode);
			vscode.commands.executeCommand('setContext', 'samwise.hasSelectedLabelReference', hasSelectedRef);


			playerView.onElementSelected(dialogueData, hasSelectedNode);

			//if (dialogueData !== lastSelectedDialogue)
			if (line !== lastSelectedLine) {
				decoration.refreshCodeDecoration();
				lastSelectedDialogue = dialogueData;
				lastSelectedLine = line;
			}
		}
	}));

	if (vscode.workspace.workspaceFolders) {
		parseWholeWorkspaceFolder(context);
	}

	context.subscriptions.push(vscode.workspace.onDidChangeWorkspaceFolders(event => {
		if (event.added.length > 0) {
			parseWholeWorkspaceFolder(context);
		}
	}));

	context.subscriptions.push(vscode.workspace.onDidDeleteFiles(event => {
		for (let file of event.files) {
			samwiseVM.onFileDeleted(file.fsPath);
		}
	}));

	context.subscriptions.push(vscode.workspace.onDidCreateFiles(event => {
		for (let file of event.files) {
			vscode.workspace.fs.readFile(file).then((fileData) => {
				const text = new TextDecoder("utf-8").decode(fileData);
				samwiseVM.parseText(file.fsPath, text);
			});
		}
	}));

	context.subscriptions.push(vscode.commands.registerCommand('samwise.runDialogue', () => {
		const activeEditor = vscode.window.activeTextEditor;
		if (activeEditor) {
			const file = activeEditor.document.fileName;
			let res: any = processResult(activeEditor, activeEditor.document.uri, file, samwiseVM.parseText(file, activeEditor.document.getText()));

			if (res.errors) {
				vscode.window.showInformationMessage('Cannot start dialogue. Fix errors before trying again.');
				return;
			}

			const line = activeEditor.selection.active.line + 1;
			const dialogueData = samwiseVM.getDialogueSymbolFromLine(file, line);

			if (dialogueData === "") {
				vscode.window.showInformationMessage('Cannot start dialogue');
			}
			else {
				let config: vscode.WorkspaceConfiguration = vscode.workspace.getConfiguration();
				const clearDialoguesOnPlay = config.get<boolean>('samwise.clearDialoguesOnPlay') ?? true;
				const clearDataOnPlay = config.get<boolean>('samwise.clearDataOnPlay') ?? true;

				if (clearDialoguesOnPlay) {
					samwiseVM.stopAll();
					playerView.onClearHistory();
				}

				if (clearDataOnPlay) {
					samwiseVM.clearData();
				}

				samwiseVM.startDialogue(dialogueData);
			}
		}

	}));

	context.subscriptions.push(vscode.commands.registerCommand('samwise.stopDialogue', (lineInfo) => {
		const line = lineInfo.lineNumber;
		const file = lineInfo.uri.fsPath;

		const activeEditor = vscode.window.activeTextEditor;
		if (activeEditor) {
			for (let pos of contexesPosition) {
				const location = pos[1];
				if (file === location.file && (line >= (location.lineStart) && line <= (location.lineEnd))) {
					samwiseVM.requestStop(pos[0]);
					break;
				}
			}
		}
	}));



	context.subscriptions.push(vscode.commands.registerCommand('samwise.runDialogueFromNode', () => {
		const activeEditor = vscode.window.activeTextEditor;
		if (activeEditor) {
			const file = activeEditor.document.fileName;
			let res: any = processResult(activeEditor, activeEditor.document.uri, file, samwiseVM.parseText(file, activeEditor.document.getText()));

			if (res.errors) {
				vscode.window.showInformationMessage('Cannot start dialogue. Fix errors before trying again.');
				return;
			}

			const line = activeEditor.selection.active.line + 1;

			let config: vscode.WorkspaceConfiguration = vscode.workspace.getConfiguration();
			const clearDialoguesOnPlay = config.get<boolean>('samwise.clearDialoguesOnPlay') ?? true;
			const clearDataOnPlay = config.get<boolean>('samwise.clearDataOnPlay') ?? true;

			if (clearDialoguesOnPlay) {
				samwiseVM.stopAll();
				playerView.onClearHistory();
			}

			if (clearDataOnPlay) {
				samwiseVM.clearData();
			}

			samwiseVM.startDialogueFromSrcPosition(file, line);

		}

	}));

	context.subscriptions.push(vscode.commands.registerCommand('samwise.stopDialogues', () => {
		samwiseVM.stopAll();

	}));

	context.subscriptions.push(vscode.commands.registerCommand('samwise.assignIdsToNodes', () => {
		const activeEditor = vscode.window.activeTextEditor;
		if (activeEditor) {
			const file = activeEditor.document.fileName;
			let res: any = processResult(activeEditor, activeEditor.document.uri, file, samwiseVM.parseText(file, activeEditor.document.getText()));

			if (res.errors) {
				vscode.window.showInformationMessage('Cannot parse dialogue. Fix errors before trying again.');
				return;
			}

			let newText = samwiseVM.assignIds(activeEditor.document.getText(), file, -1, false);
			let invalidRange = new vscode.Range(0, 0, activeEditor.document.lineCount, 0);
			let fullRange = activeEditor.document.validateRange(invalidRange);
			activeEditor.edit(edit => edit.replace(fullRange, newText));
		}

	}));

	context.subscriptions.push(vscode.commands.registerCommand('samwise.assignIdsToText', () => {
		const activeEditor = vscode.window.activeTextEditor;
		if (activeEditor) {
			const file = activeEditor.document.fileName;
			let res: any = processResult(activeEditor, activeEditor.document.uri, file, samwiseVM.parseText(file, activeEditor.document.getText()));

			if (res.errors) {
				vscode.window.showInformationMessage('Cannot parse dialogue. Fix errors before trying again.');
				return;
			}

			let newText = samwiseVM.assignIds(activeEditor.document.getText(), file, -1, true);
			let invalidRange = new vscode.Range(0, 0, activeEditor.document.lineCount, 0);
			let fullRange = activeEditor.document.validateRange(invalidRange);
			activeEditor.edit(edit => edit.replace(fullRange, newText));
		}

	}));

	context.subscriptions.push(vscode.commands.registerCommand('samwise.assignIdsToNodesDialogue', () => {
		const activeEditor = vscode.window.activeTextEditor;
		if (activeEditor) {
			const file = activeEditor.document.fileName;
			let res: any = processResult(activeEditor, activeEditor.document.uri, file, samwiseVM.parseText(file, activeEditor.document.getText()));

			if (res.errors) {
				vscode.window.showInformationMessage('Cannot parse dialogue. Fix errors before trying again.');
				return;
			}

			const line = activeEditor.selection.active.line + 1;
			let newText = samwiseVM.assignIds(activeEditor.document.getText(), file, line, false);
			let invalidRange = new vscode.Range(0, 0, activeEditor.document.lineCount, 0);
			let fullRange = activeEditor.document.validateRange(invalidRange);
			activeEditor.edit(edit => edit.replace(fullRange, newText));
		}

	}));

	context.subscriptions.push(vscode.commands.registerCommand('samwise.assignIdsToTextDialogue', () => {
		const activeEditor = vscode.window.activeTextEditor;
		if (activeEditor) {
			const file = activeEditor.document.fileName;
			let res: any = processResult(activeEditor, activeEditor.document.uri, file, samwiseVM.parseText(file, activeEditor.document.getText()));

			if (res.errors) {
				vscode.window.showInformationMessage('Cannot parse dialogue. Fix errors before trying again.');
				return;
			}

			const line = activeEditor.selection.active.line + 1;
			let newText = samwiseVM.assignIds(activeEditor.document.getText(), file, line, true);
			let invalidRange = new vscode.Range(0, 0, activeEditor.document.lineCount, 0);
			let fullRange = activeEditor.document.validateRange(invalidRange);
			activeEditor.edit(edit => edit.replace(fullRange, newText));
		}

	}));

	context.subscriptions.push(vscode.commands.registerCommand('samwise.replaceAnon', () => {
		const activeEditor = vscode.window.activeTextEditor;
		if (activeEditor) {
			const file = activeEditor.document.fileName;
			let res: any = processResult(activeEditor, activeEditor.document.uri, file, samwiseVM.parseText(file, activeEditor.document.getText()));

			if (res.errors) {
				vscode.window.showInformationMessage('Cannot parse dialogue. Fix errors before trying again.');
				return;
			}

			let newText = samwiseVM.replaceAnonElements(activeEditor.document.getText(), file, -1);
			let invalidRange = new vscode.Range(0, 0, activeEditor.document.lineCount, 0);
			let fullRange = activeEditor.document.validateRange(invalidRange);
			activeEditor.edit(edit => edit.replace(fullRange, newText));
		}

	}));


	context.subscriptions.push(vscode.commands.registerCommand('samwise.replaceAnonToDialogue', () => {
		const activeEditor = vscode.window.activeTextEditor;
		if (activeEditor) {
			const file = activeEditor.document.fileName;
			let res: any = processResult(activeEditor, activeEditor.document.uri, file, samwiseVM.parseText(file, activeEditor.document.getText()));

			if (res.errors) {
				vscode.window.showInformationMessage('Cannot parse dialogue. Fix errors before trying again.');
				return;
			}

			const line = activeEditor.selection.active.line + 1;
			let newText = samwiseVM.replaceAnonElements(activeEditor.document.getText(), file, line);
			let invalidRange = new vscode.Range(0, 0, activeEditor.document.lineCount, 0);
			let fullRange = activeEditor.document.validateRange(invalidRange);
			activeEditor.edit(edit => edit.replace(fullRange, newText));
		}

	}));


	context.subscriptions.push(vscode.commands.registerCommand('samwise.clearData', () => {
		samwiseVM.clearData();
	}));

	context.subscriptions.push(vscode.commands.registerCommand('samwise.clearHistory', () => {
		playerView.onClearHistory();
	}));


	context.subscriptions.push(vscode.commands.registerCommand('samwise.exportCSV', async () => {
		const activeEditor = vscode.window.activeTextEditor;
		if (activeEditor) {
			const file = activeEditor.document.fileName;
			let res: any = processResult(activeEditor, activeEditor.document.uri, file, samwiseVM.parseText(file, activeEditor.document.getText()));

			if (res.errors) {
				vscode.window.showInformationMessage('Cannot start dialogue. Fix errors before trying again.');
				return;
			}

			let exportedCSV = samwiseVM.exportCSV(activeEditor.document.getText());

			const fileInfos = await vscode.window.showSaveDialog({
				title: "Export CSV",
				saveLabel: "Export",
				filters: {
					'output': ['csv'],
				}
			});

			if (fileInfos !== undefined) {
				await vscode.workspace.fs.writeFile(fileInfos, new TextEncoder().encode(exportedCSV));
			}
		}

	}));

	context.subscriptions.push(vscode.commands.registerCommand('samwise.addDialogue', addDialogue));
	context.subscriptions.push(vscode.commands.registerCommand('samwise.addSpeech', addSpeech));
	context.subscriptions.push(vscode.commands.registerCommand('samwise.addCaption', addCaption));
	context.subscriptions.push(vscode.commands.registerCommand('samwise.addChoice', addChoice));
	context.subscriptions.push(vscode.commands.registerCommand('samwise.addCheck', addCheck));
	context.subscriptions.push(vscode.commands.registerCommand('samwise.addFallback', addFallback));
	context.subscriptions.push(vscode.commands.registerCommand('samwise.addRandom', addRandom));
	context.subscriptions.push(vscode.commands.registerCommand('samwise.addScore', addScore));
	context.subscriptions.push(vscode.commands.registerCommand('samwise.addSequence', addSequenceSelection));
	context.subscriptions.push(vscode.commands.registerCommand('samwise.addLoop', addLoopSelection));
	context.subscriptions.push(vscode.commands.registerCommand('samwise.addPingPong', addPingPong));
	context.subscriptions.push(vscode.commands.registerCommand('samwise.addGoto', addGoto));
	context.subscriptions.push(vscode.commands.registerCommand('samwise.addFork', addFork));
	context.subscriptions.push(vscode.commands.registerCommand('samwise.addCancel', addCancel));
	context.subscriptions.push(vscode.commands.registerCommand('samwise.addJoin', addJoin));
	context.subscriptions.push(vscode.commands.registerCommand('samwise.addAwait', addAwait));
	context.subscriptions.push(vscode.commands.registerCommand('samwise.addCode', addCode));
	context.subscriptions.push(vscode.commands.registerCommand('samwise.addComment', addComment));
	context.subscriptions.push(vscode.commands.registerCommand('samwise.addLabel', addLabel));
	context.subscriptions.push(vscode.commands.registerCommand('samwise.addTags', addTags));
	context.subscriptions.push(vscode.commands.registerCommand('samwise.addWait', addWait));
	context.subscriptions.push(vscode.commands.registerCommand('samwise.addCarriageReturn', addCarriageReturn));
	context.subscriptions.push(vscode.commands.registerCommand('editor.action.blockComment', toggleDisabledLines));

	context.subscriptions.push(vscode.commands.registerCommand('samwise.editData', async (item) => {
		let value = await vscode.window.showInputBox({
			value: item.value, title: "Overwrite value", prompt: "Insert new variable value", validateInput: (input) => {
				if (typeof item.value === 'number') {
					const parsed = parseInt(input);
					return isNaN(parsed) ? { message: "The value must be a number", severity: vscode.InputBoxValidationSeverity.Error } : null;
				}
				else if (typeof item.value === 'boolean') {
					return input === "true" || input === "false" ? null : { message: "The value must be boolean", severity: vscode.InputBoxValidationSeverity.Error };
				}
				else // string
				{
					// Constant Symbol regex
					const regexp = /^[A-Z](?:[A-Z]|[0-9]|\.|_)*$/;

					return !regexp.test(input) ? { message: "The value must be a valid Symbol", severity: vscode.InputBoxValidationSeverity.Error } : null;
				}
			},

		});

		if (value) {
			const parsedValue = parseValue(value);

			if (item.isLocal) {
				const contextId = item.contextId;

				if (typeof parsedValue === "number") {
					samwiseVM.setLocalIntData(contextId, item.name, parsedValue);
				}
				else if (typeof parsedValue === "boolean") {
					samwiseVM.setLocalBoolData(contextId, item.name, parsedValue);
				}
				else // string
				{
					samwiseVM.setLocalSymbolData(contextId, item.name, parsedValue);
				}
			}
			else {
				if (typeof parsedValue === "number") {
					samwiseVM.setIntData(item.fullContextName, item.name, parsedValue);
				}
				else if (typeof parsedValue === "boolean") {
					samwiseVM.setBoolData(item.fullContextName, item.name, parsedValue);
				}
				else {
					samwiseVM.setSymbolData(item.fullContextName, item.name, parsedValue);
				}
			}
		}
	}));

	context.subscriptions.push(vscode.commands.registerCommand('samwise.hideMetaAll', () => {
		decoration.setHideMetaState(HideMetaState.hideAll);
		vscode.commands.executeCommand('setContext', 'samwise.hideMetaState', 'hideAll');
	}));

	context.subscriptions.push(vscode.commands.registerCommand('samwise.hideMetaIDs', () => {
		decoration.setHideMetaState(HideMetaState.hideId);
		vscode.commands.executeCommand('setContext', 'samwise.hideMetaState', 'hideId');
	}));

	context.subscriptions.push(vscode.commands.registerCommand('samwise.showMeta', () => {
		decoration.setHideMetaState(HideMetaState.show);
		vscode.commands.executeCommand('setContext', 'samwise.hideMetaState', 'show');
	}));

	context.subscriptions.push(vscode.commands.registerCommand('samwise.toggleData', async (item) => {
		if (item.isLocal) {
			const contextId = item.contextId;
			const value = samwiseVM.getLocalBoolData(contextId, item.name);
			samwiseVM.setLocalBoolData(contextId, item.name, !value);
		}
		else {
			const value = samwiseVM.getBoolData(item.fullContextName, item.name);
			samwiseVM.setBoolData(item.fullContextName, item.name, !value);
		}
	}));

	context.subscriptions.push(vscode.commands.registerCommand('samwise.incrementData', async (item) => {
		if (item.isLocal) {
			const value = parseValue(samwiseVM.getLocalIntData(item.contextId, item.name));

			if (typeof value === 'number') {
				samwiseVM.setLocalIntData(item.contextId, item.name, value + 1);
			}
		}
		else {
			let value = parseValue(samwiseVM.getIntData(item.fullContextName, item.name));
			if (typeof value === 'number') {
				samwiseVM.setIntData(item.fullContextName, item.name, value + 1);
			}
		}
	}));

	context.subscriptions.push(vscode.commands.registerCommand('samwise.decrementData', async (item) => {
		if (item.isLocal) {
			const contextId = item.contextId;
			const value = parseValue(samwiseVM.getLocalIntData(contextId, item.name));

			if (typeof value === 'number') {
				samwiseVM.setLocalIntData(contextId, item.name, value - 1);
			}
		}
		else {
			const value = parseValue(samwiseVM.getIntData(item.fullContextName, item.name));

			if (typeof value === 'number') {
				samwiseVM.setIntData(item.fullContextName, item.name, value - 1);
			}
		}
	}));

	context.subscriptions.push(vscode.commands.registerCommand('samwise.clearSubData', async (item) => {
		if (item.isVariable) {
			if (item.isLocal) {
				samwiseVM.clearLocalVariable(item.contextId, item.name);
			}
			else {
				samwiseVM.clearVariable(item.fullContextName, item.name);
			}

			return;
		}
		else if (item.isLocal) {
			samwiseVM.clearLocalData(item.contextId);
		}
		else {
			samwiseVM.clearSubData(item.fullContextName + item.name + ".");
		}
	}));

	context.subscriptions.push(vscode.commands.registerCommand('samwise.saveData', async (item) => {
		const activeEditor = vscode.window.activeTextEditor;
		if (activeEditor) {
			const name = item.fullContextName + item.name + ".";

			// save all variables
			let exportData: string = samwiseVM.exportVariables(name);

			const fileInfos = await vscode.window.showSaveDialog({
				title: "Export variables",
				saveLabel: "Export",
				filters: {
					'samwise': ['sam'],
				}
			});

			if (fileInfos !== undefined) {
				await vscode.workspace.fs.writeFile(fileInfos, new TextEncoder().encode(exportData));
			}
		}
	}));

	context.subscriptions.push(vscode.commands.registerCommand('samwise.loadData', async (item) => loadData(item, true)));
	context.subscriptions.push(vscode.commands.registerCommand('samwise.loadDataAdditive', async (item) => loadData(item, false)));

	async function loadData(item: any, mustClear: boolean) {
		const fileInfos = await vscode.window.showOpenDialog({
			title: "Import variables",
			canSelectFiles: true,
			canSelectFolders: false,
			canSelectMany: true,
			filters: {
				'samwise': ['sam'],
			}
		});

		if (fileInfos !== undefined) {
			if (mustClear) {
				// clear first
				samwiseVM.clearSubData(".");
			}

			const decoder = new TextDecoder();
			fileInfos.forEach(async function (i) {
				const data = await vscode.workspace.fs.readFile(i);
				const dataText = decoder.decode(data);

				let output = samwiseVM.importVariables(dataText);

				if (output !== "") {
					console.log("Error importing variables.");
				}

			});
		}
	}

	context.subscriptions.push(vscode.commands.registerCommand('samwise.addVariable', async (item) => {
		let value = await vscode.window.showInputBox({
			value: item.isLocal ? "bVariable" : item.fullContextName + item.name + ".bVariable", title: "Create variable", prompt: "Insert new variable name", validateInput: (input) => {
				if (item.isLocal) {
					const regexp = /^[bis](?:[A-Z]|[a-z]|[0-9]|_)+$/;

					return !regexp.test(input) ? { message: "Enter a valid local variable name", severity: vscode.InputBoxValidationSeverity.Error } : null;
				}
				else {
					const regexp = /^[\.]?((?:[A-Z]|[a-z]|[0-9]|_)+\.)*[bis](?:[A-Z]|[a-z]|[0-9]|_)+$/;

					return !regexp.test(input) ? { message: "Enter a valid variable name", severity: vscode.InputBoxValidationSeverity.Error } : null;
				}
			},

		});

		if (value) {
			if (item.isLocal) {
				const contextId = item.contextId;

				if (value[0] === "i") {
					samwiseVM.setLocalIntData(contextId, value, 0);
				}
				else if (value[0] === "b") {
					samwiseVM.setLocalBoolData(contextId, value, false);
				}
				else // string
				{
					samwiseVM.setLocalSymbolData(contextId, value, "");
				}
			}
			else {
				const lastDot = value.lastIndexOf('.');

				const firstNameChar = lastDot + 1;

				if (value[firstNameChar] === "i") {
					samwiseVM.setIntData(lastDot < 0 ? "." : value.substring(0, lastDot), value.substring(firstNameChar), 0);
				}
				else if (value[firstNameChar] === "b") {
					samwiseVM.setBoolData(lastDot < 0 ? "." : value.substring(0, lastDot), value.substring(firstNameChar), false);
				}
				else {
					samwiseVM.setSymbolData(lastDot < 0 ? "." : value.substring(0, lastDot), value.substring(firstNameChar), "");
				}
			}
		}
	}));

	context.subscriptions.push(vscode.commands.registerCommand('samwise.goToLabel', async () => {
		const activeEditor = vscode.window.activeTextEditor;
		if (activeEditor) {
			const filename = activeEditor.document.fileName;
			const line = activeEditor.selection.active.line;

			let symbol = samwiseVM.getDestinationSymbol(filename, line + 1);

			let provider = new SamwiseDefinitionProvider();
			var location = provider.findDefinition(activeEditor.document, activeEditor.selection.active);

			if (location !== undefined) {
				const document = await vscode.workspace.openTextDocument(location.uri);
				const editor = await vscode.window.showTextDocument(document);

				const character = 0;
				const position = new vscode.Position(location.range.start.line, character);
				editor.selection = new vscode.Selection(position, position);
				editor.revealRange(new vscode.Range(position, position));
			}
		}
	}));

	// register Tool View
	context.subscriptions.push(vscode.window.registerWebviewViewProvider(
		SamwiseToolViewProvider.viewType,
		toolView = new SamwiseToolViewProvider(context.extensionUri)
	));

	// register Player View
	context.subscriptions.push(vscode.window.registerWebviewViewProvider(
		SamwisePlayerViewProvider.viewType,
		playerView = new SamwisePlayerViewProvider(context.extensionUri, context, contexesPosition),
		{
			webviewOptions: {
				retainContextWhenHidden: false
			}
		}
	));

	context.subscriptions.push(playerView);

	// register Data View
	context.subscriptions.push(vscode.window.registerTreeDataProvider(
		SamwiseDataViewProvider.viewType,
		dataView = new SamwiseDataViewProvider()
	));

	context.subscriptions.push(dataView);

	// register outline provider
	context.subscriptions.push(vscode.languages.registerDocumentSymbolProvider(
		{ language: "samwise" }, new SamwiseDocumentSymbolProvider()));

	context.subscriptions.push(vscode.languages.registerCompletionItemProvider(
		{ language: "samwise" }, new SamwiseCompletionItemProvider(), '>', ' '));

	context.subscriptions.push(vscode.languages.registerCodeLensProvider(
		{ language: "samwise" }, new SamwiseCodelensProvider()));
		
	context.subscriptions.push(vscode.languages.registerFoldingRangeProvider(
		{ language: "samwise" }, new SamwiseFoldingRangeProvider()));
	context.subscriptions.push(vscode.languages.registerFoldingRangeProvider(
		{ language: "samwise" }, new IndentFoldingRangeProvider()));

	context.subscriptions.push(vscode.languages.registerHoverProvider(
		{ language: "samwise" }, new SamwiseHoverProvider()));
	

	// Managed manually
	//context.subscriptions.push(vscode.languages.registerDefinitionProvider(
	//	{language: "samwise"}, new SamwiseDefinitionProvider()));

	vscode.workspace.onDidChangeConfiguration(e => {
		if (e.affectsConfiguration('samwise.clearDialoguesOnPlay') || e.affectsConfiguration('samwise.clearDataOnPlay')
			|| e.affectsConfiguration('samwise.autoAdvance')) {
			updateConfigurationChecks();
		}
	});

	context.subscriptions.push(diagnosticCollection = vscode.languages.createDiagnosticCollection('samwise'));

	refreshEditingStatus();
}

export function deactivate() {
	console.log("Dectivating Samwise extension");
	clearInterval(updateInterval);

	unregisterEvent(samwiseVM.onDialogueContextEndEvent, onDialogueContextEnd);

	unregisterEvent(samwiseVM.onCaptionStartEvent, onCaptionStart);
	unregisterEvent(samwiseVM.onSpeechStartEvent, onSpeechStart);
	unregisterEvent(samwiseVM.onChoiceStartEvent, onChoiceStart);
	unregisterEvent(samwiseVM.onWaitTimeStartEvent, onWaitTimeStart);
	unregisterEvent(samwiseVM.onSpeechOptionStartEvent, onSpeechOptionStart);
	unregisterEvent(samwiseVM.onChallengeStartEvent, onChallengeStart);
	unregisterEvent(samwiseVM.onWaitForMissingDialogueStartEvent, onWaitForMissingDialogueStart);

	decoration.deactivate();
}

function updateConfigurationChecks() {
	const config = vscode.workspace.getConfiguration();
	const clearDialoguesOnPlay = config.get<boolean>('samwise.clearDialoguesOnPlay') ?? true;
	const clearDataOnPlay = config.get<boolean>('samwise.clearDataOnPlay') ?? true;
	const autoAdvance = config.get<boolean>('samwise.autoAdvance') ?? false;

	playerView.setConfiguration(clearDialoguesOnPlay, clearDataOnPlay, autoAdvance);
}

function refreshEditingStatus() {
	if (vscode.window.activeTextEditor) {
		if (vscode.window.activeTextEditor.document.uri.scheme === 'file') {
			const enabled = vscode.window.activeTextEditor.document.languageId === 'samwise';
			vscode.commands.executeCommand('setContext', 'samwise.isEditingFile', enabled);
		}
	} else {
		vscode.commands.executeCommand('setContext', 'samwise.isEditingFile', false);
	}
}

function processResult(editor: vscode.TextEditor, uri: vscode.Uri, filename: string, result: SymbolInfo[] | ErrorsInfo): (SymbolInfo[] | ErrorsInfo) {
	if (diagnosticCollection) {
		diagnosticCollection.clear();

		let diagnostics: vscode.Diagnostic[] = [];

		const res: any = result;
		if (res.errors) {
			(<ErrorInfo[]>res.errors).forEach(error => {
				let range = editor.document.lineAt(error.line - 1).range;
				diagnostics.push(new vscode.Diagnostic(range, error.message));
			});
		}

		if (res.warnings) {
			(<ErrorInfo[]>res.warnings).forEach(warning => {
				let range = editor.document.lineAt(warning.line - 1).range;
				diagnostics.push(new vscode.Diagnostic(range, warning.message, vscode.DiagnosticSeverity.Warning));
			});
		}

		diagnosticCollection.set(uri, diagnostics);
	}

	return result;
}

async function parseWholeWorkspaceFolder(context: vscode.ExtensionContext) {
	const files = await searchSamFilesInWorkspace();

	for (let file of files) {
		const fileData = await vscode.workspace.fs.readFile(file);
		const text = new TextDecoder("utf-8").decode(fileData);
		samwiseVM.parseText(file.fsPath, text);
		await sleep(1);
	}
}

async function searchSamFilesInWorkspace() {
	return await vscode.workspace.findFiles('**/*.sam', '**/node_modules/**');
}

function sleep(milliseconds: number) {
	return new Promise(resolve => setTimeout(resolve, milliseconds));
}

function onDialogueContextEnd(id: number) {
	contexesPosition.delete(id);
	decoration.refreshRunningDialoguesDecoration(contexesPosition);
}

function onCaptionStart(id: number, text: string, location: LocationInfo) {
	contexesPosition.set(id, location);
	decoration.refreshRunningDialoguesDecoration(contexesPosition);
}

function onSpeechStart(id: number, character: string, text: string, location: LocationInfo) {
	contexesPosition.set(id, location);
	decoration.refreshRunningDialoguesDecoration(contexesPosition);
}

function onChoiceStart(id: number, character: string, choices: Choice[], location: LocationInfo) {
	contexesPosition.set(id, location);
	decoration.refreshRunningDialoguesDecoration(contexesPosition);
}

function onWaitTimeStart(id: number, time: number, location: LocationInfo) {
	contexesPosition.set(id, location);
	decoration.refreshRunningDialoguesDecoration(contexesPosition);
}

function onSpeechOptionStart(id: number, character: string, text: string, location: LocationInfo) {
	contexesPosition.set(id, location);
	decoration.refreshRunningDialoguesDecoration(contexesPosition);
}

function onChallengeStart(id: number, name: string, location: LocationInfo) {
	contexesPosition.set(id, location);
	decoration.refreshRunningDialoguesDecoration(contexesPosition);
}

function onWaitForMissingDialogueStart(id: number, name: string, location: LocationInfo) {
	contexesPosition.set(id, location);
	decoration.refreshRunningDialoguesDecoration(contexesPosition);
}