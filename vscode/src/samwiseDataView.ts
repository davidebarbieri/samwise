// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

import * as vscode from 'vscode';
import * as samwiseVM from './samwiseVM';
import { registerEvent, unregisterEvents } from "./common";

class DataNode {
    constructor(fullContextName: string, name: string) { this.fullContextName = fullContextName; this.name = name; }

    fullContextName: string = "";
    name: string = "";
    isLocal: boolean = false;
    isVariable: boolean = false;
}

class VariableNode extends DataNode {
    constructor(isLocal: boolean, fullContextName: string, name: string, value: any) { super(fullContextName, name); this.isLocal = isLocal; this.value = value; this.isVariable = true; }

    value: any;
}

class LocalVariableNode extends VariableNode {
    constructor(contextId: number, fullContextName: string, name: string, value: any) { super(true, fullContextName, name, value); this.contextId = contextId; }

    contextId: number;
}

class ContainerNode extends DataNode {
    constructor(isLocal: boolean, fullContextName: string, name: string) { super(fullContextName, name); this.isLocal = isLocal; }

    variables: Map<string, VariableNode> = new Map<string, VariableNode>();

    clear() {
        this.variables.clear();
    }
}

class LocalContainerNode extends ContainerNode {
    constructor(contextId: number, fullContextName: string, name: string) { super(true, fullContextName, name); this.contextId = contextId; }

    contextId: number = -1;
}

class GlobalContainerNode extends ContainerNode {
    constructor(fullContextName: string, name: string) { super(false, fullContextName, name); }

    children: Map<string, GlobalContainerNode> = new Map<string, GlobalContainerNode>();

    clear() {
        super.clear();
        this.children.clear();
    }
}

export class SamwiseDataViewProvider implements vscode.TreeDataProvider<DataNode> {
    public static readonly viewType = 'samwise.data';

    constructor() {
        registerEvent(samwiseVM.onBoolDataChangedEvent, this.onBoolDataChanged.bind(this), this);
        registerEvent(samwiseVM.onIntDataChangedEvent, this.onIntDataChanged.bind(this), this);
        registerEvent(samwiseVM.onSymbolDataChangedEvent, this.onSymbolDataChanged.bind(this), this);
        registerEvent(samwiseVM.onLocalBoolDataChangedEvent, this.onLocalBoolDataChanged.bind(this), this);
        registerEvent(samwiseVM.onLocalIntDataChangedEvent, this.onLocalIntDataChanged.bind(this), this);
        registerEvent(samwiseVM.onLocalSymbolDataChangedEvent, this.onLocalSymbolDataChanged.bind(this), this);
        registerEvent(samwiseVM.onDataClearEvent, this.onDataClear.bind(this), this);
        registerEvent(samwiseVM.onLocalDataClearEvent, this.onLocalDataClear.bind(this), this);
        registerEvent(samwiseVM.onContextClearEvent, this.onContextClear.bind(this), this);
        registerEvent(samwiseVM.onLocalContextClearEvent, this.onLocalContextClear.bind(this), this);
    }

    dispose(): void {
        unregisterEvents(samwiseVM.onBoolDataChangedEvent, this);
        unregisterEvents(samwiseVM.onIntDataChangedEvent, this);
        unregisterEvents(samwiseVM.onSymbolDataChangedEvent, this);
        unregisterEvents(samwiseVM.onLocalBoolDataChangedEvent, this);
        unregisterEvents(samwiseVM.onLocalIntDataChangedEvent, this);
        unregisterEvents(samwiseVM.onLocalSymbolDataChangedEvent, this);
        unregisterEvents(samwiseVM.onDataClearEvent, this);
        unregisterEvents(samwiseVM.onLocalDataClearEvent, this);
        unregisterEvents(samwiseVM.onContextClearEvent, this);
        unregisterEvents(samwiseVM.onLocalContextClearEvent, this);
    }

    refresh(): void {
        this._onDidChangeTreeData.fire(undefined);
    }

    getTreeItem(element: DataNode): vscode.TreeItem {
        const isVariable = element instanceof VariableNode;
        const item = new vscode.TreeItem(element.name, isVariable ? vscode.TreeItemCollapsibleState.None : vscode.TreeItemCollapsibleState.Expanded);

        item.tooltip = `${element.name}`;

        if (isVariable) {
            const variableElement = element as VariableNode;
            item.description = `${variableElement.value}`;
            item.contextValue = element.name.startsWith('b') ? "variable_bool" : element.name.startsWith('i') ? "variable_int" : "variable_symbol";
        }
        else {
            if (element.name === "") {
                item.description = "[Global]";
                item.contextValue = "container_global";
            }
            else {
                if (element instanceof GlobalContainerNode) {
                    item.contextValue = "container";
                }
                else {
                    item.contextValue = "container_context";
                }
            }
        }

        return item;
    }

