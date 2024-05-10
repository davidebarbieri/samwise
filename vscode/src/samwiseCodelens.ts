// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

import * as vscode from 'vscode';

import * as samwiseVM from './samwiseVM';
import { ReferenceInfo, SymbolInfo } from './common';

var path = require('path');

class ReferenceCodeLens extends vscode.CodeLens {
	public name: string = "";
	public document: vscode.TextDocument;

	constructor(document: vscode.TextDocument, name: string, range: vscode.Range) {
		super(range);
		this.name = name;
		this.document = document;
	}
}

export class SamwiseCodelensProvider implements vscode.CodeLensProvider {
	private _onDidChangeCodeLenses: vscode.EventEmitter<void> = new vscode.EventEmitter<void>();
	private codeLenses: vscode.CodeLens[] = [];

	public readonly onDidChangeCodeLenses: vscode.Event<void> = this._onDidChangeCodeLenses.event;

	constructor() {

		vscode.workspace.onDidChangeConfiguration((_) => {
			this._onDidChangeCodeLenses.fire();
		});
	}

	provideCodeLenses(document: vscode.TextDocument, token: vscode.CancellationToken): vscode.ProviderResult<vscode.CodeLens[]> {
		if (vscode.workspace.getConfiguration().get("samwise.enableCodeLens", true)) {
			this.codeLenses = [];

			// Add references code lens to Dialogue Headers and labelled nodes
			const value = samwiseVM.cachedSymbols.get(document.fileName);

			if (value) {
				value.forEach((dialogueInfo: SymbolInfo) => {
					this.codeLenses.push(new ReferenceCodeLens(document, dialogueInfo.symbol, new vscode.Range(new vscode.Position(dialogueInfo.startLine - 1, 0), new vscode.Position(dialogueInfo.endLine, 0))));

					dialogueInfo.children.forEach((labelInfo: SymbolInfo) => {
						// dialogueInfo.symbol + "." + labelInfo.symbol;
						this.codeLenses.push(new ReferenceCodeLens(document, dialogueInfo.symbol + "." + labelInfo.symbol, new vscode.Range(new vscode.Position(labelInfo.startLine - 1, 0), new vscode.Position(labelInfo.endLine, 0))));

					});
				});
			}

			return this.codeLenses;
		}
		return [];
	}
	resolveCodeLens?(codeLens: ReferenceCodeLens, token: vscode.CancellationToken): vscode.ProviderResult<vscode.CodeLens> {
		if (vscode.workspace.getConfiguration().get("samwise.enableCodeLens", true)) {
			const refs: (ReferenceInfo[]) = JSON.parse(samwiseVM.getReferences(codeLens.name));

			let locations: vscode.Location[] = [];

			refs.forEach((info: ReferenceInfo) => locations.push(new vscode.Location(vscode.Uri.file(info.file), new vscode.Position(info.line - 1, 0))));

			if (refs.length === 0) {
				codeLens.command = {
					title: "0 references",
					tooltip: "",
					command: "",
					arguments: []
				};
			}
			else {
				codeLens.command = {
					title: refs.length === 1 ? "1 reference" : (refs.length) + " references",
					tooltip: "",
					command: 'editor.action.peekLocations',
					arguments: [
						codeLens.document.uri,
						new vscode.Position(codeLens.range.start.line, codeLens.range.start.character),
						locations,
						'peek'
					]
				};
			}
			return codeLens;
		}
		return null;
	}
}