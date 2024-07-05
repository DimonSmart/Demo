window.imageProcessing = {
    cropImage: function (imageDataUrl, left, right, top, bottom) {
        return new Promise((resolve) => {
            const img = new Image();
            img.onload = () => {
                const canvas = document.createElement("canvas");
                const context = canvas.getContext("2d");

                canvas.width = img.width - left - right;
                canvas.height = img.height - top - bottom;

                context.drawImage(
                    img,
                    left,
                    top,
                    canvas.width,
                    canvas.height,
                    0,
                    0,
                    canvas.width,
                    canvas.height
                );

                resolve(canvas.toDataURL());
            };
            img.src = imageDataUrl;
        });
    },
    rotateImage: function (imageDataUrl, angle) {
        return new Promise((resolve) => {
            const img = new Image();
            img.onload = () => {
                const canvas = document.createElement("canvas");
                const context = canvas.getContext("2d");

                const width = img.width;
                const height = img.height;

                canvas.width = Math.abs(angle) % 180 === 90 ? height : width;
                canvas.height = Math.abs(angle) % 180 === 90 ? width : height;

                context.translate(canvas.width / 2, canvas.height / 2);
                context.rotate((angle * Math.PI) / 180);
                context.drawImage(img, -width / 2, -height / 2);

                resolve(canvas.toDataURL());
            };
            img.src = imageDataUrl;
        });
    },
    addText: function (imageDataUrl, text, x, y, fontSize, color) {
        return new Promise((resolve) => {
            const img = new Image();
            img.onload = () => {
                const canvas = document.createElement("canvas");
                const context = canvas.getContext("2d");

                canvas.width = img.width;
                canvas.height = img.height;

                context.drawImage(img, 0, 0);
                context.font = `${fontSize}px Arial`;
                context.fillStyle = color;
                context.fillText(text, x, y);

                resolve(canvas.toDataURL());
            };
            img.src = imageDataUrl;
        });
    }
};
