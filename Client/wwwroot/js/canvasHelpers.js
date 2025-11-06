// ===============================
// 📜 Blazor Canvas Helper Functions
// ===============================

// Interceptuje wywołania .NET z JS
const _originalInvoke = DotNet.invokeMethodAsync;
DotNet.invokeMethodAsync = function (assemblyName, methodName, ...args) {
    console.log(`🔍 DotNet.invokeMethodAsync intercepted: ${assemblyName}.${methodName}`, args);
    return _originalInvoke(assemblyName, methodName, ...args)
        .catch(err => {
            console.warn("❌ Intercepted DotNet.invokeMethodAsync error:", err);
            throw err;
        });
};

// ===============================
// 🖱️ Globalne zmienne
// ===============================
window._dragListenersInitialized = window._dragListenersInitialized || false;
window.currentDraggedModel = null;
window.currentDraggedType = null;
window.isDragging = false;
window.blazorCanvas = null;

// ===============================
// 🎨 Ustawienie referencji
// ===============================
window.setCanvasRef = (canvasRef) => {
    if (!canvasRef) {
        console.error("❌ Blazor przekazał null do setCanvasRef!");
        return;
    }
    window.blazorCanvas = canvasRef;
    console.log("✅ Canvas ustawiony!", canvasRef);
};

window.setDotNetHelper = (dotNetHelper) => {
    window.dotNetHelper = dotNetHelper;
    console.log("✅ DotNetHelper ustawiony!");
};

window.clearDotNetHelper = () => {
    console.log("🧹 Czyszczenie dotNetHelper. Stary:", window.dotNetHelper);
    window.dotNetHelper = null;
};

// ===============================
// 🧱 Przeciąganie modeli
// ===============================
window.startDrag = (modelType, event) => {
    if (event.button !== 0) return; // tylko lewy przycisk myszy
    event.preventDefault();
    window.isDragging = true;
    window.currentDraggedType = modelType;

    const img = document.getElementById(`wing-${modelType}`);
    if (!img) {
        console.error(`❌ Nie znaleziono obrazka dla modelu: ${modelType}`);
        return;
    }

    const cursorImg = new Image();
    cursorImg.src = img.src;
    cursorImg.style.position = "absolute";
    cursorImg.style.pointerEvents = "none";
    cursorImg.style.width = "50px";
    cursorImg.style.height = "50px";
    cursorImg.style.opacity = "0.8";
    cursorImg.id = "dragCursorImage";
    document.body.appendChild(cursorImg);

    window.currentDraggedImage = cursorImg;
    window.updateCursorImagePosition(event);
};

window.updateCursorImagePosition = (event) => {
    if (!window.isDragging || !window.currentDraggedImage) return;
    window.currentDraggedImage.style.left = `${event.pageX - 25}px`;
    window.currentDraggedImage.style.top = `${event.pageY - 25}px`;
};

if (!window._stopDragDefined) {
    window.stopDrag = async function (event) {
        if (!window.isDragging) return;
        window.isDragging = false;

        if (window.currentDraggedImage) {
            document.body.removeChild(window.currentDraggedImage);
            window.currentDraggedImage = null;
        }

        if (!window.blazorCanvas) {
            console.error("❌ Brak referencji do canvasu!");
            return;
        }

        const rect = window.blazorCanvas.getBoundingClientRect();
        const x = event.clientX - rect.left;
        const y = event.clientY - rect.top;

        console.log(`🛑 Upuszczono model: ${window.currentDraggedType} na (${x}, ${y})`);

        if (window.dotNetHelper) {
            try {
                await window.dotNetHelper.invokeMethodAsync('OnDragEnd', window.currentDraggedType, x, y);
            } catch (err) {
                console.warn("❌ invokeMethodAsync (OnDragEnd) nie powiodło się:", err);
                window.dotNetHelper = null;
            }
        }

        window.currentDraggedType = null;
    };

    document.addEventListener("mouseup", window.stopDrag);
    window._stopDragDefined = true;
}

if (!window._dragListenersInitialized) {
    document.addEventListener("mouseup", window.stopDrag);
    document.addEventListener("mousemove", window.updateCursorImagePosition);
    window._dragListenersInitialized = true;
}

