document.addEventListener("DOMContentLoaded", function () {
    // ---------------------------
    // Variables globales
    // ---------------------------
    const tbody = document.querySelector(".empleado-container tbody");
    const spinnerModal = new bootstrap.Modal(document.getElementById("spinnerModal"));
    let citas = []; // Citas cargadas desde backend
    let citaIdGestion = null; // Para modal de gestionar

    // ---------------------------
    // 1. Cargar citas desde backend
    // ---------------------------
    function cargarCitas() {
        $.ajax({
            url: "/Empleado/GetCitasAsignadas",
            method: "GET",
            success: function (data) {
                if (data.success) {
                    citas = data.citas;
                    renderTabla();
                } else {
                    Swal.fire("Error", data.message, "error");
                }
            },
            error: function () {
                Swal.fire("Error", "No se pudieron cargar las citas.", "error");
            }
        });
    }

    // ---------------------------
    // 2. Renderizar tabla de citas
    // ---------------------------
    function renderTabla() {
        tbody.innerHTML = "";

        if (citas.length === 0) {
            const tr = document.createElement("tr");
            tr.innerHTML = `
                <td colspan="8" class="text-center text-muted py-3">
                    <i class="bi bi-info-circle"></i> Sin citas asignadas
                </td>
            `;
            tbody.appendChild(tr);
        }

        citas.forEach(c => {
            const tr = document.createElement("tr");
            tr.dataset.id = c.citaid;

            tr.innerHTML = `
                <td>${c.cliente}</td>
                <td>${c.vehiculo}</td>
                <td>${c.servicio}</td>
                <td>${c.sede}</td>
                <td>${c.horainicio}</td>
                <td>${c.horafin}</td>
                <td>
                    ${renderEstado(c.estado)}
                </td>
                <td class="text-center">
                    <button class="btn btn-manage btn-sm btn-gestionar" title="Gestionar" data-bs-toggle="modal" data-bs-target="#modalGestionar">
                        <i class="bi bi-gear-fill"></i>
                    </button>
                </td>
            `;
            tbody.appendChild(tr);
        });
    }

    function renderEstado(estado) {
        switch (estado) {
            case 1: return `<span class="badge bg-warning text-dark">Pendiente</span>`;
            case 2: return `<span class="badge bg-primary">En curso</span>`;
            case 3: return `<span class="badge bg-success">Finalizada</span>`;
            case 0: return `<span class="badge bg-danger">Cancelada</span>`;
            default: return `<span class="badge bg-secondary">${estado}</span>`;
        }
    }

    // ---------------------------
    // 3. Delegación de acciones de fila
    // ---------------------------
    tbody.addEventListener("click", function (e) {
        const tr = e.target.closest("tr");
        if (!tr) return;
        const id = parseInt(tr.dataset.id);

        // --- Abrir modal gestionar ---
        if (e.target.closest(".btn-gestionar")) {
            citaIdGestion = id;
            cargarDetalleCita(id);
        }
    });

    // ---------------------------
    // 4. Cargar detalle de la cita en modal
    // ---------------------------
    function cargarDetalleCita(citaId) {
        $.ajax({
            url: `/Empleado/GetDetalleCita?citaId=${citaId}`,
            method: "GET",
            success: function (data) {
                if (data.success) {
                    const c = data.cita;
                    const modalBody = document.querySelector("#modalGestionar .modal-body");

                    modalBody.innerHTML = `
                        <table class="table table-sm table-bordered text-center mb-3">
                            <tr>
                                <th>Placa</th>
                                <th>Hora Inicio</th>
                                <th>Servicio</th>
                            </tr>
                            <tr>
                                <td>${c.vehiculo}</td>
                                <td>${c.horainicio}</td>
                                <td>${c.servicio}</td>
                            </tr>
                        </table>
                        <div class="d-flex justify-content-around">
                            <button class="btn btn-success btn-iniciar"><i class="bi bi-play-circle"></i> Iniciar</button>
                            <button class="btn btn-primary btn-finalizar"><i class="bi bi-check-circle"></i> Finalizar</button>
                            <button class="btn btn-danger btn-cancelar"><i class="bi bi-x-circle"></i> Cancelar</button>
                        </div>
                    `;
                } else {
                    Swal.fire("Error", data.message, "error");
                }
            },
            error: function () {
                Swal.fire("Error", "No se pudo cargar la cita.", "error");
            }
        });
    }

    // ---------------------------
    // 5. Acciones dentro del modal
    // ---------------------------
    document.querySelector("#modalGestionar").addEventListener("click", function (e) {
        if (!citaIdGestion) return;

        // --- Iniciar ---
        if (e.target.closest(".btn-iniciar")) {
            cambiarEstadoCita(2);
        }

        // --- Finalizar ---
        if (e.target.closest(".btn-finalizar")) {
            cambiarEstadoCita(3);
        }

        // --- Cancelar ---
        if (e.target.closest(".btn-cancelar")) {
            cambiarEstadoCita(0);
        }
    });

    // ---------------------------
    // 6. Cambiar estado de la cita
    // ---------------------------
    function cambiarEstadoCita(nuevoEstado) {
        spinnerModal.show();
        $.ajax({
            url: "/Empleado/CambiarEstadoCita",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify({ citaId: citaIdGestion, estado: nuevoEstado }),
            success: function (data) {
                spinnerModal.hide();
                if (data.success) {
                    Swal.fire("Éxito", data.message, "success").then(() => {
                        bootstrap.Modal.getInstance(document.getElementById("modalGestionar")).hide();
                        cargarCitas();
                    });
                } else {
                    Swal.fire("Error", data.message, "error");
                }
            },
            error: function () {
                spinnerModal.hide();
                Swal.fire("Error", "No se pudo cambiar el estado de la cita.", "error");
            }
        });
    }

    // ---------------------------
    // Inicialización
    // ---------------------------
    cargarCitas();
});
