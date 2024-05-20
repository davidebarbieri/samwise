const vscode = acquireVsCodeApi();

import * as layout from './layout.js';

const rootDiv = document.getElementById("root");

// Add Container
const toolboxDiv = document.createElement("div");
toolboxDiv.setAttribute("id", "toolbox");
toolboxDiv.setAttribute("class", "toolbox");
rootDiv.appendChild(toolboxDiv);

function addToolButtonIcon(div, icon, tooltip, commandToExecute) {
    div.appendChild(layout.makeButtonIcon(icon, tooltip, e => { vscode.postMessage({ command: commandToExecute }); }));
}

function addToolButtonLabel(div, label, tooltip, commandToExecute) {
    div.appendChild(layout.makeButtonLabel(label, tooltip, e => { vscode.postMessage({ command: commandToExecute }); }));
}

function addToolButtonCharacter(div, character, tooltip) {
    div.appendChild(layout.makeButtonLabel(character, tooltip, e => { vscode.postMessage({ character: character }); }));
}

addToolButtonIcon(toolboxDiv, "comment-discussion", "Add Dialogue", "samwise.addDialogue");
addToolButtonIcon(toolboxDiv, "comment", "Add Speech Balloon", "samwise.addSpeech");
addToolButtonIcon(toolboxDiv, "symbol-constant", "Add Caption Balloon", "samwise.addCaption");

addToolButtonIcon(toolboxDiv, "list-ordered", "Add Choice", "samwise.addChoice");
addToolButtonIcon(toolboxDiv, "question", "Add Fallback", "samwise.addFallback");
addToolButtonIcon(toolboxDiv, "gift", "Add Random", "samwise.addRandom");
addToolButtonIcon(toolboxDiv, "table", "Add Score", "samwise.addScore");

addToolButtonIcon(toolboxDiv, "list-flat", "Add Sequence Selection", "samwise.addSequence");
addToolButtonIcon(toolboxDiv, "debug-restart-frame", "Add Loop Selection", "samwise.addLoop");
addToolButtonIcon(toolboxDiv, "remote", "Add Ping Pong", "samwise.addPingPong");

addToolButtonIcon(toolboxDiv, "debug-step-over", "Add Goto", "samwise.addGoto");
addToolButtonIcon(toolboxDiv, "type-hierarchy-sub", "Fork Dialogue", "samwise.addFork");
addToolButtonIcon(toolboxDiv, "type-hierarchy-super", "Join Dialogue", "samwise.addJoin");
addToolButtonIcon(toolboxDiv, "circle-slash", "Cancel Dialogue", "samwise.addCancel");
addToolButtonIcon(toolboxDiv, "sync", "Await Dialogue", "samwise.addAwait");

addToolButtonIcon(toolboxDiv, "code", "Add Code", "samwise.addCode");

addToolButtonIcon(toolboxDiv, "info", "Add Comment", "samwise.addComment");

addToolButtonIcon(toolboxDiv, "bookmark", "Add Label", "samwise.addLabel");
addToolButtonIcon(toolboxDiv, "tag", "Add Tags", "samwise.addTags");
addToolButtonIcon(toolboxDiv, "game", "Add Challenge", "samwise.addCheck");
addToolButtonIcon(toolboxDiv, "watch", "Wait Time", "samwise.addWait");


toolboxDiv.appendChild(document.createElement("vscode-divider"));

addToolButtonCharacter(toolboxDiv, "↵", "Line-break (Shift+Enter)");
addToolButtonCharacter(toolboxDiv, "‑", "Non-breaking hyphen");
addToolButtonCharacter(toolboxDiv, "–", "En-dash");
addToolButtonCharacter(toolboxDiv, "—", "Em-dash");
addToolButtonCharacter(toolboxDiv, "―", "Horizontal bar");
addToolButtonCharacter(toolboxDiv, "…", "Ellipsis");
addToolButtonCharacter(toolboxDiv, "⋮", "Vertical Ellipsis");

addToolButtonCharacter(toolboxDiv, "♫", "Beamed quavers");


toolboxDiv.appendChild(document.createElement("vscode-divider"));

