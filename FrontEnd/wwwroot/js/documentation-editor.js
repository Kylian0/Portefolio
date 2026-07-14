window.documentationEditor = {
    enableLeaveWarning: function () { window.onbeforeunload = () => "Des modifications ne sont pas enregistrées."; },
    disableLeaveWarning: function () { window.onbeforeunload = null; },
    saveDraft: function (key, value) { sessionStorage.setItem(key, value); },
    loadDraft: function (key) { return sessionStorage.getItem(key); },
    clearDraft: function (key) { sessionStorage.removeItem(key); }
};
