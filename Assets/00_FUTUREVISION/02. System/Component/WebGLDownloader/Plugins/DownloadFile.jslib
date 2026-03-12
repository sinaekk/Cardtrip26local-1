mergeInto(LibraryManager.library, {
    DownloadFile: function (array, length, fileNamePtr) {
        var fileName = UTF8ToString(fileNamePtr);
        var bytes = new Uint8Array(Module.HEAPU8.buffer, array, length);

        var blob = new Blob([bytes], { type: "application/octet-stream" });
        var link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        link.download = fileName;

        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }
});
