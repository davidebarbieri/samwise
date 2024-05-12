// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

import * as vscode from 'vscode';

import type { Choice, LocationInfo } from "./common";
import * as utils from "./common";
import { registerEvent, unregisterEvents } from "./common";

import * as samwiseVM from './samwiseVM';
import * as fs from 'fs';

import * as editing from './samwiseEditing';

export class SamwisePlayerViewProvider implements vscode.WebviewViewProvider {
	constructor(private readonly _extensionUri: vscode.Uri, private readonly _context: vscode.ExtensionContext, private readonly _contexesPosition: Map<number, LocationInfo>) {
		registerEvent(samwiseVM.onDialogueContextStartEvent, this.onDialogueContextStart.bind(this), this);
		registerEvent(samwiseVM.onDialogueContextEndEvent, this.onDialogueContextEnd.bind(this), this);
		registerEvent(samwiseVM.onCaptionStartEvent, this.onCaptionStart.bind(this), this);
		registerEvent(samwiseVM.onCaptionEndEvent, this.onCaptionEnd.bind(this), this);
		registerEvent(samwiseVM.onSpeechStartEvent, this.onSpeechStart.bind(this), this);
		registerEvent(samwiseVM.onSpeechEndEvent, this.onSpeechEnd.bind(this), this);
		registerEvent(samwiseVM.onChoiceStartEvent, this.onChoiceStart.bind(this), this);
		registerEvent(samwiseVM.onChoiceEndEvent, this.onChoiceEnd.bind(this), this);
		registerEvent(samwiseVM.onWaitTimeStartEvent, this.onWaitTimeStart.bind(this), this);
		registerEvent(samwiseVM.onWaitTimeEndEvent, this.onWaitTimeEnd.bind(this), this);
		registerEvent(samwiseVM.onSpeechOptionStartEvent, this.onSpeechOptionStart.bind(this), this);
		registerEvent(samwiseVM.onChallengeStartEvent, this.onChallengeStart.bind(this), this);
		registerEvent(samwiseVM.onWaitForMissingDialogueStartEvent, this.onWaitForMissingDialogueStart.bind(this), this);
		registerEvent(samwiseVM.onWaitForMissingDialogueEndEvent, this.onWaitForMissingDialogueEnd.bind(this), this);
		registerEvent(samwiseVM.onErrorEvent, this.onError.bind(this), this);
	}

	dispose(): void {
		unregisterEvents(samwiseVM.onDialogueContextStartEvent, this);
		unregisterEvents(samwiseVM.onDialogueContextEndEvent, this);
		unregisterEvents(samwiseVM.onCaptionStartEvent, this);
		unregisterEvents(samwiseVM.onCaptionEndEvent, this);
		unregisterEvents(samwiseVM.onSpeechStartEvent, this);
		unregisterEvents(samwiseVM.onSpeechEndEvent, this);
		unregisterEvents(samwiseVM.onChoiceStartEvent, this);
		unregisterEvents(samwiseVM.onChoiceEndEvent, this);
		unregisterEvents(samwiseVM.onWaitTimeStartEvent, this);
		unregisterEvents(samwiseVM.onWaitTimeEndEvent, this);
		unregisterEvents(samwiseVM.onSpeechOptionStartEvent, this);
		unregisterEvents(samwiseVM.onChallengeStartEvent, this);
		unregisterEvents(samwiseVM.onWaitForMissingDialogueStartEvent, this);
		unregisterEvents(samwiseVM.onWaitForMissingDialogueEndEvent, this);
		unregisterEvents(samwiseVM.onErrorEvent, this);
	}

	public static readonly viewType = 'samwise.player';

	private _view?: vscode.WebviewView;
	private _running: boolean = false;

	public setConfiguration(clearDialoguesOnPlay: boolean, clearDataOnPlay: boolean) {
		this.postMessage({ command: "setConfiguration", clearDialoguesOnPlay: clearDialoguesOnPlay, clearDataOnPlay: clearDataOnPlay });
	}

	public onElementSelected(symbol: string, isNode: boolean) {
		if (this._view === null || !this._view?.visible) {
			return; // skip if not visible
		}
		this.postMessage({ command: "onElementSelected", symbol: symbol, isNode: isNode });
	}

	public onDialogueContextStart(id: number, title: string) {
		this.postMessage({ command: "onDialogueContextStart", id: id, title: title });
	}

	public onDialogueContextEnd(id: number) {
		this.postMessage({ command: "onDialogueContextEnd", id: id });
	}

	public onCaptionStart(id: number, text: string) {
		this.postMessage({ command: "onCaptionStart", id: id, text: text });
	}

