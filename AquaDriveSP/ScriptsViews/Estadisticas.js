// ScriptsViews/Estadisticas.js
document.addEventListener("DOMContentLoaded", function () {
    // ---------------------------
    // Variables globales
    // ---------------------------
    const fechaInicioInput = document.getElementById("fecha-inicio");
    const fechaFinInput = document.getElementById("fecha-fin");
    const btnBuscar = document.querySelector(".filter-row .btn-blue");
    const tbodyIngresos = document.getElementById("ingresos-table-body");

    let chart1 = null; // Autos por empleado
    let chart2 = null; // Servicios por sede

    // ---------------------------
    // 1. Función para obtener estadísticas desde backend
    // ---------------------------
    function cargarEstadisticas() {
        const fechaInicio = fechaInicioInput.value;
        const fechaFin = fechaFinInput.value;

        if (!fechaInicio || !fechaFin) {
            Swal.fire("Error", "Debes seleccionar una fecha de inicio y fin.", "error");
            return;
        }

        if (new Date(fechaFin) < new Date(fechaInicio)) {
            Swal.fire("Error", "La fecha inicio debe ser menor o igual a la fecha fin.", "error");
            return;
        }

        $.ajax({
            url: "/Admin/GetEstadisticas",
            method: "GET",
            data: { fechaInicio, fechaFin },
            success: function (data) {
                if (data) {
                    renderChart1(data.AutosPorEmpleado);
                    renderChart2(data.ServiciosPorSede);
                    renderTabla(data.IngresosPorServicio);
                } else {
                    Swal.fire("Error", "No se recibieron datos.", "error");
                }
            },
            error: function () {
                Swal.fire("Error", "No se pudieron cargar las estadísticas.", "error");
            }
        });
    }

    // ---------------------------
    // 2. Renderizar gráfico 1: Autos lavados por empleado
    // ---------------------------
    function renderChart1(datos) {
        const ctx = document.getElementById("chart1").getContext("2d");
        if (chart1) chart1.destroy();

        chart1 = new Chart(ctx, {
            type: "bar",
            data: {
                labels: datos.map(x => x.Nombre),
                datasets: [{
                    label: "Autos lavados",
                    data: datos.map(x => x.Cantidad),
                    backgroundColor: "#00cfff"
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: { display: false },
                    title: {
                        display: true,
                        text: "Autos lavados por empleado"
                    }
                },
                scales: {
                    x: {
                        title: {
                            display: true,
                            text: "Empleados"
                        }
                    },
                    y: {
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: "Cantidad de autos"
                        }
                    }
                }
            }
        });
    }

    // ---------------------------
    // 3. Renderizar gráfico 2: Servicios por sede (Bar Chart)
    // ---------------------------
    function renderChart2(datos) {
        const ctx = document.getElementById("chart2").getContext("2d");
        if (chart2) chart2.destroy();

        chart2 = new Chart(ctx, {
            type: "bar", 
            data: {
                labels: datos.map(x => x.nombre), // sedes
                datasets: [{
                    label: "Servicios realizados",
                    data: datos.map(x => x.Cantidad), // cantidad de servicios
                    backgroundColor: [
                        "#00cfff", "#0099cc", "#33cc33", "#ff9933",
                        "#ff3333", "#9966cc", "#ff66b2"
                    ]
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: {
                        display: false //oculta la leyenda (no necesaria en barras)
                    },
                    title: {
                        display: true,
                        text: "Cantidad de servicios por sede"
                    }
                },
                scales: {
                    x: {
                        title: {
                            display: true,
                            text: "Sedes"
                        }
                    },
                    y: {
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: "Número de servicios"
                        }
                    }
                }
            }
        });
    }

    // ---------------------------
    // 4. Renderizar tabla de ingresos
    // ---------------------------
    function renderTabla(datos) {
        tbodyIngresos.innerHTML = "";

        if (datos.length === 0) {
            tbodyIngresos.innerHTML = `
                <tr>
                    <td colspan="4" class="text-muted">Sin registros</td>
                </tr>
            `;
            return;
        }

        datos.forEach(item => {
            const tr = document.createElement("tr");
            tr.innerHTML = `
                <td>${item.nombre}</td>
                <td>$${item.Precio.toLocaleString()}</td>
                <td>${item.Cantidad}</td>
                <td>$${item.Total.toLocaleString()}</td>
            `;
            tbodyIngresos.appendChild(tr);
        });
    }

    // ---------------------------
    // 5. Eventos
    // ---------------------------
    btnBuscar.addEventListener("click", cargarEstadisticas);

    // ---------------------------
    // 6. Inicialización
    // ---------------------------
    // Auto-cargar con fechas del mes actual
    const hoy = new Date();
    const inicioMes = new Date(hoy.getFullYear(), hoy.getMonth(), 1);

    fechaInicioInput.value = inicioMes.toISOString().split("T")[0];
    fechaFinInput.value = hoy.toISOString().split("T")[0];

    cargarEstadisticas();
});


