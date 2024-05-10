// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

import * as vscode from 'vscode';

import * as samwiseVM from './samwiseVM';
import { SymbolInfo } from './common';

var path = require('path');

export class SamwiseCompletionItemProvider implements vscode.CompletionItemProvider {
    public provideCompletionItems(
        document: vscode.TextDocument, position: vscode.Position, token: vscode.CancellationToken):
        Thenable<vscode.CompletionItem[]> {

        let items: vscode.CompletionItem[] = [];

        if (position.character > 1) {
            const gotoRegexp = /^[\t ]*(->)[\t ]*$/;
            const forkRegexp = /^[\t ]*[^\[\(\]\)<]*(=>)[\t ]*$/;
            const awaitRegexp = /^[\t ]*[^\[\(\]\)<]*(<=>)[\t ]*$/;

            const lineText = document.lineAt(position.line);

            const isGoto = gotoRegexp.test(lineText.text);
            const isFork = !isGoto && forkRegexp.test(lineText.text);
            const isAwait = !isGoto && awaitRegexp.test(lineText.text);

            if (isGoto || isFork || isAwait) {
                const isStarting = lineText.text[position.character - 1] === '>';

                if (isFork) {
                    // => supports anonymous jumps, give that as first option
                    const completionItem = new vscode.CompletionItem("");
                    completionItem.insertText = "\n\t";
                    completionItem.label = " ";
                    completionItem.documentation = "Create an anonymous fork";
                    completionItem.detail = "Anonymous fork";

                    items.push(completionItem);
                }
                else if (isGoto) {
                    // Goto supports "end"
                    const completionItem = new vscode.CompletionItem("end");
                    completionItem.insertText = isStarting ? " end\n" : "end";
                    completionItem.documentation = "End dialogue";
                    completionItem.sortText = "__";
                    completionItem.detail = "End";

                    items.push(completionItem);
                }

                // Get current Dialogue
                const dialogueSymbol = samwiseVM.getDialogueSymbolFromLine(document.fileName, position.line);

                // TODO: cache this
                samwiseVM.cachedSymbols.forEach((value: SymbolInfo[], file: string) => {
                    value.forEach((info: SymbolInfo) => {
                        const isLocal = info.symbol === dialogueSymbol;

                        const labelItems = addLabels(info, file, isLocal, isStarting);
                        if (labelItems.length > 0) {
                            items.push(...labelItems);
                        }

                        let symbol: string = info.symbol;
                        const completionItem = new vscode.CompletionItem(symbol);
                        completionItem.documentation = new vscode.MarkdownString(path.parse(file).base + ":" + info.startLine);
                        if (isStarting) {
                            completionItem.insertText = " " + symbol + "\n";
                        }
                        completionItem.kind = vscode.CompletionItemKind.Module;
                        completionItem.detail = "Dialogue";
                        //completionItem.filterText = undefined;
                        //completionItem.sortText = undefined;

                        items.push(completionItem);
                    });
                });
            }
        }

        return Promise.resolve(items);
    }
}

function addLabels(parent: SymbolInfo, file: string, isLocal: boolean, isStarting: boolean): vscode.CompletionItem[] {
    let items: vscode.CompletionItem[] = [];
    //if (info.type === 'Label')

    parent.children.forEach((info: SymbolInfo) => {
        let symbol: string = isLocal ? info.symbol : parent.symbol + "." + info.symbol;

        const completionItem = new vscode.CompletionItem(symbol);

        if (isStarting) {
            completionItem.insertText = " " + symbol + "\n";
        }

        if (isLocal) {
            completionItem.sortText = "_" + symbol;
        }
        completionItem.detail = isLocal ? "Local label" : "Global label";
        completionItem.kind = vscode.CompletionItemKind.Constant;
        completionItem.documentation = new vscode.MarkdownString(path.parse(file).base + ":" + info.startLine);

        items.push(completionItem);
    });

    return items;
}