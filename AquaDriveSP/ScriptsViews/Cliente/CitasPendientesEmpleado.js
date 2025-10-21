document.addEventListener("DOMContentLoaded", function () {
    const tbody = document.querySelector(".table-citas tbody");
    const buscador = document.getElementById("buscador");

    function cargarCitas() {
        $.ajax({
            url: "/Cliente/GetMisCitas",
            method: "POST",
            data: { estados: [1, 2] }, // Pendiente y En curso
            success: function (data) {
                tbody.innerHTML = "";

                if (!data.success || data.citas.length === 0) {
                    tbody.innerHTML = `<tr><td colspan="7" class="text-center text-muted">No hay citas</td></tr>`;
                    return;
                }

                data.citas.forEach(cita => {
                    const timestamp = parseInt(cita.FechaHoraInicio.replace(/[^0-9]/g, ""));
                    const fechaHora = new Date(timestamp);

                    let btnCancelar = '';
                    if (cita.Estado === "Pendiente") {
                        btnCancelar = `<button class="btn btn-cancelar btn-sm btn-cancelar-cita" data-id="${cita.CitaId}">Cancelar</button>`;
                    }

                    tbody.innerHTML += `
                                    <tr>
                                        <td>${cita.Placa}</td>
                                        <td>${fechaHora.toLocaleString('es-CO')}</td>
                                        <td>${cita.Sede}</td>
                                        <td>${cita.TipoServicio}</td>
                                        <td>${cita.Empleado}</td>
                                        <td>${cita.Estado}</td>
                                        <td class="text-center">${btnCancelar}</td>
                                    </tr>
                                `;
                });
            },
            error: function () {
                Swal.fire("Error", "No se pudieron cargar las citas.", "error");
            }
        });
    }

    // Filtrado por cualquier columna
    buscador.addEventListener("input", function () {
        const filtro = this.value.toLowerCase();
        const filas = tbody.querySelectorAll("tr");

        filas.forEach(tr => {
            const textoFila = tr.innerText.toLowerCase();
            tr.style.display = textoFila.includes(filtro) ? "" : "none";
        });
    });

    // Cancelar cita
    tbody.addEventListener("click", function (e) {
        if (e.target.classList.contains("btn-cancelar-cita")) {
            const citaId = e.target.dataset.id;

            Swal.fire({
                title: "¿Cancelar cita?",
                text: "Esta acción no se puede deshacer.",
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: "Sí, cancelar",
                cancelButtonText: "No",
                customClass: {
                    confirmButton: 'btn btn-success',
                    cancelButton: 'btn btn-danger'
                },
                buttonsStyling: false
            }).then(result => {
                if (result.isConfirmed) {
                    $.ajax({
                        url: "/Cliente/CancelarCita",
                        method: "POST",
                        data: { citaId },
                        success: function (res) {
                            if (res.success) {
                                Swal.fire("Cancelada", res.mensaje, "success");
                                cargarCitas();
                            } else {
                                Swal.fire("Error", res.mensaje, "error");
                            }
                        },
                        error: function () {
                            Swal.fire("Error", "No se pudo cancelar la cita.", "error");
                        }
                    });
                }
            });
        }
    });

    // Carga inicial
    cargarCitas();
});