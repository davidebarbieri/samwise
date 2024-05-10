// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

const fs = require('fs');
const dotnet = require("./app/dotnet");
(<any>global).Module = dotnet;

import type { ErrorsInfo, ReferenceInfo, SymbolInfo, LocationInfo, Choice } from "./common";
import { raiseEvent } from "./common";

// Bindings

export let parsedFiles = new Set<string>();
export let parseDialogue: any;
export let startDialogue: any;
export let getDialogueSymbolFromLine: any;
export let getDialogueTitleFromLine: any;
export let isDialogueNodeFromLine: any;
export let getDestinationSymbol: any;
export let isExitingNode: any;
export let startDialogueFromSrcPosition: any;
export let getRunningDialogues: any;
export let stopAll: any;

export let requestChoose: any;
export let requestAdvance: any;
export let completeSpeechOption: any;
export let requestCompleteChallenge: any;
export let requestStop: any;
export let requestResolve: any;

export let clearData: any;
export let clearLocalData: any;
export let clearSubData: any;
export let clearVariable: any;
export let clearLocalVariable: any;

export let setLocalIntData: any;
export let setLocalBoolData: any;
export let setLocalSymbolData: any;
export let setIntData: any;
export let setBoolData: any;
export let setSymbolData: any;
export let getLocalIntData: any;
export let getLocalBoolData: any;
export let getLocalSymbolData: any;
export let getIntData: any;
export let getBoolData: any;

export let assignIds: any;
export let replaceAnonElements: any;
export let exportCSV: any;
export let exportVariables: any;
export let importVariables: any;

export let update: any;

export let getReferences: any;
export let deleteReferences: any;

export let onDialogueContextStartEvent: any = [];
export let onDialogueContextEndEvent: any = [];
export let onCaptionStartEvent: any = [];
export let onCaptionEndEvent: any = [];
export let onSpeechStartEvent: any = [];
export let onSpeechEndEvent: any = [];
export let onChoiceStartEvent: any = [];
export let onChoiceEndEvent: any = [];
export let onWaitTimeStartEvent: any = [];
export let onWaitTimeEndEvent: any = [];
export let onSpeechOptionStartEvent: any = [];
export let onChallengeStartEvent: any = [];
export let onWaitForMissingDialogueStartEvent: any = [];
export let onWaitForMissingDialogueEndEvent: any = [];
export let onErrorEvent: any = [];
export let onBoolDataChangedEvent: any = [];
export let onIntDataChangedEvent: any = [];
export let onSymbolDataChangedEvent: any = [];
export let onLocalBoolDataChangedEvent: any = [];
export let onLocalIntDataChangedEvent: any = [];
export let onLocalSymbolDataChangedEvent: any = [];
export let onDataClearEvent: any = [];
export let onLocalDataClearEvent: any = [];
export let onContextClearEvent: any = [];
export let onLocalContextClearEvent: any = [];

export let getDialogueStatistics: any = [];

export let cachedSymbols: Map<string, SymbolInfo[]> = new Map<string, SymbolInfo[]>();
export let cachedReferences: Map<string, ReferenceInfo[]> = new Map<string, ReferenceInfo[]>();

export function serializeCachedSymbols(): any {
	return JSON.stringify(Array.from(cachedSymbols.entries()));
}

export function deserializeCachedSymbols(serializedValue: any) {
	const mapArray: [string, any][] = serializedValue ? JSON.parse(serializedValue) : [];

	cachedSymbols = new Map(mapArray);
}

export function isInitialized(): boolean {
	return parseDialogue !== undefined;
}

export async function waitUntilInitialized(): Promise<void> {
	while (!isInitialized()) {
		await new Promise(resolve => {
			setTimeout(resolve, 200);
		});
	}
}

export function onFileDeleted(filename: string) {
	cachedSymbols.delete(filename);
	deleteReferences(filename);
}

export function parseText(filename: string, dialogueText: string): (SymbolInfo[] | ErrorsInfo) {
	parsedFiles.add(filename);

	try {
		let output = parseDialogue(filename, dialogueText);
		let parsedValue: (SymbolInfo[] | ErrorsInfo) = JSON.parse(output);
		let res: any = parsedValue;

		if (!res.errors) {
			// Gather informations
			cachedSymbols.set(filename, parsedValue as SymbolInfo[]);
		}

		return parsedValue;
	}
	catch (err) {
		console.error(err);
		return [];
	}
}

export function parseFile(filename: string): (SymbolInfo[] | ErrorsInfo) {
	let dialogueText: string = fs.readFileSync(filename, 'utf8');
	return parseText(filename, dialogueText);
}

