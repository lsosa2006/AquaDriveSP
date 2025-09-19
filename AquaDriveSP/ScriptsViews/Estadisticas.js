document.querySelector('.btn-blue').addEventListener('click', function () {
    const fechaInicio = document.getElementById('fecha-inicio').value;
    const fechaFin = document.getElementById('fecha-fin').value;

    if (!fechaInicio || !fechaFin) {
        alert("Selecciona rango de fechas");
        return;
    }

    fetch(`/Admin/GetEstadisticas?fechaInicio=${fechaInicio}&fechaFin=${fechaFin}`)
        .then(res => res.json())
        .then(data => {
            // --- Chart 1: autos lavados por empleado ---
            if (window.chart1) window.chart1.destroy();
            const ctx1 = document.getElementById("chart1").getContext("2d");
            const maxEmpleado = Math.max(...data.AutosPorEmpleado.map(x => x.Cantidad), 0);
            window.chart1 = new Chart(ctx1, {
                type: 'bar',
                data: {
                    labels: data.AutosPorEmpleado.map(x => x.Nombre),
                    datasets: [{
                        label: "Autos lavados",
                        data: data.AutosPorEmpleado.map(x => x.Cantidad),
                        backgroundColor: '#00cfff'
                    }]
                },
                options: {
                    responsive: true,
                    plugins: { legend: { display: false } },
                    scales: {
                        y: {
                            beginAtZero: true,
                            max: Math.ceil(maxEmpleado * 1.1) // +10% para margen
                        }
                    }
                }
            });

            // --- Chart 2: servicios por sede ---
            if (window.chart2) window.chart2.destroy();
            const ctx2 = document.getElementById("chart2").getContext("2d");
            const maxSede = Math.max(...data.ServiciosPorSede.map(x => x.Cantidad), 0);
            window.chart2 = new Chart(ctx2, {
                type: 'bar',
                data: {
                    labels: data.ServiciosPorSede.map(x => x.nombre),
                    datasets: [{
                        label: "Servicios por sede",
                        data: data.ServiciosPorSede.map(x => x.Cantidad),
                        backgroundColor: '#00cfff'
                    }]
                },
                options: {
                    responsive: true,
                    plugins: { legend: { display: false } },
                    scales: {
                        y: {
                            beginAtZero: true,
                            max: Math.ceil(maxSede * 1.1) // +10% margen
                        }
                    }
                }
            });

            // --- Tabla dinámica: ingresos por tipo de servicio ---
            const tbody = document.querySelector(".admin-container table tbody");
            tbody.innerHTML = "";
            data.IngresosPorServicio.forEach(s => {
                tbody.innerHTML += `
                    <tr>
                        <td>${s.nombre}</td>
                        <td>${s.Precio.toLocaleString('es-CO')}</td>
                        <td>${s.Cantidad}</td>
                        <td>${s.Total.toLocaleString('es-CO')}</td>
                    </tr>
                `;
            });
        })
        .catch(err => console.error(err));
});
