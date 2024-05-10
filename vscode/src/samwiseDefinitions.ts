// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

import * as vscode from 'vscode';

import * as samwiseVM from './samwiseVM';
import { SymbolInfo } from './common';

var path = require('path');

export class SamwiseDefinitionProvider implements vscode.DefinitionProvider {
    provideDefinition(
        document: vscode.TextDocument, position: vscode.Position, token: vscode.CancellationToken): vscode.Location | undefined {
        return this.findDefinition(document, position);
    }

    public findDefinition(
        document: vscode.TextDocument, position: vscode.Position): vscode.Location | undefined {
        const line = position.line + 1;
        const symbol: string = samwiseVM.getDestinationSymbol(document.fileName, line);

        let location: vscode.Location | undefined = undefined;

        if (symbol === undefined || symbol === "") {
            return;
        }

        const dialogueSymbol = samwiseVM.getDialogueSymbolFromLine(document.fileName, line);

        // search symbol
        samwiseVM.cachedSymbols.forEach((value: SymbolInfo[], file: string) => {
            value.forEach((dialogueInfo: SymbolInfo) => {
                if (dialogueInfo.symbol === symbol) {
                    const uri = vscode.Uri.file(file);
                    location = new vscode.Location(uri, new vscode.Position(dialogueInfo.startLine - 1, 0));
                    return;
                }

                const isLocal = dialogueInfo.symbol === dialogueSymbol;

                dialogueInfo.children.forEach((labelInfo: SymbolInfo) => {
                    let targetSymbol: string = isLocal ? labelInfo.symbol : dialogueInfo.symbol + "." + labelInfo.symbol;

                    if (targetSymbol === symbol) {
                        location = new vscode.Location(vscode.Uri.file(file), new vscode.Position(labelInfo.startLine - 1, 0));
                        return;
                    }
                });
            });
        });

        return location;
    }
}