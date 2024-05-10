// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

import * as vscode from 'vscode';

import type { SymbolInfo } from "./common";

import * as parser from './samwiseVM';

export class SamwiseDocumentSymbolProvider implements vscode.DocumentSymbolProvider {
    async provideDocumentSymbols(document: vscode.TextDocument, token: vscode.CancellationToken): Promise<vscode.DocumentSymbol[]> {
        // wait until initalized
        await parser.waitUntilInitialized();

        // Parse
        let info: any = parser.parseText(document.fileName, document.getText());

        if (info === undefined || info.errors) {
            return this._previousInfo;
        }
        else {
            const newSymbols = createSymbols(info);
            this._previousInfo = newSymbols;
            return newSymbols;
        }
    }

    _previousInfo: vscode.DocumentSymbol[] = [];
}

function createSymbols(elements: SymbolInfo[]): vscode.DocumentSymbol[] {
    let results: vscode.DocumentSymbol[] = [];

    elements.forEach(element => {

        let symbol = createSymbolForElement(element);
        if (element.children) {
            symbol.children = createSymbols(element.children);
        }

        results.push(symbol);
    });

    return results;
}

function createSymbolForElement(info: SymbolInfo): vscode.DocumentSymbol {
    const fullRange = new vscode.Range(info.startLine - 1, 0, info.endLine - 1, 1);
    const nameRange = new vscode.Range(info.startLine - 1, 0, info.startLine - 1, 1);

    return new vscode.DocumentSymbol(info.symbol, info.name, toSymbolKind(info.type), fullRange, nameRange);
}


function toSymbolKind(kind: string): vscode.SymbolKind {
    if (kind === "Label") {
        return vscode.SymbolKind.Constant;
    }
    else {
        return vscode.SymbolKind.Module;
    }
}