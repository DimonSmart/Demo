window.captureScreenshot = async function () {
    // Attempt to use the Web Capture API
    if (document.documentElement.capture) {
        try {
            // Capture the entire page using the Web Capture API (returns a Blob)
            const blob = await document.documentElement.capture();
            // Convert Blob to data URL
            return await new Promise((resolve, reject) => {
                const reader = new FileReader();
                reader.onloadend = () => resolve(reader.result);
                reader.onerror = reject;
                reader.readAsDataURL(blob);
            });
        } catch (error) {
            console.error("Error using Web Capture API:", error);
            // If the Web Capture API doesn't work, fall back to html2canvas
        }
    }

    // Calculate the dimensions of the entire page
    const body = document.body;
    const html = document.documentElement;
    const width = Math.max(
        body.scrollWidth, body.offsetWidth,
        html.clientWidth, html.scrollWidth, html.offsetWidth
    );
    const height = Math.max(
        body.scrollHeight, body.offsetHeight,
        html.clientHeight, html.scrollHeight, html.offsetHeight
    );

    try {
        // Capture the entire page using html2canvas
        const canvas = await html2canvas(document.body, {
            scrollY: 0,
            windowWidth: width,
            windowHeight: height,
            useCORS: true,
            allowTaint: false
        });
        return canvas.toDataURL("image/png");
    } catch (error) {
        console.error("Error using html2canvas:", error);
        throw error;
    }
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