	public onCaptionEnd(id: number) {
		this.postMessage({ command: "onCaptionEnd", id: id });
	}

	public onWaitTimeStart(id: number, time: number) {
		this.postMessage({ command: "onWaitTimeStart", id: id, time: time });
	}

	public onWaitTimeEnd(id: number) {
		this.postMessage({ command: "onWaitTimeEnd", id: id });
	}

	public onSpeechStart(id: number, character: string, text: string) {
		var avatarURL = this._findAvatar(character);

		this.postMessage({ command: "onSpeechStart", id: id, character: character, text: text, avatar: avatarURL });
	}

	public onSpeechOptionStart(id: number, character: string, text: string) {
		var avatarURL = this._findAvatar(character);

		this.postMessage({ command: "onSpeechOptionStart", id: id, character: character, text: text, avatar: avatarURL });
	}

	public onSpeechEnd(id: number) {
		this.postMessage({ command: "onSpeechEnd", id: id });
	}

	public onChoiceStart(id: number, character: string, choices: Choice[]) {
		var avatarURL = this._findAvatar(character);

		this.postMessage({ command: "onChoiceStart", id: id, character: character, choices: choices, avatar: avatarURL });
	}

	public onChoiceEnd(id: number) {
		this.postMessage({ command: "onChoiceEnd", id: id });
	}

	public onChallengeStart(id: number, name: string) {
		this.postMessage({ command: "onChallengeStart", id: id, name: name });
	}

	public onWaitForMissingDialogueStart(id: number, name: string) {
		this.postMessage({ command: "onWaitForMissingDialogueStart", id: id, name: name });
	}

	public onWaitForMissingDialogueEnd(id: number, name: string) {
		this.postMessage({ command: "onWaitForMissingDialogueEnd", id: id, name: name });
	}

	public onError(id: number, text: string) {
		this.postMessage({ command: "onError", id: id, text: text });
	}

	public onClear() {
		this.postMessage({ command: "onClear" });
	}

	public onClearHistory() {
		this.postMessage({ command: "onClearHistory" });
	}

	public postMessage(message: any) {
		if (this._view && this._view.visible) {
			this._view.webview.postMessage(message);
		}
		else {
			this.pendingMessages.push(message);
		}
	}

	public resolveWebviewView(
		webviewView: vscode.WebviewView,
		context: vscode.WebviewViewResolveContext,
		_token: vscode.CancellationToken,
	) {
		this._view = webviewView;

		webviewView.webview.options = {
			// Allow scripts in the webview
			enableScripts: true
			/*
			,

			localResourceRoots: [
				this._extensionUri
			]
			*/
		};

		this.onElementSelected("", false);

		vscode.commands.executeCommand('setContext', 'samwise.isSidepanelOpen', true);

		webviewView.onDidDispose(() => {
			vscode.commands.executeCommand('setContext', 'samwise.isSidepanelOpen', false);
			this._view = undefined;
		});

		webviewView.onDidChangeVisibility(() => {
			vscode.commands.executeCommand('setContext', 'samwise.isSidepanelOpen', webviewView.visible);
		});

		webviewView.webview.onDidReceiveMessage(message => {
			if (message.execute) {
				vscode.commands.executeCommand(message.execute);
			}
			else if (message.updateSetting) {
				const settingKey = message.updateSetting;
				const newValue = message.value;

				vscode.workspace.getConfiguration().update(settingKey, newValue, vscode.ConfigurationTarget.Workspace);
			}
			else {
				switch (message.command) {
					case 'initDone':
						{
							if (!this._running) {
								this._running = true;

								this.onClear();
							}

							this.pendingMessages.forEach((message) => webviewView.webview.postMessage(message));
							this.pendingMessages = [];

							// Update configuration
							const config = vscode.workspace.getConfiguration();
							const clearDialoguesOnPlay = config.get<boolean>('samwise.clearDialoguesOnPlay') ?? true;
							const clearDataOnPlay = config.get<boolean>('samwise.clearDataOnPlay') ?? true;

							this.setConfiguration(clearDialoguesOnPlay, clearDataOnPlay);
						}
					case 'advance':
						{
							samwiseVM.requestAdvance(message.id);
							break;
						}
					case 'stop':
						{
							samwiseVM.requestStop(message.id);
							break;
						}
					case
						'jump':
						{
							const location = this._contexesPosition.get(message.id);
							if (location) {
								editing.showTextLine(location);
							}
							break;
						}
					case 'choose':
						{
							samwiseVM.requestChoose(message.id, message.choice);
							break;
						}
					case 'completeChallenge':
						{
							samwiseVM.requestCompleteChallenge(message.id, message.result);
							break;
						}
					case 'tryResolve':
						{
							const resolved: boolean = samwiseVM.requestResolve(message.id, message.name);

							if (!resolved) {
								vscode.window.showOpenDialog(
									{
										canSelectFiles: true,
										canSelectFolders: false,
										title: "Select Samwise file",
										// eslint-disable-next-line @typescript-eslint/naming-convention
										filters: { 'Samwise': ['sam'] }
									}
								).then((filename) => {
									if (filename && filename.length > 0) {
										samwiseVM.parseFile(filename[0].fsPath);
										samwiseVM.requestResolve(message.id, message.name);
									}
								});
							}

							break;
						}
				}
			}
		});

		webviewView.webview.html = this._getWebviewContent(webviewView.webview);
	}

