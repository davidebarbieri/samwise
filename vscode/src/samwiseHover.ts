// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

import * as vscode from 'vscode';

import * as samwiseVM from './samwiseVM';
import { StatsInfo } from './common';
import { titleRegex } from './common';

var path = require('path');

export class SamwiseHoverProvider implements vscode.HoverProvider {
    provideHover(document: vscode.TextDocument, position: vscode.Position, token: vscode.CancellationToken): vscode.ProviderResult<vscode.Hover> {

        const value = samwiseVM.cachedSymbols.get(document.fileName);

        if (value) {
            for (let dialogueInfo of value) {
                if (position.line >= dialogueInfo.startLine - 1 && position.line < dialogueInfo.endLine) {

                    // Check title regex
                    if (!titleRegex.test(document.lineAt(position.line).text)) {
                        continue;
                    }
                    
                    const stats: (StatsInfo) = JSON.parse(samwiseVM.getDialogueStatistics(dialogueInfo.symbol));

                    let title = stats.title;
                    if (title.trim().length === 0) {
                        title = "<i>Untitled</i>";
                    }

                    const content = new vscode.MarkdownString(`<p>${title}</p>`);
                    content.appendMarkdown(`Label: <code>${dialogueInfo.symbol}</code><br>`);
                    content.appendMarkdown(`Words: <code>${stats.words}</code><br>`);
                    content.appendMarkdown(`Nodes: <code>${stats.nodes}</code>`);
                    content.supportHtml = true;
                    content.isTrusted = true;

                    return {
                        contents: [content]
                    };
                }
            }
        }

        return null;
    }
}