// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

import * as vscode from 'vscode';
import * as utils from "./common";

export class SamwiseToolViewProvider implements vscode.WebviewViewProvider {
	constructor(private readonly _extensionUri: vscode.Uri) { }

	public static readonly viewType = 'samwise.tool';

	private _view?: vscode.WebviewView;

	public resolveWebviewView(
		webviewView: vscode.WebviewView,
		context: vscode.WebviewViewResolveContext,
		_token: vscode.CancellationToken,
	) {
		this._view = webviewView;

		webviewView.webview.options = {
			// Allow scripts in the webview
			enableScripts: true,

			localResourceRoots: [
				this._extensionUri
			]
		};

		webviewView.webview.html = this._getWebviewContent(webviewView.webview);

		webviewView.webview.onDidReceiveMessage(message => {
			if (message.character) {
				const editor = vscode.window.activeTextEditor;

				if (editor) {
					const document = editor.document;
					const snippetString = new vscode.SnippetString().appendText(message.character);

					var position = editor.selection.active;

					editor.insertSnippet(snippetString, position);
				}
			}
			else {
				vscode.commands.executeCommand(message.command);
			}
		});
	}


	private _getWebviewContent(webview: vscode.Webview) {
		const toolUri = webview.asWebviewUri(vscode.Uri.joinPath(this._extensionUri, "webview-ui", "tool.js"));
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

				<script nonce="${nonce}" type="module" src="${toolUri}"></script>
				<script nonce="${nonce}" type="module" src="${toolkitUri}"></script>
			</head>
			<body>
				<div id="root"></div>
			</body>
			</html>`;
	}
}