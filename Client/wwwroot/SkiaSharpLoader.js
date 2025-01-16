// wwwroot/SkiaSharpLoader.js
if (!globalThis.Module) {
    globalThis.Module = {};
}
globalThis.Module.locateFile = (file) => {
    return `_content/SkiaSharp.NativeAssets.WebAssembly/${file}`;
};
