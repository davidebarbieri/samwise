const vscode = acquireVsCodeApi();

var lastNode = {};
var runningTimers = {};

function onViewAdvance(id) {
  vscode.postMessage({ command: "advance", id: id });
}

function onViewCaptionStart(id) {
  let dialogue = document.getElementById("dialogue" + id);
  dialogue.innerHTML = '<vscode-progress-ring role="alert" aria-label="Loading" aria-live="assertive"></vscode-progress-ring>';
  vscode.postMessage({ command: "advance", id: id });
}

function onWaitForMissingDialogueStart(id, name) {
  vscode.postMessage({ command: "tryResolve", id: id, name: name });
}

function onChoose(id, choiceId, character, avatar) {
  // Reset Timers
  if (runningTimers[id]) {
    for (var i = 0; i < runningTimers[id].length; i++) {
      clearInterval(runningTimers[id][i]);
    }

    delete runningTimers[id];
  }

  if (choiceId === -1) {
    lastNode[id] = null;
    vscode.postMessage({ command: "choose", id: id, choice: choiceId });
  }
  else {
    let choiceButton = document.getElementById("Choice" + id + "_" + choiceId);
    let isMute = choiceButton.classList.contains("action");

    lastNode[id] = { character: character, avatar: avatar, text: choiceButton.innerText, mute: isMute };
    vscode.postMessage({ command: "choose", id: id, choice: choiceId });
  }
}

function onViewCompleteChallenge(id, name, result) {
  let dialogue = document.getElementById("dialogue" + id);
  dialogue.innerHTML = ""; // clear form

  const resultText = (result ? `Check ${name} Passed` : `Check ${name} Failed`);

  const check = document.createElement("vscode-label");
  check.innerText = resultText;

  const content = document.createElement("div");
  content.setAttribute("class", result ? "checkPassed" : "checkFailed");
  content.appendChild(check);

  // Save to history
  let history = document.getElementById("history");

  history.insertBefore(content, history.firstChild);


  vscode.postMessage({ command: "completeChallenge", id: id, result: result });
}

function onViewStop(id) {
  vscode.postMessage({ command: "stop", id: id });
}

function onViewJump(id) {
  vscode.postMessage({ command: "jump", id: id });
}

function onElementSelected(layout, symbol, selected, isNode) {
  const selectionRootDiv = document.getElementById("selectionroot");
  let oldSelectionDiv = document.getElementById("selection");

  selectionDiv = document.createElement("div");
  selectionDiv.setAttribute("id", "selection");
  selectionDiv.setAttribute("class", "selection");
  selectionRootDiv.replaceChild(selectionDiv, oldSelectionDiv);

  const runButton = layout.makeButtonIcon("play", "Run", e => {
    vscode.postMessage({ execute: "samwise.runDialogue" });
  }, selected);

  let label;

  if (selected) {
    label = layout.makeLabel("selectionText", `${symbol}`);
  }
  else {
    label = layout.makeLabel("selectionTextDisabled", "No dialogue selected");
  }

  selectionDiv.appendChild(runButton);

  const runFromNodeButton = layout.makeButtonIcon("run-below", "Run from node", e => {
    vscode.postMessage({ execute: "samwise.runDialogueFromNode" });
  }, selected && isNode);

  selectionDiv.appendChild(runFromNodeButton);
  selectionDiv.appendChild(label);
}

function hasAutoAdvance()
{
  const option = document.getElementById("selection_options_auto_advance");
  return option.classList.contains("checked");
}

function computeReadTime(text, multiplier)
{
  const wordsPerSecond = 3.5;
  const wordCount = text.split(/\s+/).length;
  let readingTime = wordCount / wordsPerSecond;
  return 0.5 + readingTime * multiplier;
}

function createAdvanceButtonOrAutoadvance(id, text, navigation)
{
  if (hasAutoAdvance())
    {
      setTimeout(() => {
        onViewCaptionStart(id);
      }, Math.floor(computeReadTime(text, 1) * 1000));
    }
    else
    {
      const button = document.createElement("vscode-button");
      button.innerText = "Advance";
      button.setAttribute("appearance", "primary");
      button.setAttribute("onclick", `onViewCaptionStart(${id})`);

      navigation.appendChild(button);
    }
}

