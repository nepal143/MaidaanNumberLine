mergeInto(LibraryManager.library, {
    FocusInputField: function(inputFieldID) {
        var input = document.getElementById(inputFieldID);
        if (input) {
            input.focus();
        }
    }
});