(<any>global).onExtensionStarted = function onExtensionStarted() {
	parseDialogue = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:ParseDialogue");
	startDialogue = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:StartDialogue");
	getDialogueSymbolFromLine = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:GetDialogueSymbolFromLine");
	getDialogueTitleFromLine = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:GetDialogueTitleFromLine");
	isDialogueNodeFromLine = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:IsDialogueNodeFromLine");
	getDestinationSymbol = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:GetDestinationSymbol");
	isExitingNode = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:IsExitingNode");
	startDialogueFromSrcPosition = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:StartDialogueFromSrcPosition");
	getRunningDialogues = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:GetRunningDialogues");

	requestChoose = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:Choose");
	requestAdvance = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:Advance");
	completeSpeechOption = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:CompleteSpeechOption");
	requestCompleteChallenge = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:CompleteChallenge");
	requestStop = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:Stop");
	stopAll = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:StopAll");
	requestResolve = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:TryResolveMissingDialogues");

	clearData = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:ClearData");
	clearLocalData = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:ClearLocalData");
	clearSubData = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:ClearSubcontextData");
	clearVariable = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:ClearVariable");
	clearLocalVariable = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:ClearLocalVariable");

	setBoolData = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:SetBoolData");
	setIntData = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:SetIntData");
	setSymbolData = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:SetSymbolData");
	setLocalBoolData = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:SetLocalBoolData");
	setLocalIntData = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:SetLocalIntData");
	setLocalSymbolData = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:SetLocalSymbolData");

	getBoolData = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:GetBoolData");
	getIntData = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:GetIntData");
	getLocalBoolData = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:GetLocalBoolData");
	getLocalIntData = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:GetLocalIntData");

	assignIds = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.Refactoring:AddUniqueIds");
	replaceAnonElements = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.Refactoring:ReplaceAnonymousVariables");
	exportCSV = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.Utilities:ExportToCSV");
	exportVariables = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:ExportVariables");
	importVariables = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:ImportVariables");

	update = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:Update");

	getReferences = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:GetReferences");
	deleteReferences = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:DeleteCodeData");

	getDialogueStatistics = dotnet.mono_bind_static_method("[SamwiseWasm] Peevo.Samwise.Wasm.EntryPoint:GetDialogueStats");

};

(<any>global).onDialogueContextStart = (id: number, title: string) => raiseEvent(onDialogueContextStartEvent, id, title);
(<any>global).onDialogueContextEnd = (id: number) => raiseEvent(onDialogueContextEndEvent, id);

(<any>global).onCaptionStart = (id: number, text: string, location: LocationInfo) => raiseEvent(onCaptionStartEvent, id, text, location);
(<any>global).onCaptionEnd = (id: number) => raiseEvent(onCaptionEndEvent, id);
(<any>global).onSpeechStart = (id: number, character: string, text: string, location: LocationInfo) => raiseEvent(onSpeechStartEvent, id, character, text, location);
(<any>global).onSpeechEnd = (id: number) => raiseEvent(onSpeechEndEvent, id);
(<any>global).onChoiceStart = (id: number, character: string, choices: Choice[], location: LocationInfo) => raiseEvent(onChoiceStartEvent, id, character, choices, location);
(<any>global).onChoiceEnd = (id: number) => raiseEvent(onChoiceEndEvent, id);
(<any>global).onWaitTimeStart = (id: number, time: number, location: LocationInfo) => raiseEvent(onWaitTimeStartEvent, id, time, location);
(<any>global).onWaitTimeEnd = (id: number) => raiseEvent(onWaitTimeEndEvent, id);
(<any>global).onSpeechOptionStart = (id: number, character: string, text: string, location: LocationInfo) => raiseEvent(onSpeechOptionStartEvent, id, character, text, location);
(<any>global).onChallengeStart = (id: number, name: string, location: LocationInfo) => raiseEvent(onChallengeStartEvent, id, name, location);

(<any>global).onWaitForMissingDialogueStart = (id: number, name: string, location: LocationInfo) => raiseEvent(onWaitForMissingDialogueStartEvent, id, name, location);
(<any>global).onWaitForMissingDialogueEnd = (id: number, name: string) => raiseEvent(onWaitForMissingDialogueEndEvent, id, name);
(<any>global).onError = (id: number, text: string) => raiseEvent(onErrorEvent, id);

(<any>global).onBoolDataChanged = (contextName: string, name: string, value: boolean) => raiseEvent(onBoolDataChangedEvent, contextName, name, value);
(<any>global).onIntDataChanged = (contextName: string, name: string, value: number) => raiseEvent(onIntDataChangedEvent, contextName, name, value);
(<any>global).onSymbolDataChanged = (contextName: string, name: string, value: string) => raiseEvent(onSymbolDataChangedEvent, contextName, name, value);
(<any>global).onLocalBoolDataChanged = (contextId: number, name: string, value: boolean) => raiseEvent(onLocalBoolDataChangedEvent, contextId, name, value);
(<any>global).onLocalIntDataChanged = (contextId: number, name: string, value: number) => raiseEvent(onLocalIntDataChangedEvent, contextId, name, value);
(<any>global).onLocalSymbolDataChanged = (contextId: number, name: string, value: string) => raiseEvent(onLocalSymbolDataChangedEvent, contextId, name, value);
(<any>global).onDataClear = (contextName: string, name: string) => raiseEvent(onDataClearEvent, contextName, name);
(<any>global).onLocalDataClear = (contextId: number, name: string) => raiseEvent(onLocalDataClearEvent, contextId, name);
(<any>global).onContextClear = (contextName: string) => raiseEvent(onContextClearEvent, contextName);
(<any>global).onLocalContextClear = (contextId: number) => raiseEvent(onLocalContextClearEvent, contextId);