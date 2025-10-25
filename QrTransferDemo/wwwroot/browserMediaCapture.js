const contexts = new Map();
let nextContextId = 1;

function ensureContext(handle) {
    const context = contexts.get(handle);
    if (!context) {
        throw new Error(`Capture context ${handle} is not available.`);
    }

    return context;
}

function stopFramePump(context) {
    if (context.pumpHandle === null) {
        return;
    }

    if (context.useVideoFrameCallback && typeof context.video.cancelVideoFrameCallback === "function") {
        context.video.cancelVideoFrameCallback(context.pumpHandle);
    } else {
        cancelAnimationFrame(context.pumpHandle);
    }

    context.pumpHandle = null;
}

function scheduleFramePump(context) {
    if (!context.stream) {
        return;
    }

    if (context.useVideoFrameCallback && typeof context.video.requestVideoFrameCallback === "function") {
        context.pumpHandle = context.video.requestVideoFrameCallback((now, metadata) => {
            renderFrame(context, metadata && typeof metadata.width === "number" ? metadata.width : 0, metadata && typeof metadata.height === "number" ? metadata.height : 0);
        });
    } else {
        context.pumpHandle = requestAnimationFrame(() => {
            renderFrame(context, 0, 0);
        });
    }
}

function renderFrame(context, metadataWidth, metadataHeight) {
    if (!context.stream) {
        return;
    }

    const video = context.video;
    const width = video.videoWidth || metadataWidth;
    const height = video.videoHeight || metadataHeight;

    if (!width || !height) {
        scheduleFramePump(context);
        return;
    }

    if (context.canvas.width !== width) {
        context.canvas.width = width;
    }

    if (context.canvas.height !== height) {
        context.canvas.height = height;
    }

    context.ctx.drawImage(video, 0, 0, width, height);
    const imageData = context.ctx.getImageData(0, 0, width, height);
    context.latestFrame = {
        width,
        height,
        version: context.frameVersion + 1,
        pixels: new Uint8Array(imageData.data)
    };
    context.frameVersion = context.latestFrame.version;
    scheduleFramePump(context);
}

function startFramePump(context) {
    stopFramePump(context);
    context.latestFrame = null;
    context.frameVersion = 0;
    scheduleFramePump(context);
}

export function createContext(videoElementId, canvasElementId) {
    const video = document.getElementById(videoElementId);
    const canvas = document.getElementById(canvasElementId);

    if (!(video instanceof HTMLVideoElement) || !(canvas instanceof HTMLCanvasElement)) {
        throw new Error("Capture preview elements are not available.");
    }

    const ctx = canvas.getContext("2d");
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
        latestFrame: null,
        frameVersion: 0,
        pumpHandle: null,
        useVideoFrameCallback: typeof video.requestVideoFrameCallback === "function"
    });

    return handle;
}

export async function startCapture(handle, source, frameRateHint, width, height, facingMode) {
    const context = ensureContext(handle);
    const previousStream = context.stream;

    context.stream = null;
    stopFramePump(context);

    if (previousStream) {
        previousStream.getTracks().forEach(track => {
            try {
                track.stop();
            } catch (error) {
                console.warn("Failed to stop media track", error);
            }
        });
    }

    const videoConstraints = {};
    if (typeof frameRateHint === "number" && !Number.isNaN(frameRateHint)) {
        videoConstraints.frameRate = frameRateHint;
    }
    if (typeof width === "number" && width > 0) {
        videoConstraints.width = width;
    }
    if (typeof height === "number" && height > 0) {
        videoConstraints.height = height;
    }
    if (typeof facingMode === "string" && facingMode) {
        videoConstraints.facingMode = facingMode;
    }

    const isScreen = source === 0;
    let stream;

    try {
        if (isScreen) {
            const screenConstraints = { video: {}, audio: false };
            if (videoConstraints.frameRate) {
                screenConstraints.video.frameRate = videoConstraints.frameRate;
            }
            stream = await navigator.mediaDevices.getDisplayMedia(screenConstraints);
        } else {
            const cameraConstraints = { video: videoConstraints, audio: false };
            stream = await navigator.mediaDevices.getUserMedia(cameraConstraints);
        }
    } catch (error) {
        throw error;
    }

    context.stream = stream;
    context.latestFrame = null;
    context.frameVersion = 0;

    context.video.srcObject = stream;
    try {
        await context.video.play();
    } catch (error) {
        console.warn("Unable to start media playback", error);
    }

    startFramePump(context);
}

export function stopCapture(handle) {
    const context = contexts.get(handle);
    if (!context) {
        return;
    }

    const stream = context.stream;
    context.stream = null;
    stopFramePump(context);

    if (stream) {
        stream.getTracks().forEach(track => {
            try {
                track.stop();
            } catch (error) {
                console.warn("Failed to stop media track", error);
            }
        });
    }

    context.video.pause?.();
    context.video.srcObject = null;
    context.latestFrame = null;
    context.frameVersion = 0;
}

export function disposeContext(handle) {
    const context = contexts.get(handle);
    if (!context) {
        return;
    }

    stopCapture(handle);
    contexts.delete(handle);
}

export function tryReadLatestFrame(handle, knownVersion) {
    const context = ensureContext(handle);
    const frame = context.latestFrame;

    if (!frame || frame.version === knownVersion) {
        return null;
    }

    return {
        width: frame.width,
        height: frame.height,
        version: frame.version,
        pixels: frame.pixels
    };
}