    getChildren(element?: DataNode): Thenable<DataNode[]> {
        if (element) {
            if (element instanceof VariableNode) {
                return Promise.resolve([]);
            }

            // then, it's a container
            let nodes: DataNode[] = [];

            const container: ContainerNode = element as ContainerNode;

            // first, variables
            for (let key of container.variables.keys()) {
                nodes.push(container.variables.get(key) as VariableNode);
            }

            // then, children
            if (element instanceof GlobalContainerNode) {
                const globalContainer: GlobalContainerNode = element as GlobalContainerNode;

                for (let key of globalContainer.children.keys()) {
                    nodes.push(globalContainer.children.get(key) as GlobalContainerNode);
                }
            }

            return Promise.resolve(nodes);
        }
        else {
            // root
            let nodes: DataNode[] = [];

            // Global root
            nodes.push(this.globalRoot);

            // Add contexes local data
            for (let key of this.localData.keys()) {
                const localNode = this.localData.get(key);

                if (localNode !== undefined) {
                    nodes.push(localNode);
                }
            }

            return Promise.resolve(nodes);
        }
    }

    onBoolDataChanged(contextName: string, name: string, value: boolean) {
        this.writeData(contextName, name, value);
        this.refresh();
    }

    onIntDataChanged(contextName: string, name: string, value: number) {
        this.writeData(contextName, name, value);
        this.refresh();
    }

    onSymbolDataChanged(contextName: string, name: string, value: string) {
        this.writeData(contextName, name, value);
        this.refresh();
    }

    onLocalBoolDataChanged(contextId: number, name: string, value: boolean) {
        this.writeLocalData(contextId, name, value);
        this.refresh();
    }

    onLocalIntDataChanged(contextId: number, name: string, value: number) {
        this.writeLocalData(contextId, name, value);
        this.refresh();
    }

    onLocalSymbolDataChanged(contextId: number, name: string, value: string) {
        this.writeLocalData(contextId, name, value);
        this.refresh();
    }

    onContextClear(contextName: string) {
        this.deleteData(contextName);
        this.refresh();
    }

    onLocalContextClear(contextId: number) {
        if (this.localData.has(contextId)) {
            this.localData.delete(contextId);
        }
        this.refresh();
    }

    onDataClear(contextName: string, name: string) {
        let currData = this.globalRoot;

        if (contextName) {
            const parts = contextName.split(".");

            for (let i = 0; i < parts.length; ++i) {
                const part = parts[i];

                if (!part) {
                    continue;
                }

                let subMap = currData.children.get(part);

                if (subMap === undefined) {
                    return;
                }

                currData = subMap;
            }
        }

        currData.variables.delete(name);
        this.refresh();
    }

    onLocalDataClear(contextId: number, name: string) {
        let subMap = this.localData.get(contextId);

        if (subMap !== undefined) {
            subMap.variables.delete(name);
            this.refresh();
        }
    }

    private deleteData(contextName: string) {
        let currData = this.globalRoot;
        let currKey: string = "";
        let parentData = null;

        if (contextName) {
            const parts = contextName.split(".");

            for (let i = 0; i < parts.length; ++i) {
                const part = parts[i];

                if (!part) {
                    continue;
                }

                let subMap = currData.children.get(part);

                if (subMap === undefined) {
                    return;
                    //currData.set(part, subMap = currData = new Map<string, any>());
                }

                parentData = currData;
                currKey = part;
                currData = subMap;
            }
        }

        currData.clear();

        if (parentData) {
            parentData.children.delete(currKey);
        }
    }

    private writeData(contextName: string, name: string, value: boolean | number | string) {
        let currData = this.globalRoot;

        if (contextName) {
            const parts = contextName.split(".");

            for (let i = 0; i < parts.length; ++i) {
                const part = parts[i];

                if (!part) {
                    continue;
                }

                let subMap = currData.children.get(part);

                if (subMap === undefined) {
                    currData.children.set(part, subMap = currData = new GlobalContainerNode(currData.fullContextName + currData.name + ".", part));
                }

                currData = subMap;
            }
        }

        currData.variables.set(name, new VariableNode(false, currData.fullContextName + currData.name + ".", name, value));
    }

    private writeLocalData(context: number, name: string, value: boolean | number | string) {
        let subMap = this.localData.get(context);

        if (subMap === undefined) {
            this.localData.set(context, subMap = new LocalContainerNode(context, "", "[" + context + "]"));
        }

        subMap.variables.set(name, new LocalVariableNode(context, "", name, value));
    }

    private globalRoot = new GlobalContainerNode("", "");
    private localData = new Map<number, ContainerNode>();

    private _onDidChangeTreeData: vscode.EventEmitter<any> = new vscode.EventEmitter<any>();
    readonly onDidChangeTreeData: vscode.Event<any> = this._onDidChangeTreeData.event;
}