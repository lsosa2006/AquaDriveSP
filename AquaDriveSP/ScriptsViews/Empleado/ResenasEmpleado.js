document.addEventListener("DOMContentLoaded", function () {

    // 🔹 Cargar reseñas iniciales
    cargarResenas();

    // 🔹 Botón filtrar
    document.getElementById("btnFiltrar").addEventListener("click", function () {
        cargarResenas();
    });

    // 🔹 Función para cargar reseñas
    function cargarResenas() {
        const fechaInicio = document.getElementById("fechaInicio").value;
        const fechaFin = document.getElementById("fechaFin").value;
        const puntuacion = document.getElementById("puntuacion").value;

        fetch(`/Empleado/GetResenas?fechaInicio=${fechaInicio || ""}&fechaFin=${fechaFin || ""}&puntuacion=${puntuacion || ""}`)
            .then(res => res.json())
            .then(data => {
                if (!data.success) {
                    Swal.fire("Error", data.message, "error");
                    return;
                }

                if (data.total === 0) {
                    document.getElementById("resumen").style.display = "none";
                    document.getElementById("tablaContainer").style.display = "none";
                    document.getElementById("sinDatos").style.display = "block";
                    return;
                }

                // Mostrar resumen
                document.getElementById("lblPromedio").innerText = data.promedio.toFixed(1);
                document.getElementById("lblTotal").innerText = data.total;
                document.getElementById("resumen").style.display = "flex";

                // Construir filas de la tabla
                const tbody = document.getElementById("tablaResenas");
                tbody.innerHTML = "";

                data.reseñas.forEach(r => {
                    const tr = document.createElement("tr");

                    const estrellas = generarEstrellas(r.Puntuacion);

                    tr.innerHTML = `
                        <td>${r.Cliente}</td>
                        <td>${r.Placa}</td>
                        <td>${r.Fecha}</td>
                        <td>${estrellas}</td>
                        <td>${r.Comentario}</td>
                    `;
                    tbody.appendChild(tr);
                });

                document.getElementById("tablaContainer").style.display = "block";
                document.getElementById("sinDatos").style.display = "none";
            })
            .catch(() => Swal.fire("Error", "No se pudieron obtener las reseñas.", "error"));
    }

    // 🔹 Generar estrellas visuales
    function generarEstrellas(puntaje) {
        let html = "";
        const fullStars = Math.floor(puntaje); // número entero de estrellas llenas
        const emptyStars = 5 - fullStars;      // resto hasta completar 5

        // Estrellas llenas
        for (let i = 0; i < fullStars; i++) {
            html += '<i class="bi bi-star-fill text-warning"></i>';
        }

        // Estrellas vacías
        for (let i = 0; i < emptyStars; i++) {
            html += '<i class="bi bi-star text-warning"></i>';
        }

        return html;
    }

});
