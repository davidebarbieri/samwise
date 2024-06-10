// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

type Choice =
	{
		text: string;
		mute: boolean;
		time: number;
	};

type NodeDebugInfo =
	{
		dialogueName: string;
		lines: number[];
	};

type SymbolInfo =
	{
		symbol: string;
		name: string;
		type: string;
		startLine: number;
		endLine: number;

		children: SymbolInfo[];
	};

type ReferenceInfo =
	{
		file: string;
		line: number;
	};

type LocationInfo =
	{
		file: string;
		lineStart: number;
		lineEnd: number;
	};

type ErrorsInfo =
	{
		errors: ErrorInfo[];
	};

type ErrorInfo =
	{
		message: string;
		line: number;
	};

type StatsInfo =
	{
		title: string;
		nodes: number;
		words: number;
	};

	
export let titleRegex    = /[\t ]*([¯«»─┬┼│┤├┴┐┘┌└═╦╬║╣╠╩╗╝╔╚░▒▓█▀■▄±‗¶§]+[^\n\r]*)/;
export let contentRegex  = /([\t ]*)(([\t ]*[^\n\r\t ]+)*)([\t ]*)/;

export let wholeTagRegex = /((?:[^#]|(?:##))*)(#[^\n\r]+)/;
export let idTagRegex    = /((?:[^#]|(?:##))*)(#[ \t]*([^\n\r]*))\b(id=[^,\n\r]+)[ \t]*([^\n\r]*)/;

export type { Choice };
export type { NodeDebugInfo };
export type { SymbolInfo };
export type { ReferenceInfo };
export type { ErrorsInfo };
export type { ErrorInfo };
export type { LocationInfo };
export type { StatsInfo };

export function parseValue(text: string): boolean | number | string {
	if (text === "true") { return true; }

	if (text === "false") { return false; }

	const intValue = parseInt(text);

	if (!isNaN(intValue)) {
		return intValue;
	}

	return text;
}

export function getNonce() {
	let text = '';
	const possible = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
	for (let i = 0; i < 32; i++) {
		text += possible.charAt(Math.floor(Math.random() * possible.length));
	}
	return text;
}

export function registerEvent(event: any[], handler: any, register: any = undefined) {
	event.push([register, handler]);
}

export function unregisterEvent(event: any[], handler: any) {
	const index = event.findIndex((value) => value[1] === handler);
	if (index !== -1) {
		event.splice(index, 1);
	}
}

export function unregisterEvents(event: any[], register: any) {
	let i = 0;
	while (i < event.length) {
		if (event[i][0] === register) {
			event.splice(i, 1);
		} else {
			i++;
		}
	}
}

export function raiseEvent(event: any[], ...args: any) {
	for (let e of event) {
		e[1](...args);
	}
}