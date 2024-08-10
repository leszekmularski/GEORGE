var ganttData = [];
var gantt;
var tasks;
var options;

window.initializeFrappeGanttFromJson = function (selector, tasksJson, optionsJson, customPopupHtml, dotNetHelper) {
    try {
        resetHeaderPosition();

        tasks = JSON.parse(tasksJson);
        options = JSON.parse(optionsJson);

        console.log("customPopupHtml received:", customPopupHtml);

        if (typeof customPopupHtml === 'string') {
            try {
                options.custom_popup_html = eval('(' + customPopupHtml + ')');
                console.log("Custom popup HTML function successfully created.");
            } catch (parseError) {
                console.error("Error parsing customPopupHtml function:", parseError);
                throw parseError;
            }
        }

        if (typeof Gantt !== 'undefined') {
            gantt = new Gantt(selector, tasks, options, {
                on_click: function (task) {
                    var taskIndex = tasks.findIndex(t => t.id === task.id);
                    console.log("Clicked task:", task);
                    if (dotNetHelper) {
                        dotNetHelper.invokeMethodAsync('OnClick', taskIndex);
                    }
                    // Debugowanie: Wywołanie custom_popup_html
                    var popupContent = options.custom_popup_html(task);
                    console.log("Popup content generated:", popupContent);
                },
                on_date_change: function (task, start, end) {
                    console.log(task, start, end);
                },
                on_progress_change: function (task, progress) {
                    console.log(task, progress);
                },
                on_view_change: function (mode) {
                    console.log(mode);
                }
            });

            ganttData = gantt.tasks;

            if (typeof gantt.set_scroll_position === 'function') {
                gantt.set_scroll_position(Date.now());
            } else {
                console.error("set_scroll_position function is not defined");
            }

            // Adding event listener for each bar in the Gantt chart
            setTimeout(() => {
                document.querySelectorAll(".bar-wrapper").forEach((bar, index) => {
                    bar.setAttribute("data-task-id", index);
                    bar.addEventListener("click", function (event) {
                        event.stopPropagation(); // Prevents the SVG click event
                        dotNetHelper.invokeMethodAsync('OnClick', index);
                    });
                });
            }, 500); // Adjust the timeout as needed to ensure the bars are rendered
        } else {
            console.error("Gantt is not defined");
        }
    } catch (e) {
        console.error("Error initializing Gantt chart:", e);
    }
};


// Function to save Gantt data
window.SaveGanttData = function () {
    return ganttData;
};

// Function to change view mode
function ChangeView(viewMode, dotNetHelper) {
    try {
        if (gantt) {

            resetHeaderPosition();

            gantt.change_view_mode(viewMode);
            if (typeof gantt.set_scroll_position === 'function') {
                gantt.set_scroll_position(Date.now());
            } else {
                console.error("set_scroll_position function is not defined");
            }

            // Add event listener for each bar in the Gantt chart
            setTimeout(() => {
                document.querySelectorAll(".bar-wrapper").forEach((bar, index) => {
                    bar.setAttribute("data-task-id", index);
                    bar.addEventListener("click", function (event) {
                        event.stopPropagation(); // Prevent the SVG click event
                        if (dotNetHelper) {
                            dotNetHelper.invokeMethodAsync('OnClick', index);
                        }
                    });
                });
            }, 500); // Adjust the timeout as needed to ensure the bars are rendered
        } else {
            console.error("Gantt instance is not available");
        }
    } catch (e) {
        console.error("Error changing view mode:", e);
    }
}

// Function to refresh Gantt data
function refreshGanttData(dotNetHelper) {
    try {
        if (gantt) {

           gantt.refresh(tasks);

            // Add event listener for each bar in the Gantt chart
            setTimeout(() => {
                document.querySelectorAll(".bar-wrapper").forEach((bar, index) => {
                    bar.setAttribute("data-task-id", index);
                    bar.addEventListener("click", function (event) {
                        event.stopPropagation(); // Prevent the SVG click event
                        if (dotNetHelper) {
                            dotNetHelper.invokeMethodAsync('OnClick', index);
                        }
                    });
                });
            }, 500); // Adjust the timeout as needed to ensure the bars are rendered
        } else {
            console.error("Gantt instance is not available for refresh");
        }

    } catch (e) {
        console.error("Error refreshing Gantt data:", e);
    }
}

// Function to handle click event on a task
function on_click(taskIndex) {
    var task = tasks[taskIndex];
    return task;
}

// Function to add click event to Gantt chart
window.addGanttClickEvent = function (selector, dotNetHelper) {
    try {
        var svgElement = document.querySelector(selector);
        if (svgElement) {
            svgElement.addEventListener("click", function (event) {
                var taskElement = event.target.closest(".bar-wrapper");
                if (taskElement) {
                    var taskId = taskElement.getAttribute("data-task-id");
                    if (taskId !== null) {
                        console.log("Clicked task ID:", taskId);
                        dotNetHelper.invokeMethodAsync('OnClick', parseInt(taskId));
                    }
                }
            });
        } else {
            console.error("SVG element not found!");
        }
    } catch (e) {
        console.error("Error adding Gantt click event:", e);
    }
};

// Function to clear Gantt chart
window.clearGantt = function (selector) {
    try {
        var element = document.querySelector(selector);
        if (element) {
            while (element.firstChild) {
                element.removeChild(element.firstChild);
            }
            element.removeAttribute("viewBox");
            element.removeAttribute("width");
            element.removeAttribute("height");
            console.log("clearGantt: element został wyczyszczony");

            if (gantt) {
                gantt = null;
            }
        } else {
            console.error("clearGantt: nie znaleziono elementu!");
        }
    } catch (e) {
        console.error("Błąd podczas czyszczenia wykresu Gantta:", e);
    }
};

function removeGanttEventListeners() {
    document.querySelectorAll(".bar-wrapper").forEach((bar) => {
        var newBar = bar.cloneNode(true); // Tworzenie kopii elementu bez nasłuchiwania
        bar.parentNode.replaceChild(newBar, bar); // Zastąpienie starego elementu nowym
    });
}

function resetHeaderPosition() {
    const headers = document.querySelectorAll('.gantt-container .grid-header, .gantt-container .upper-header, .gantt-container .lower-header');
    headers.forEach(header => {
        header.style.top = '0';
    });
}

window.createNewGanttSVG = function (selector) {
    try {
        var container = document.querySelector(selector);
        if (container) {
            var svg = document.createElementNS("http://www.w3.org/2000/svg", "svg");
            svg.setAttribute("id", "gantt");
            svg.style.overflow = "scroll";
            container.appendChild(svg);
            console.log("createNewGanttSVG: nowy element SVG został dodany");
        } else {
            console.error("createNewGanttSVG: nie znaleziono kontenera!");
        }
    } catch (e) {
        console.error("Błąd podczas tworzenia nowego elementu SVG:", e);
    }
};

//document.querySelector('.viewmode-select').addEventListener('change', function () {
//    initializeGantt();
//});

//document.addEventListener('DOMContentLoaded', function () {
//    var viewModeSelect = document.querySelector('.viewmode-select');
//    if (viewModeSelect) {
//        viewModeSelect.addEventListener('change', function () {
//            initializeGantt();
//        });
//    } else {
//        console.error('Element .viewmode-select not found');
//    }
//});
