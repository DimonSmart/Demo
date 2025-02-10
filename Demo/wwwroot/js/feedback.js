window.captureScreenshot = async function () {
    var element = document.body;
    var canvas = await html2canvas(element);
    return canvas.toDataURL("image/png");
};

window.openFeedbackDialog = function () {
    var dlg = document.getElementById("feedbackDialog");
    if (dlg) {
        dlg.showModal();
    }
};

window.closeFeedbackDialog = function () {
    var dlg = document.getElementById("feedbackDialog");
    if (dlg) {
        dlg.close();
    }
};
