document.addEventListener("DOMContentLoaded", function () {
    const fechaInicioInput = document.getElementById("fecha-inicio");
    const fechaFinInput = document.getElementById("fecha-fin");
    const btnFiltrar = document.getElementById("btnfiltrar");
    const totalCitasEl = document.getElementById("total-citas");

    let chartCitas = null;
    let chartServicios = null;

    function cargarDesempeno() {
        const fechaInicio = fechaInicioInput.value;
        const fechaFin = fechaFinInput.value;


        if (!fechaInicio || !fechaFin) {
            Swal.fire("Error", "Debes seleccionar un rango de fechas.", "error");
            return;
        }

        $.ajax({
            url: "/Empleado/GetDesempeno",
            method: "GET",
            data: { fechaInicio, fechaFin },
            success: function (data) {
                if (!data) {
                    Swal.fire("Sin datos", "No se encontraron registros en ese rango de fechas.", "info");
                    return;
                }

                // Datos resumen
                totalCitasEl.textContent = data.totalCitas;

                // Gráficos
                renderChartCitas(data.citasPorFecha);
                renderChartServicios(data.servicios);
            },
            error: function () {
                Swal.fire("Error", "No se pudieron cargar los datos de desempeño.", "error");
            }
        });
    }

    function renderChartCitas(datos) {
        if (!Array.isArray(datos) || datos.length === 0) {
            console.warn("No hay datos para el gráfico de citas", datos);
            return;
        }

        const ctx = document.getElementById("chartCitas").getContext("2d");
        if (chartCitas) chartCitas.destroy();

        chartCitas = new Chart(ctx, {
            type: "bar",
            data: {
                labels: datos.map(x => {
                    const timestamp = parseInt(x.Fecha.match(/\d+/)[0]);
                    const fecha = new Date(timestamp);
                    return fecha.toLocaleDateString("es-ES");
                }),

                datasets: [{
                    label: "Citas Atendidas",
                    data: datos.map(x => x.Total),
                    backgroundColor: "rgba(0, 123, 255, 0.2)",
                    borderColor: "#007bff",
                    borderWidth: 2
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    title: {
                        display: true,
                        text: "Citas atendidas por fecha",
                        font: { size: 18, weight: "bold" }
                    }
                },
                scales: {
                    y: { beginAtZero: true }
                }
            }
        });
    }

    function renderChartServicios(datos) {
        if (!Array.isArray(datos) || datos.length === 0) {
            console.warn("No hay datos para el gráfico de servicios", datos);
            return;
        }

        const ctx = document.getElementById("chartServicios").getContext("2d");
        if (chartServicios) chartServicios.destroy();

        chartServicios = new Chart(ctx, {
            type: "pie",
            data: {
                labels: datos.map(x => x.TipoServicio),
                datasets: [{
                    data: datos.map(x => x.Total),
                    backgroundColor: [
                        "rgba(0, 123, 255, 0.4)",
                        "rgba(40, 167, 69, 0.4)",
                        "rgba(255, 193, 7, 0.4)",
                        "rgba(220, 53, 69, 0.4)",
                        "rgba(23, 162, 184, 0.4)"
                    ],
                    borderColor: "#fff",
                    borderWidth: 2
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    title: {
                        display: true,
                        text: "Distribución por tipo de servicio",
                        font: { size: 18, weight: "bold" }
                    }
                }
            }
        });
    }


    btnFiltrar.addEventListener("click", cargarDesempeno);

    // Inicialización automática
    const hoy = new Date();
    const inicioMes = new Date(hoy.getFullYear(), hoy.getMonth(), 1);
    fechaInicioInput.value = inicioMes.toISOString().split("T")[0];
    fechaFinInput.value = hoy.toISOString().split("T")[0];
    cargarDesempeno();
});
