Utils = {};

Utils.PreventChooseText = () => {
    window.getSelection ? window.getSelection().removeAllRanges() : document.selection.empty();
}