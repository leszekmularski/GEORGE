// Inicjalizacja pustej tablicy ganttData
var ganttData = [];
var gantt;
var tasks;
var options;
// Funkcja do inicjalizacji wykresu Gantt
window.initializeFrappeGanttFromJson = function (selector, tasksJson, optionsJson, dotNetHelper) {
    tasks = JSON.parse(tasksJson);
    options = JSON.parse(optionsJson);
   
    gantt = new Gantt(selector, tasks, options, {
        on_click: function (task) {
            var taskIndex = tasks.findIndex(t => t.id === task.id);
            console.log("Clicked task:", task);
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync('OnClick', taskIndex);
                dotNetHelper = dotNetHelper;
            }
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

    gantt.set_scroll_position(Date.now());

    // Add event listener for each bar in the Gantt chart
    setTimeout(() => {
        document.querySelectorAll(".bar-wrapper").forEach((bar, index) => {
            bar.setAttribute("data-task-id", index);
            bar.addEventListener("click", function (event) {
                event.stopPropagation(); // Prevent the SVG click event
                dotNetHelper.invokeMethodAsync('OnClick', index);
            });
        });
    }, 500); // Adjust the timeout as needed to ensure the bars are rendered
};

// Poczekaj na załadowanie danych, a następnie wywołaj metodę SaveGanttData()
setTimeout(function () {
    SaveGanttData(ganttData);
}, 1000); // Przykładowy czas oczekiwania 1000ms (1s)

// Funkcja do przekazania danych do funkcji po stronie C#
window.SaveGanttData = function () {
    return ganttData; // Odwołanie do globalnej zmiennej bez użycia `this`
};


// Funkcja do zmiany widoku w wykresie Gantt
function ChangeView(viewMode, dotNetHelper) {
    // Wyświetlamy wartość kliknięcia w konsoli
    // Zmieniamy widok w zależności od przekazanego trybu widoku

    //console.log(viewMode);
    gantt.change_view_mode(viewMode);
    gantt.set_scroll_position(Date.now());

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
}

function refreshGanttData(dotNetHelper) {
    // Wyświetlamy wartość kliknięcia w konsoli
    // Zmieniamy widok w zależności od przekazanego trybu widoku
    //if (window.gantt && window.dotNetHelper) 
    if (gantt) {
        //console.log("refreshGanttData");
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
    }
    else {
        console.log("Err refreshGanttData");
    }
 
}

function on_click(taskIndex) {
    // Pobierz konkretny obiekt zadania na podstawie indeksu
    var task = tasks[taskIndex];
    // Zwróć tylko kliknięte zadanie
    return task;
}

window.addGanttClickEvent = function (selector, dotNetHelper) {
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
};
window.clearGantt = function (selector) {
    const element = document.querySelector(selector);
    if (element) {
        element.innerHTML = '';
        console.log("clearGantt: element cleared");
    } else {
        console.error("clearGantt: element not found!");
    }
};