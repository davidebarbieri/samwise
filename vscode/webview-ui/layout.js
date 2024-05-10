export function makeIcon(iconId, divClass) {
  const icon = document.createElement("div");
  icon.className = divClass;
  icon.innerHTML = `<span class="codicon codicon-${iconId}">`;

  return icon;
}

export function makeButtonIcon(iconId, tooltip, onClick, enabled = true) {
  const button = document.createElement("vscode-button");
  button.innerHTML = `<div class="icon"><span class="codicon codicon-${iconId}"></span><span class="tooltiptext">${tooltip}</span></div>`;
  button.setAttribute("appearance", "icon");
  button.setAttribute("aria-label", tooltip);

  if (!enabled) {
    button.setAttribute("disabled", "");
  }

  if (onClick) {
    button.addEventListener("click", onClick);
  }

  return button;
}

export function makeButtonIconSerializable(iconId, tooltip, onClick, enabled = true) {
  const button = document.createElement("vscode-button");
  button.innerHTML = `<div class="icon"><span class="codicon codicon-${iconId}"></span><span class="tooltiptext">${tooltip}</span></div>`;
  button.setAttribute("appearance", "icon");
  button.setAttribute("aria-label", tooltip);

  if (!enabled) {
    button.setAttribute("disabled", "");
  }

  if (onClick) {
    button.setAttribute("onclick", onClick);
  }

  return button;
}

export function makeButtonLabel(label, tooltip, onClick, enabled = true) {
  const button = document.createElement("vscode-button");
  button.innerHTML = `<div class="icon"><div class="button"><span>${label}</span><span class="tooltiptext">${tooltip}</span></div></div>`;
  button.setAttribute("appearance", "icon");
  button.setAttribute("aria-label", tooltip);

  if (!enabled) {
    button.setAttribute("disabled", "");
  }

  if (onClick) {
    button.addEventListener("click", onClick);
  }

  return button;
}

export function makeLabel(className, label) {
  const element = document.createElement("vscode-label");
  element.setAttribute("class", className);
  element.innerText = label;

  return element;
}

export function makeSpeechBalloon(character, avatar, message) {
  const nameText = document.createElement("vscode-label");
  nameText.innerText = character + ":";

  const characterAvatar = document.createElement("img");
  characterAvatar.setAttribute("class", "balloonAvatar");
  characterAvatar.setAttribute("src", avatar);
          
  const characterAvatarCanvas = document.createElement("div");
  characterAvatarCanvas.setAttribute("class", "balloonAvatarCanvas");
  characterAvatarCanvas.appendChild(characterAvatar);

  const text = document.createElement("vscode-label");
  text.innerText = message.replaceAll("↵", "\n");

  const balloonName = document.createElement("div");
  balloonName.setAttribute("class", "balloonName");
  balloonName.appendChild(nameText);

  const balloon = document.createElement("div");
  balloon.setAttribute("class", "balloon");
  balloon.appendChild(text);

  balloon.appendChild(balloonName);
  balloon.appendChild(text);

  const balloonArrow = document.createElement("div");
  balloonArrow.setAttribute("class", "balloonArrow");

  const balloonFrame = document.createElement("div");
  balloonFrame.setAttribute("class", "balloonFrame");
  balloonFrame.appendChild(characterAvatarCanvas);
  balloonFrame.appendChild(balloonArrow);
  balloonFrame.appendChild(balloon);

  return balloonFrame;
}


export function makeHistorySpeechBalloon(character, avatar, message, mute) {
  const nameText = document.createElement("vscode-label");
  nameText.innerText = character + ":";

  const characterAvatar = document.createElement("img");
  characterAvatar.setAttribute("class", "balloonAvatar");
  characterAvatar.setAttribute("src", avatar);
          
  const characterAvatarCanvas = document.createElement("div");
  characterAvatarCanvas.setAttribute("class", "balloonAvatarCanvas");
  characterAvatarCanvas.appendChild(characterAvatar);

  const text = document.createElement("vscode-label");
  text.innerText = message.replaceAll("↵", "\n");

  const balloonName = document.createElement("div");
  balloonName.setAttribute("class", "balloonName");
  balloonName.appendChild(nameText);

  const balloon = document.createElement("div");
  balloon.setAttribute("class", mute ? "historyBalloon action" : "historyBalloon speech");
  balloon.appendChild(text);

  balloon.appendChild(balloonName);
  balloon.appendChild(text);

  const historyElement = document.createElement("div");
  historyElement.setAttribute("class", "historyElement");
  historyElement.appendChild(characterAvatarCanvas);
  historyElement.appendChild(balloon);

  return historyElement;
}

export function makeCaption(message) {
  const caption = document.createElement("vscode-label");
  caption.innerText = message.replaceAll("↵", "\n");

  const content = document.createElement("div");
  content.setAttribute("class", "caption");

  content.appendChild(caption);
  return content;
}

export function makeHistoryCaption(message) {
  const caption = document.createElement("vscode-label");
  caption.innerText = message.replaceAll("↵", "\n");

  const content = document.createElement("div");
  content.setAttribute("class", "historyCaption");

  content.appendChild(caption);
  return content;
}