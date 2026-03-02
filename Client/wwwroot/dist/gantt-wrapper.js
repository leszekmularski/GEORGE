// gantt-wrapper.js
let ganttChart = null;

// Eksportuj funkcje do globalnego zakresu
window.ganttZoomIn = function () {
    if (!ganttChart) return;
    const currentMode = ganttChart.options.view_mode;
    const modes = ['Quarter Day', 'Half Day', 'Day', 'Week', 'Month'];
    const currentIndex = modes.indexOf(currentMode);
    if (currentIndex > 0) {
        ganttChart.change_view_mode(modes[currentIndex - 1]);
    }
};

window.ganttZoomOut = function () {
    if (!ganttChart) return;
    const currentMode = ganttChart.options.view_mode;
    const modes = ['Quarter Day', 'Half Day', 'Day', 'Week', 'Month'];
    const currentIndex = modes.indexOf(currentMode);
    if (currentIndex < modes.length - 1) {
        ganttChart.change_view_mode(modes[currentIndex + 1]);
    }
};

window.scrollGantt = function (x, y) {
    const container = document.querySelector('#gantt-horizontal-container');
    if (container) {
        container.scrollBy({ left: x, top: y, behavior: 'smooth' });
    }
};

window.scrollGanttToToday = function () {
    if (!ganttChart) return;
    const today = new Date();
    const svg = document.querySelector('.gantt-view-container');
    if (svg) {
        svg.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
};

window.clearGantt = function () {
    const container = document.getElementById('gantt');
    if (container) {
        container.innerHTML = '';
    }
    ganttChart = null;
};

window.getTaskByIndex = function (index) {
    if (!ganttChart || !ganttChart.tasks) return null;
    const tasks = ganttChart.tasks;
    if (index >= 0 && index < tasks.length) {
        return JSON.stringify(tasks[index]);
    }
    return null;
};

window.initializeFrappeGantt = function (selector, tasksJson, optionsJson, dotNetHelper) {
    try {
        // Wyczyść poprzedni wykres
        window.clearGantt();

        // Sprawdź czy biblioteka Gantt jest dostępna
        if (typeof Gantt === 'undefined') {
            console.error('Frappe Gantt library not loaded');
            throw new Error('Frappe Gantt library not loaded');
        }

        const container = document.querySelector(selector);
        if (!container) {
            console.error('Container not found:', selector);
            throw new Error('Container not found: ' + selector);
        }

        // Parse data
        const tasks = JSON.parse(tasksJson);
        const options = JSON.parse(optionsJson);

        // DODAJ NIESTANDARDOWY DYPEK (POPUP)
        options.popup = function (task) {
            try {
                // Rozbij nazwę na części (zakładając format "Nazwa - Miejscowość")
                const nameParts = task.name.split(' - ');
                const nazwaPodmiotu = nameParts[0] || task.name;
                const miejscowosc = nameParts.length > 1 ? nameParts[1] : '---';

                // Formatuj daty po polsku
                const startDate = new Date(task.start).toLocaleDateString('pl-PL', {
                    day: '2-digit',
                    month: '2-digit',
                    year: 'numeric'
                });
                const endDate = new Date(task.end).toLocaleDateString('pl-PL', {
                    day: '2-digit',
                    month: '2-digit',
                    year: 'numeric'
                });

                // Określ status i kolor na podstawie progresu
                const isCompleted = task.progress === 100;
                const statusText = isCompleted ? '✅ Zakończone' : '⏳ Planowane';
                const statusColor = isCompleted ? '#27ae60' : '#e67e22';

                // Zwróć HTML dymka
                return `
                    <div style="padding: 15px; background: white; border-radius: 10px; 
                                box-shadow: 0 6px 20px rgba(0,0,0,0.2); min-width: 280px;
                                font-family: 'Segoe UI', Arial, sans-serif;
                                border-left: 5px solid ${statusColor};
                                transition: all 0.3s ease;">
                        
                        <!-- Nagłówek z nazwą podmiotu -->
                        <div style="font-weight: 600; font-size: 15px; color: #2c3e50; 
                                    margin-bottom: 12px; border-bottom: 2px solid #f0f0f0; 
                                    padding-bottom: 8px; display: flex; align-items: center;">
                            <span style="background: ${statusColor}; width: 10px; height: 10px; 
                                        border-radius: 50%; display: inline-block; margin-right: 8px;"></span>
                            <span>${nazwaPodmiotu}</span>
                        </div>
                        
                        <!-- Szczegóły w formie tabeli -->
                        <div style="font-size: 13px; color: #34495e;">
                            <div style="display: flex; margin-bottom: 8px; align-items: flex-start;">
                                <div style="width: 90px; color: #7f8c8d; font-weight: 500;">Miejscowość:</div>
                                <div style="font-weight: 500; color: #2c3e50;">${miejscowosc}</div>
                            </div>
                            
                            <div style="display: flex; margin-bottom: 8px; align-items: flex-start;">
                                <div style="width: 90px; color: #7f8c8d; font-weight: 500;">Data rozpoczęcia:</div>
                                <div>${startDate}</div>
                            </div>
                            
                            <div style="display: flex; margin-bottom: 8px; align-items: flex-start;">
                                <div style="width: 90px; color: #7f8c8d; font-weight: 500;">Data zakończenia:</div>
                                <div>${endDate}</div>
                            </div>
                            
                            <div style="display: flex; margin-bottom: 8px; align-items: flex-start;">
                                <div style="width: 90px; color: #7f8c8d; font-weight: 500;">Status:</div>
                                <div style="color: ${statusColor}; font-weight: 600; display: flex; align-items: center;">
                                    <span style="margin-right: 5px;">${statusText}</span>
                                </div>
                            </div>
                        </div>
                        
                        <!-- Pasek postępu -->
                        <div style="margin-top: 15px;">
                            <div style="display: flex; justify-content: space-between; 
                                        font-size: 11px; color: #7f8c8d; margin-bottom: 5px;">
                                <span>Postęp realizacji:</span>
                                <span style="font-weight: 600; color: ${statusColor};">${task.progress}%</span>
                            </div>
                            <div style="background: #ecf0f1; border-radius: 20px; height: 10px; width: 100%; overflow: hidden;">
                                <div style="background: linear-gradient(90deg, ${statusColor} 0%, ${statusColor} 100%); 
                                            width: ${task.progress}%; height: 100%; 
                                            border-radius: 20px; transition: width 0.3s ease;"></div>
                            </div>
                        </div>
                        
                        <!-- ID zadania i dodatkowe informacje -->
                        <div style="margin-top: 12px; padding-top: 8px; border-top: 1px dashed #ecf0f1; 
                                    font-size: 10px; color: #95a5a6; display: flex; justify-content: space-between;">
                            <span>🆔 ${task.id || 'Brak ID'}</span>
                            <span>📊 ${task.progress}% ukończone</span>
                        </div>
                    </div>
                `;
            } catch (error) {
                console.error('Error generating popup:', error);
                // Fallback do prostszego dymka w razie błędu
                return `
                    <div style="padding: 10px; background: white; border-radius: 5px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);">
                        <strong>${task.name || 'Zadanie'}</strong><br>
                        <span>Postęp: ${task.progress || 0}%</span>
                    </div>
                `;
            }
        };

        // Ustaw wyzwalacz popupu (hover lub click)
        options.popup_trigger = "hover"; // Możesz zmienić na "click" jeśli wolisz

        // Ustaw callback dla kliknięć
        options.on_click = function (task) {
            if (dotNetHelper && task && task.id) {
                const taskIndex = tasks.findIndex(t => t.id === task.id);
                if (taskIndex >= 0) {
                    dotNetHelper.invokeMethodAsync('OnClick', taskIndex)
                        .catch(err => console.error('Error invoking .NET method:', err));
                }
            }
        };

        // Dodaj opcjonalnie callback dla podwójnego kliknięcia
        options.on_double_click = function (task) {
            console.log('Double clicked on task:', task);
            // Tutaj możesz dodać własną logikę
        };

        // Dodaj callback dla zmiany daty (jeśli potrzebujesz)
        options.on_date_change = function (task, start, end) {
            console.log('Task date changed:', task, start, end);
        };

        // Inicjalizuj wykres
        ganttChart = new Gantt(container, tasks, options);

        console.log('Gantt chart initialized successfully with custom popup');
        return true;
    } catch (error) {
        console.error('Error initializing Gantt chart:', error);
        throw error;
    }
};

// Dodaj helper do sprawdzania załadowania
window.isGanttLoaded = function () {
    return typeof Gantt !== 'undefined' && typeof window.initializeFrappeGantt !== 'undefined';
};