	private _getWebviewContent(webview: vscode.Webview) {

		// Get the local path to main script run in the webview, then convert it to a uri we can use in the webview.
		const mainUri = webview.asWebviewUri(vscode.Uri.joinPath(this._extensionUri, "webview-ui", "main.js"));
		const cssUri = webview.asWebviewUri(vscode.Uri.joinPath(this._extensionUri, "webview-ui", "theme.css"));
		const toolkitUri = webview.asWebviewUri(vscode.Uri.joinPath(this._extensionUri, 'node_modules', '@vscode', 'webview-ui-toolkit', 'dist', 'toolkit.js'));
		const codiconsUri = webview.asWebviewUri(vscode.Uri.joinPath(this._extensionUri, 'node_modules', '@vscode/codicons', 'dist', 'codicon.css'));

		// Use a nonce to only allow a specific script to be run.
		const nonce = utils.getNonce();

		return `<!DOCTYPE html>
			<html lang="en">
			<head>
				<meta charset="UTF-8">
				<!--
					Use a content security policy to only allow loading images from https or from our extension directory,
					and only allow scripts that have a specific nonce.
				-->
				<!-- <meta http-equiv="Content-Security-Policy" content="default-src 'none'; style-src ${webview.cspSource}; font-src ${webview.cspSource}; script-src 'nonce-${nonce}';"> -->
				<meta name="viewport" content="width=device-width, initial-scale=1.0">
				
				<link rel="stylesheet" type="text/css" href=${cssUri} />
				<link rel="stylesheet" type="text/css" href="${codiconsUri}" />

				<script nonce="${nonce}" src="${mainUri}"></script>
				<script nonce="${nonce}" type="module" src="${toolkitUri}"></script>
			</head>
			<body>
				<div id="selectionroot">
				<div id="selection_options_fold"><vscode-button appearance="icon" id="options_fold" aria-label="Toggle Preview Settings"><div class="icon"><span class="codicon codicon-settings"></span><span class="tooltiptext">Toggle Preview Settings</span></div></div>
				<div id="selection">No dialogues selected.</div>
					<div class="folded" id="selection_options">
						<vscode-checkbox id="selection_options_clear_dialogues">Clear dialogues on start</vscode-checkbox>
						<vscode-checkbox id="selection_options_clear_data">Clear data on start</vscode-checkbox>
						<vscode-checkbox checked id="selection_options_skip">Skip pauses</vscode-checkbox>
					</div>
				</div>
				<div id="root"><vscode-label class="selectionTextDisabled" id="dialogueplaceholder">No dialogue running</vscode-label></div>
				<div id="history"></div>
			</body>
			</html>`;
	}

	private _findAvatar(character: string) {
		// Find Custom Avatar
		var avatarURL = "";
		vscode.workspace.workspaceFolders?.some((folder) => {
			const path = vscode.Uri.joinPath(folder.uri, ".samwise", "avatars", character + ".png");
			if (this._view && fs.existsSync(path.fsPath)) {
				avatarURL = this._view.webview.asWebviewUri(path).toString();
				return true;
			}

			return false;
		});

		if (avatarURL === "" && this._view) {
			// Provide a default Avatar
			var avatarNo = this.tempAvatars.get(character);

			if (avatarNo === undefined) {
				avatarNo = this.tempAvatars.size % 16;
				this.tempAvatars.set(character, avatarNo);
			}

			avatarURL = this._view.webview.asWebviewUri(vscode.Uri.joinPath(this._extensionUri, "webview-ui", "avatars", "char" + (avatarNo + 1) + ".png")).toString();
		}

		return avatarURL;
	}

	private pendingMessages: any[] = [];

	private tempAvatars = new Map<string, number>();
}