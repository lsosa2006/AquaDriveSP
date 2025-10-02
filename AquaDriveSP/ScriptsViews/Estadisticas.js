document.addEventListener("DOMContentLoaded", function () {
    // Variables globales
    const fechaInicioInput = document.getElementById("fecha-inicio");
    const fechaFinInput = document.getElementById("fecha-fin");
    const btnBuscar = document.querySelector(".filter-row .btn-blue");
    const tbodyIngresos = document.getElementById("ingresos-table-body");

    let chart1 = null; // Autos por empleado
    let chart2 = null; // Servicios por sede

    //Obtener estadísticas desde backend
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

    // Renderizar gráfico 1: Autos lavados por empleado
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
                    borderColor: "#007bff",
                    backgroundColor: "rgba(0, 123, 255, 0.2)",
                    borderWidth: 2
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: { display: true },
                    title: {
                        display: true,
                        text: "Autos lavados por empleado",
                        color: "#333",
                        font: {
                            size: 18,
                            weight: "bold"
                        },
                        padding: { top: 10, bottom: 20 }
                    }
                },
                scales: {
                    x: {
                        title: { display: true, text: "Empleados" }
                    },
                    y: {
                        beginAtZero: true,
                        title: { display: true, text: "Cantidad de autos" }
                    }
                }
            }
        });
    }

    // Renderizar gráfico 2: Servicios por sede
    function renderChart2(datos) {
        const ctx = document.getElementById("chart2").getContext("2d");
        if (chart2) chart2.destroy();

        chart2 = new Chart(ctx, {
            type: "bar",
            data: {
                labels: datos.map(x => x.nombre), // sedes
                datasets: [{
                    label: "Servicios realizados",
                    data: datos.map(x => x.Cantidad),
                    borderColor: "#28a745",
                    backgroundColor: "rgba(40, 167, 69, 0.2)",
                    borderWidth: 2
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: { display: true },
                    title: {
                        display: true,
                        text: "Cantidad de servicios por sede",
                        color: "#333",
                        font: {
                            size: 18,
                            weight: "bold"
                        },
                        padding: { top: 10, bottom: 20 }
                    }
                },
                scales: {
                    x: {
                        title: { display: true, text: "Sedes" }
                    },
                    y: {
                        beginAtZero: true,
                        title: { display: true, text: "Número de servicios" }
                    }
                }
            }
        });
    }

    // Renderizar tabla de ingresos
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

    // Eventos
    btnBuscar.addEventListener("click", cargarEstadisticas);

    // Inicialización
    // Auto-cargar con fechas del mes actual
    const hoy = new Date();
    const inicioMes = new Date(hoy.getFullYear(), hoy.getMonth(), 1);

    fechaInicioInput.value = inicioMes.toISOString().split("T")[0];
    fechaFinInput.value = hoy.toISOString().split("T")[0];

    cargarEstadisticas();
});