function initialize(layout) {
  onElementSelected(layout, '', '', false);

  var prevState = vscode.getState();
  if (prevState) {
    // restore DOM
    const root = document.getElementById("root");
    const history = document.getElementById("history");
    root.innerHTML = prevState.dialogues;
    history.innerHTML = prevState.history;
    lastNode = prevState.lastNode ? prevState.lastNode : {};
  }

  window.addEventListener('message', event => {
    const message = event.data; // The JSON data our extension sent

    const root = document.getElementById("root");
    const history = document.getElementById("history");

    switch (message.command) {
      case 'onElementSelected':
        {
          const symbol = message.symbol;
          const selected = symbol !== '';
          const isNode = message.isNode;

          onElementSelected(layout, symbol, selected, isNode);

          break;
        }
      case 'onDialogueContextStart':
        {
          // Add Container
          const dialogueContainerDiv = document.createElement("div");
          dialogueContainerDiv.setAttribute("id", "dialogueContainer" + message.id);
          root.appendChild(dialogueContainerDiv);

          // Add Header in Container
          const headerDiv = document.createElement("div");
          headerDiv.setAttribute("id", "header" + message.id);
          headerDiv.setAttribute("class", "header");

          const titleDiv = document.createElement("div");
          titleDiv.innerHTML = `<vscode-label class="title" id="title${message.id}">[${message.id}] ${message.title}</vscode-label>`;

          const stopButton = layout.makeButtonIconSerializable("close", "Stop", `onViewStop(${message.id})`);
          const jumpButton = layout.makeButtonIconSerializable("go-to-file", "Go to line", `onViewJump(${message.id})`);

          headerDiv.appendChild(stopButton);
          headerDiv.appendChild(titleDiv);
          headerDiv.appendChild(jumpButton);

          dialogueContainerDiv.appendChild(headerDiv);

          // Add Content in Container
          const dialogueDiv = document.createElement("div");
          dialogueDiv.setAttribute("id", "dialogue" + message.id);
          dialogueDiv.setAttribute("class", "dialogue");
          dialogueContainerDiv.appendChild(dialogueDiv);

          // Add Separator in Container
          const separator = document.createElement("vscode-divider");
          dialogueContainerDiv.appendChild(separator);

          document.getElementById("dialogueplaceholder")?.remove();

          break;
        }

      case 'onDialogueContextEnd':
        {
          document.getElementById("dialogueContainer" + message.id).remove();

          if (root.childElementCount === 0) {
            let label = layout.makeLabel("selectionTextDisabled", "No dialogue running");
            label.setAttribute("id", "dialogueplaceholder");

            root.appendChild(label);
          }

          break;
        }

      case 'onCaptionStart':
        {
          let dialogue = document.getElementById("dialogue" + message.id);

          const content = layout.makeCaption(message.text);

          const navigation = document.createElement("div");
          navigation.setAttribute("class", "navigation");

          createAdvanceButtonOrAutoadvance(message.id, message.text, navigation);

          dialogue.appendChild(content);
          dialogue.appendChild(navigation);

          lastNode[message.id] = { text: message.text };
          break;
        }

      case 'onWaitTimeStart':
        {
          let dialogue = document.getElementById("dialogue" + message.id);
          dialogue.innerHTML = '<div class="threedots"><div class="dot"></div><div class="dot"></div><div class="dot"></div></div>';

          const optionSkip = document.getElementById("selection_options_skip");

          if (optionSkip.classList.contains("checked")) {
            onViewAdvance(message.id);
          }
          else {
            setTimeout(() => {
              onViewAdvance(message.id);
            }, Math.floor(message.time * 1000));
          }

          break;
        }

      case 'onSpeechStart':
        {
          let dialogue = document.getElementById("dialogue" + message.id);

          const navigation = document.createElement("div");
          navigation.setAttribute("class", "navigation");

          createAdvanceButtonOrAutoadvance(message.id, message.text, navigation);

          let balloonFrame = layout.makeSpeechBalloon(message.character, message.avatar, message.text);
          dialogue.appendChild(balloonFrame);
          dialogue.appendChild(navigation);

          lastNode[message.id] = { character: message.character, avatar: message.avatar, text: message.text };
          break;
        }

      case 'onSpeechOptionStart':
        {
          document.getElementById("dialogue" + message.id).innerHTML = "";

          // Save to history
          let history = document.getElementById("history");
          let balloonFrame = layout.makeHistorySpeechBalloon(message.character, message.avatar, message.text, false);

          history.insertBefore(balloonFrame, history.firstChild);

          vscode.postMessage({ command: "advance", id: message.id });
          break;
        }

      case 'onChoiceStart':
        {
          const choiceList = document.createElement("div");
          choiceList.setAttribute("class", "choicePanel");

          let dialogue = document.getElementById("dialogue" + message.id);

          const nameText = document.createElement("vscode-label");
          nameText.setAttribute("class", "choiceName");
          nameText.innerText = message.character + ":";

          const characterAvatar = document.createElement("img");
          characterAvatar.setAttribute("class", "balloonAvatar");
          characterAvatar.setAttribute("src", message.avatar);
          
          const characterAvatarCanvas = document.createElement("div");
          characterAvatarCanvas.setAttribute("class", "balloonAvatarCanvas");

          characterAvatarCanvas.appendChild(characterAvatar);
          dialogue.appendChild(characterAvatarCanvas);
          dialogue.appendChild(nameText);
          dialogue.appendChild(choiceList);

          for (let i = 0; i < message.choices.length; ++i) {
            const button = document.createElement("vscode-button");
            button.setAttribute("id", "Choice" + message.id + "_" + message.choices[i].id);
            button.innerText = message.choices[i].text;

            if (message.choices[i].mute) {
              button.setAttribute("class", "choice action");
              button.setAttribute("appearance", "secondary");
            }
            else {
              button.setAttribute("class", "choice speech");
              button.setAttribute("appearance", "secondary");
            }

            button.setAttribute("onclick", `onChoose(${message.id}, ${message.choices[i].id}, "${message.character}", "${message.avatar}")`);

            const choiceLine = document.createElement("div");
            choiceLine.setAttribute("class", "choiceLine");

            const choiceHeader = document.createElement("div");
            choiceHeader.setAttribute("class", "iconChoiceContainer");

            if (message.choices[i].time > 0) {
              let progressValue = 0;
              let speed = 16;
              let timeDuration = message.choices[i].time;

              let progress = setInterval(() => {

                progressValue += (speed * 0.001) / timeDuration;

                choiceHeader.style.background = `conic-gradient(
                    #4d5bf9 ${progressValue * 360}deg,
                    #cadcff ${progressValue * 360}deg
                )`;

                if (progressValue >= 1) {
                  clearInterval(progress);
                  choiceLine.remove();

                  // if no choice left, select null
                  if (choiceList.childElementCount === 0) {
                    onChoose(message.id, -1, message.character, message.avatar);
                  }
                }
              }, 16);

              if (!runningTimers[message.id]) {
                runningTimers[message.id] = new Array();
              }

              runningTimers[message.id].push(progress);
            }

            if (message.choices[i].mute) {
              choiceHeader.appendChild(layout.makeIcon("symbol-event", "iconPre"));
            }
            else {
              choiceHeader.appendChild(layout.makeIcon("comment", "iconPre"));
            }

            choiceLine.appendChild(choiceHeader);
            choiceLine.appendChild(button);

            choiceList.appendChild(choiceLine);
          }
          break;
        }

      case 'onCaptionEnd':
        {
          document.getElementById("dialogue" + message.id).innerHTML = "";

          var node = lastNode[message.id];
          // Save to history
          let history = document.getElementById("history");
          let balloonFrame = layout.makeHistoryCaption(node.text);

          history.insertBefore(balloonFrame, history.firstChild);

          break;
        }

      case 'onWaitTimeEnd':
        {
          document.getElementById("dialogue" + message.id).innerHTML = "";
          break;
        }

      case 'onChallengeStart':
        {
          document.getElementById("dialogue" + message.id).innerHTML = "";

          const choiceList = document.createElement("div");
          choiceList.setAttribute("class", "challengePanel");

          let dialogue = document.getElementById("dialogue" + message.id);
          dialogue.appendChild(choiceList);

          const buttonPass = document.createElement("vscode-button");
          const buttonFail = document.createElement("vscode-button");

          buttonPass.innerText = "SUCCESS";
          buttonFail.innerText = "FAIL";

          buttonPass.setAttribute("class", "choice action");
          buttonFail.setAttribute("class", "choice action");

          buttonPass.setAttribute("appearance", "secondary");
          buttonFail.setAttribute("appearance", "secondary");

          buttonPass.setAttribute("onclick", `onViewCompleteChallenge(${message.id}, \"${message.name}\", true)`);
          buttonFail.setAttribute("onclick", `onViewCompleteChallenge(${message.id}, \"${message.name}\", false)`);

          const choiceLine = document.createElement("div");
          choiceLine.setAttribute("class", "choiceLine");

          choiceLine.appendChild(buttonPass);
          choiceLine.appendChild(buttonFail);

          const nameDiv = document.createElement("div");
          let name = layout.makeLabel("challengeName", message.name);
          nameDiv.appendChild(name);

          choiceList.appendChild(nameDiv);
          choiceList.appendChild(choiceLine);

          break;
        }
      case 'onChoiceEnd':
        {
          document.getElementById("dialogue" + message.id).innerHTML = "";
          break;
        }
      case 'onSpeechEnd':
        {
          document.getElementById("dialogue" + message.id).innerHTML = "";

          var node = lastNode[message.id];
          // Save to history
          let history = document.getElementById("history");
          let balloonFrame = layout.makeHistorySpeechBalloon(node.character, node.avatar, node.text, node.mute);

          history.insertBefore(balloonFrame, history.firstChild);

          break;
        }
      case 'onClearHistory':
        {
          history.innerHTML = "";
          break;
        }
      case 'onClear':
        {
          root.innerHTML = "";
          history.innerHTML = "";

          let label = layout.makeLabel("selectionTextDisabled", "No dialogue running");
          label.setAttribute("id", "dialogueplaceholder");

          root.appendChild(label);

          vscode.setState(null);
          break;
        }
      case 'onWaitForMissingDialogueStart':
        {
          let dialogue = document.getElementById("dialogue" + message.id);

          const button = document.createElement("vscode-button");
          button.innerText = "Resolve";
          button.setAttribute("appearance", "primary");
          button.setAttribute("onclick", `onWaitForMissingDialogueStart(${message.id}, "${message.name}")`);


          const caption = document.createElement("vscode-label");
          caption.innerHTML = "Missing dialogue <b>" + message.name + "</b>";

          const content = document.createElement("div");
          content.setAttribute("class", "missingReference");

          const navigation = document.createElement("div");
          navigation.setAttribute("class", "navigation");

          content.appendChild(caption);
          navigation.appendChild(button);

          dialogue.appendChild(content);
          dialogue.appendChild(navigation);
          break;
        }
      case 'onWaitForMissingDialogueEnd':
        {
          document.getElementById("dialogue" + message.id).innerHTML = "";
          break;
        }
      case 'onError':
        {
          let dialogue = document.getElementById("dialogue" + message.id);

          const errorMsg = document.createElement("vscode-label");
          errorMsg.innerText = message.text;

          const content = document.createElement("div");
          content.setAttribute("class", "error");

          content.appendChild(layout.makeIcon("warning", "messageIcon"));
          content.appendChild(errorMsg);

          dialogue.appendChild(content);
          break;
        }
      case 'setConfiguration':
        {
          const optionClearDialogues = document.getElementById("selection_options_clear_dialogues");
          const optionClearData = document.getElementById("selection_options_clear_data");
          const optionAutoAdvance = document.getElementById("selection_options_auto_advance");

          disableOptionsEvents = true;
          optionClearDialogues.checked = message.clearDialoguesOnPlay;
          optionClearData.checked = message.clearDataOnPlay;
          optionAutoAdvance.checked = message.autoAdvance;
          disableOptionsEvents = false;

          break;
        }
    }

    vscode.setState({ dialogues: root.innerHTML, history: history.innerHTML, lastNode: lastNode });
  });

  const foldButton = document.getElementById("selection_options_fold");
  foldButton.addEventListener("click", () => {
    const selectionOptions = document.getElementById("selection_options");
    if (selectionOptions.getAttribute("class") === "folded") {
      selectionOptions.setAttribute("class", "unfolded");
    }
    else {
      selectionOptions.setAttribute("class", "folded");
    }
  });

  const optionClearDialogues = document.getElementById("selection_options_clear_dialogues");
  const optionClearData = document.getElementById("selection_options_clear_data");
  const optionAutoAdvance = document.getElementById("selection_options_auto_advance");

  optionClearDialogues.addEventListener("change", e => {
    if (!disableOptionsEvents) {
      vscode.postMessage({ updateSetting: "samwise.clearDialoguesOnPlay", value: e.target.checked });
    }
  });

  optionClearData.addEventListener("change", e => {
    if (!disableOptionsEvents) {
      vscode.postMessage({ updateSetting: "samwise.clearDataOnPlay", value: e.target.checked });
    }
  });

  optionAutoAdvance.addEventListener("change", e => {
    if (!disableOptionsEvents) {
      vscode.postMessage({ updateSetting: "samwise.autoAdvance", value: e.target.checked });
    }
  });

  vscode.postMessage({ command: "initDone" });
}

import("./layout.js").then(initialize);