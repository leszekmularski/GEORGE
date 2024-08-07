function focusElement(element) {
    element.focus();
}

function isDevice() {
    return /android|webos|iphone|ipad|ipod|blackberry|iemobile|opera mini|mobile/i.test(navigator.userAgent);
}

//jQuery.noConflict();
//jQuery(document).ready(function ($) {
//    $('[data-toggle="tooltip"]').tooltip();
//});


function autoExpand(field) {
    // Oblicz szerokoœæ tekstu wpisanego w pole <input>
    var textWidth = field.scrollWidth;
    // Ustaw szerokoœæ pola <input> na podstawie szerokoœci tekstu
    field.style.width = textWidth + "px";
}

function printTable() {
    var tableHtml = document.querySelector("table").outerHTML;
    var printWindow = window.open('', '', 'width=600,height=600');
    printWindow.document.open();
    printWindow.document.write('<html><head><title>Print Table</title></head><body>');
    printWindow.document.write(tableHtml);
    printWindow.document.write('</body></html>');
    printWindow.document.close();
    printWindow.print();
}

function getTableHtml(containerId) {
    var container = document.getElementById(containerId);
    var tableHtml = container.innerHTML;
    return tableHtml;
}

async function downloadFileFromStream(fileName, contentStreamReference) {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName || '';
    anchorElement.click();
    anchorElement.remove();
    URL.revokeObjectURL(url);
}

function saveByteArray(byteArray, fileName) {
    var blob = new Blob([byteArray], { type: "application/octet-stream" });
    var url = URL.createObjectURL(blob);
    var a = document.createElement("a");
    document.body.appendChild(a);
    a.style.display = "none";
    a.href = url;
    a.download = fileName;
    a.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
}

window.browser = {
    getUserAgent: function () {
        return navigator.userAgent;
    }
};

window.getSessionStorageItem = function (key) {
    var value = sessionStorage.getItem(key);
    console.log("Wartoœæ " + key + " w sessionStorage:", value);
    return value;
};
