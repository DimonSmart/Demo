const contexts = new Map();
let nextContextId = 1;

export function isScreenCaptureSupported() {
    const mediaDevices = navigator?.mediaDevices;
    return !!(mediaDevices && typeof mediaDevices.getDisplayMedia === "function");
}

function ensureContext(handle) {
    const context = contexts.get(handle);
    if (!context) {
        throw new Error(`Capture context ${handle} is not available.`);
    }

    return context;
}

function stopStream(stream) {
    if (!stream) {
        return;
    }

    stream.getTracks().forEach(track => {
        try {
            track.stop();
        } catch (error) {
            console.warn("Failed to stop media track", error);
        }
    });
}

function resetContextState(context) {
    context.stream = null;
    context.frameVersion = 0;
}

async function waitForVideoReady(video) {
    if (video.readyState >= HTMLMediaElement.HAVE_METADATA) {
        return;
    }

    await new Promise(resolve => {
        const handler = () => {
            video.removeEventListener("loadedmetadata", handler);
            resolve();
        };

        video.addEventListener("loadedmetadata", handler, { once: true });
    });
}

export function createContext(videoElementId, canvasElementId) {
    const video = document.getElementById(videoElementId);
    const canvas = document.getElementById(canvasElementId);

    if (!(video instanceof HTMLVideoElement) || !(canvas instanceof HTMLCanvasElement)) {
        throw new Error("Capture preview elements are not available.");
    }

    const ctx = canvas.getContext("2d", { willReadFrequently: true });
    if (!ctx) {
        throw new Error("Unable to acquire canvas context.");
    }

    const handle = nextContextId++;
    video.autoplay = true;
    video.muted = true;
    video.playsInline = true;

    contexts.set(handle, {
        video,
        canvas,
        ctx,
        stream: null,
        frameVersion: 0
    });

    return handle;
}

export async function attachStream(handle, stream) {
    const context = ensureContext(handle);
    const previousStream = context.stream;

    resetContextState(context);
    stopStream(previousStream);

    if (!stream) {
        context.video.pause?.();
        context.video.srcObject = null;
        return;
    }

    context.stream = stream;
    context.video.srcObject = stream;

    try {
        await context.video.play();
    } catch (error) {
        console.warn("Unable to start media playback", error);
    }

    await waitForVideoReady(context.video);
}

export function stopCapture(handle) {
    const context = contexts.get(handle);
    if (!context) {
        return;
    }

    stopStream(context.stream);
    context.video.pause?.();
    context.video.srcObject = null;
    resetContextState(context);
}

export function disposeContext(handle) {
    const context = contexts.get(handle);
    if (!context) {
        return;
    }

    stopCapture(handle);
    contexts.delete(handle);
}

export function tryCaptureFrame(handle, knownVersion) {
    const context = ensureContext(handle);

    if (!context.stream) {
        return null;
    }

    const video = context.video;
    if (video.readyState < HTMLMediaElement.HAVE_CURRENT_DATA) {
        console.log('[tryCaptureFrame] Video not ready, readyState:', video.readyState);
        return null;
    }

    const width = video.videoWidth;
    const height = video.videoHeight;

    if (!width || !height) {
        console.log('[tryCaptureFrame] Invalid video dimensions:', width, 'x', height);
        return null;
    }

    if (context.canvas.width !== width) {
        context.canvas.width = width;
    }

    if (context.canvas.height !== height) {
        context.canvas.height = height;
    }

    try {
        context.ctx.drawImage(video, 0, 0, width, height);
        const imageData = context.ctx.getImageData(0, 0, width, height);

        if (!imageData || !imageData.data || imageData.data.length === 0) {
            console.error('[tryCaptureFrame] ImageData is empty or invalid');
            return null;
        }

        const version = context.frameVersion + 1;
        context.frameVersion = version;

        if (version === knownVersion) {
            return null;
        }

        const pixels = new Uint8Array(imageData.data);
        console.log(`[tryCaptureFrame] Captured frame: ${width}x${height}, pixels.length=${pixels.length}, version=${version}`);

        return {
            width,
            height,
            version,
            pixels: pixels
        };
    } catch (error) {
        console.error('[tryCaptureFrame] Error capturing frame:', error);
        return null;
    }
}
