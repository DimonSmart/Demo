window.getSelectionStart = (element) => element.selectionStart;
window.getSelectionEnd = (element) => element.selectionEnd;
window.updateTextArea = (element, cursorPosition) => {
    element.focus();
    element.setSelectionRange(cursorPosition, cursorPosition);
};
window.updateTextAreaContent = (element, value) => {
    element.value = value;
    element.dispatchEvent(new Event('input'));
};