// ===============================
// 🎯 Obsługa kursora
// ===============================
window.SetCustomCursor = (base64Image) => {
    const cursorUrl = `data:image/png;base64,${base64Image}`;
    document.body.style.cursor = `url(${cursorUrl}) 8 8, auto`;
};

window.ResetCursor = () => {
    document.querySelector(".canvas-container").style.cursor = "default";
    document.querySelector("canvas").style.cursor = "default";
};

window.AddCanvasHoverListener = (canvasElement) => {
    canvasElement.addEventListener('mousemove', () => {
        if (document.body.style.cursor.includes('url')) {
            document.body.style.cursor = document.body.style.cursor;
        }
    });
};

window.ResetCursorGlobal = () => {
    document.documentElement.style.cursor = 'default';
    document.body.style.cursor = 'default';
    const container = document.querySelector('.canvas-container');
    if (container) container.style.cursor = 'default';

    const style = document.createElement('style');
    style.innerHTML = '*{cursor: default !important;}';
    document.head.appendChild(style);
    setTimeout(() => document.head.removeChild(style), 100);
};

// ===============================
// 🖼️ Obsługa tekstur
// ===============================
window.loadAndDrawTexture = async (canvasElement, imageUrl, dotNetHelper) => {
    const ctx = canvasElement.getContext('2d');
    const img = new Image();
    img.crossOrigin = "anonymous";
    img.src = imageUrl;

    await new Promise((resolve, reject) => {
        img.onload = () => resolve();
        img.onerror = reject;
    });

    const pattern = ctx.createPattern(img, 'repeat');
    ctx.fillStyle = pattern;
    ctx.fill("evenodd");
    ctx.canvas.dataset.pattern = "wood-pattern";

    if (dotNetHelper) {
        try {
            await dotNetHelper.invokeMethodAsync('OnTextureLoaded');
        } catch (err) {
            console.warn("❌ OnTextureLoaded invoke failed", err);
        }
    }
};

// ===============================
// 🪟 Pomocnicze
// ===============================
window.getCanvasBoundingRect = function (canvasElement) {
    if (!canvasElement) {
        console.warn("❌ Canvas element is null in JS.");
        return null;
    }

    const rect = canvasElement.getBoundingClientRect();
    const scaleX = canvasElement.width / rect.width;
    const scaleY = canvasElement.height / rect.height;

    return {
        left: rect.left,
        top: rect.top,
        width: rect.width,
        height: rect.height,
        scaleX: scaleX,
        scaleY: scaleY
    };
};

window.getElementSize = (selector) => {
    const el = document.querySelector(selector);
    if (!el) return null;

    const rect = el.getBoundingClientRect();
    return {
        width: rect.width,
        height: rect.height
    };
};

window.registerResizeCallback = (dotnetHelper) => {
    window.addEventListener("resize", () => {
        dotnetHelper.invokeMethodAsync("UpdateCanvasSizeFromResize");
    });
};

window.registerResizeHandler = (dotnetHelper) => {
    window.addEventListener("resize", () => {
        dotnetHelper.invokeMethodAsync("UpdateCanvasSizeFromResize");
    });
};

window.isCanvasValid = () => {
    const isValid = !!window.blazorCanvas && document.contains(window.blazorCanvas);
    return isValid;
};

window.clearCanvasRef = () => {
    console.log("🧹 Czyszczenie referencji do canvas");
    window.blazorCanvas = null;
};

window.SetCurrentModel = (modelType, modelRowId) => {
    window.currentDraggedModel = {
        type: modelType,
        rowId: modelRowId
    };

    document.documentElement.setAttribute('data-current-model', modelType);
    document.documentElement.setAttribute('data-model-row-id', modelRowId);
    console.log(`📦 Ustawiono model: ${modelType}, ID: ${modelRowId}`);
};

window.getDraggedModelInfo = () => {
    if (!window.currentDraggedModel) {
        console.warn("⚠️ currentDraggedModel is not set.");
        return null;
    }
    return {
        type: window.currentDraggedModel.type,
        rowId: window.currentDraggedModel.rowId
    };
};

window.clearCurrentModel = () => {
    window.currentDraggedModel = null;
    document.documentElement.removeAttribute('data-current-model');
};

// ===============================
// 🛑 Blokuj domyślne drag&drop
// ===============================
document.addEventListener('dragover', (e) => {
    e.preventDefault();
});
