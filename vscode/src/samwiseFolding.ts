// (c) Copyright 2025 Davide 'PeevishDave' Barbieri

import * as vscode from 'vscode';

import * as samwiseVM from './samwiseVM';
import { SymbolInfo } from './common';
import { titleRegex } from './common';

var path = require('path');

export class SamwiseFoldingRangeProvider implements vscode.FoldingRangeProvider  {
    provideFoldingRanges(document: vscode.TextDocument, context: vscode.FoldingContext, token: vscode.CancellationToken) {
            const ranges: vscode.FoldingRange[] = [];

            // Provide foldable dialogues
            for (let line = 0; line < document.lineCount; line++) {
                let startPosition = this.searchNextFoldableTitle(line, document);
                if (startPosition === undefined) {
                    break;
                }

                let endPosition = this.searchDialogueEnd(startPosition + 1, document);

                ranges.push(new vscode.FoldingRange(startPosition, endPosition, vscode.FoldingRangeKind.Region));

                line = endPosition;
            }

            return ranges;
        }

        searchNextFoldableTitle(currentLine: number, document: vscode.TextDocument): number | undefined {
            
            let prevWasTitle = false;
                  
            for (let line = currentLine; line < document.lineCount; line++) {
                const lineString = document.lineAt(line).text;

                let titleMatch = lineString.match(titleRegex);
                
                if (titleMatch !== null && titleMatch.index !== undefined) {
                    prevWasTitle = true;
                }
                else {
                    if (prevWasTitle) {
                        return line - 1;
                    }
                }
            }
            
            return undefined;
        }

        searchDialogueEnd(currentLine: number, document: vscode.TextDocument): number {
            
            for (let line = currentLine; line < document.lineCount; line++) {
                const lineString = document.lineAt(line).text;

                let titleMatch = lineString.match(titleRegex);
                
                if (titleMatch !== null && titleMatch.index !== undefined) {
                    return line - 1;
                }
            }
            
            return document.lineCount - 1;
        }
}

export class IndentFoldingRangeProvider implements vscode.FoldingRangeProvider  {
    provideFoldingRanges(document: vscode.TextDocument, context: vscode.FoldingContext, token: vscode.CancellationToken) {
        const foldingRanges: vscode.FoldingRange[] = [];
        const stack: { indent: number, startLine: number }[] = [];
        const tabSize = vscode.workspace.getConfiguration('editor', document.uri).get<number>('tabSize') || 4;

        let lastNonEmptyLine = 0;
        for (let i = 0; i < document.lineCount; i++) {
            const line = document.lineAt(i);
            if (line.isEmptyOrWhitespace) 
            {
                continue;
            }

            const indent = getIndentLevel(line.text, tabSize);

            if (indent < 0)
            {
                continue;
            }

            while (stack.length && indent <= stack[stack.length - 1].indent) {
                const block = stack.pop()!;
                if (lastNonEmptyLine > block.startLine) {
                    foldingRanges.push(new vscode.FoldingRange(block.startLine, lastNonEmptyLine));
                }
            }

            stack.push({ indent, startLine: i });

            lastNonEmptyLine = i;
        }

        while (stack.length > 0) {
            const block = stack.pop()!;
            if (lastNonEmptyLine > block.startLine) {
                foldingRanges.push(new vscode.FoldingRange(block.startLine, lastNonEmptyLine));
            }
        }

        return foldingRanges;
    }

}

function getIndentLevel(line: string, tabSize: number): number {
	const len = line.length;

    let indent = 0;
	for (let i = 0; i < len; ++i) {
		const c = line.charAt(i);
		if (c === ' ') {
			indent++;
		} else if (c === '\t') {
			indent = indent + tabSize - (indent % tabSize);
		} else {
	        return indent;
		}
	}

	return -1; // empty line
}