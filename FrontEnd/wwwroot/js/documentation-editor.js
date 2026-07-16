window.documentationEditor = {
    enableLeaveWarning: function () { window.onbeforeunload = () => "Des modifications ne sont pas enregistrées."; },
    disableLeaveWarning: function () { window.onbeforeunload = null; },
    saveDraft: function (key, value) { sessionStorage.setItem(key, value); },
    loadDraft: function (key) { return sessionStorage.getItem(key); },
    clearDraft: function (key) { sessionStorage.removeItem(key); },
    registerMediaPicker: function (reference) { window.documentationEditor.mediaPickerReference = reference; },
    openMediaPicker: function (type, multiple) {
        return window.documentationEditor.mediaPickerReference?.invokeMethodAsync('OpenMediaPickerFromTinyMce', type, multiple);
    }
};
