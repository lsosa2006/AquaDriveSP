document.addEventListener("DOMContentLoaded", function () {
    // ---------------------------
    // Variables globales
    // ---------------------------
    const tbody = document.querySelector(".admin-container tbody");
    const btnGuardar = document.querySelector(".admin-container .btn-success");
    const spinnerModal = new bootstrap.Modal(document.getElementById("spinnerModal"));
    let sedes = []; // Lista de sedes desde backend
    let empleados = []; // Cache de empleados
    let empleadoIdHorario = null; // Empleado actualmente editando horario

    // ---------------------------
    // 1. Cargar sedes desde backend
    // ---------------------------
    function cargarSedes() {
        $.ajax({
            url: "/Admin/GetSedes",
            method: "GET",
            success: function (data) {
                if (data.success) {
                    sedes = data.sedes;
                } else {
                    Swal.fire("Error", data.message, "error");
                }
            },
            error: function () {
                Swal.fire("Error", "No se pudieron cargar las sedes.", "error");
            }
        });
    }

    // ---------------------------
    // 2. Cargar empleados desde backend
    // ---------------------------
    function cargarEmpleados() {
        $.ajax({
            url: "/Admin/GetEmpleados",
            method: "GET",
            success: function (data) {
                if (data.success) {
                    empleados = data.empleados;
                    renderTabla();
                } else {
                    Swal.fire("Error", data.message, "error");
                }
            },
            error: function () {
                Swal.fire("Error", "No se pudieron cargar los empleados.", "error");
            }
        });
    }

    // ---------------------------
    // 3. Renderizar tabla de empleados
    // ---------------------------
    function renderTabla() {
        tbody.innerHTML = "";

        if (empleados.length === 0) {
            const tr = document.createElement("tr");
            tr.innerHTML = `
                <td colspan="10" class="text-center text-muted py-3">
                    <i class="bi bi-info-circle"></i> Sin registros
                </td>
            `;
            tbody.appendChild(tr);
        }
        empleados.forEach(emp => {
            const tr = document.createElement("tr");

            // 🔹 Normalizar la fecha
            let fechaContrato = "";
            if (emp.fechacontratacion) {
                const fecha = new Date(emp.fechacontratacion);
                if (!isNaN(fecha)) {
                    fechaContrato = fecha.toISOString().split("T")[0];
                }
            }

            tr.dataset.id = emp.empleadoid;
            tr.innerHTML = `
            <td>${emp.usuarioid}</td>
            <td>${emp.usuario.nombre}</td>
            <td>${emp.usuario.apellido}</td>
            <td>${emp.usuario.email}</td>
            <td>${emp.usuario.telefono}</td>
            <td>${emp.usuario.contrasena || ""}</td>
            <td>
                <select class="form-select">
                    ${sedes.map(s =>
                    `<option value="${s.sedeid}" ${s.sedeid === emp.sedeid ? "selected" : ""}>
                            ${s.nombre}
                        </option>`
                ).join("")}
                </select>
            </td>
            <td>
                <input type="date" class="form-control" value="${fechaContrato}">
            </td>
            <td>
                <label class="switch">
                    <input type="checkbox" ${emp.estado === "Aceptado" ? "checked" : ""}>
                    <span class="slider"></span>
                </label>
            </td>
            <td>
                <div class="action-buttons">
                    <button class="btn-blue btn-sm btn-horario" title="Asignar horario">
                        <i class="bi bi-clock"></i>
                    </button>
                    <button class="btn btn-danger btn-sm btn-eliminar" title="Eliminar">
                        <i class="bi bi-x-lg"></i>
                    </button>
                </div>
            </td>
        `;
            tbody.appendChild(tr);
        });
    }

    // ---------------------------
    // 4. Guardar cambios (bulk update)
    // ---------------------------
    btnGuardar.addEventListener("click", function () {
        if (empleados.length === 0) {
            Swal.fire("Error", "Sin registros para actualizar", "error");
            return;
        }
        const filas = tbody.querySelectorAll("tr");
        const empleadosActualizados = [];
        let valido = true;
        let c = 0;

        filas.forEach(tr => {
            const id = parseInt(tr.dataset.id);
            const sedeid = parseInt(tr.querySelector("select").value);
            const fecha = tr.querySelector("input[type='date']").value;
            const estado = tr.querySelector("input[type='checkbox']").checked ? 1 : 0;
            c = c + 1;
            // Validación básica
            if (estado === 1 && (isNaN(sedeid) || sedeid === 0)) {
                valido = false;
                return;
            }

            empleadosActualizados.push({
                empleadoid: id,
                sedeid: sedeid || 0,
                fechacontratacion: fecha || null,
                estado
            });
        });

        if (!valido) {
            Swal.fire("Error", `Un empleado aceptado debe tener una sede asignada, fila: ${c}.`, "error");
            return;
        }
        spinnerModal.show();
        // AJAX POST
        $.ajax({
            url: "/Admin/BulkUpdateEmpleados",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify(empleadosActualizados),
            success: function (data) {
                if (data.success) {
                    Swal.fire("Éxito", data.message, "success").then(() => {
                        spinnerModal.hide();
                        cargarSedes();
                        cargarEmpleados();
                    });
                } else {
                    Swal.fire("Error", data.message, "error").then(() => {
                        spinnerModal.hide();
                    });
                }
            },
            error: function () {
                Swal.fire("Error", "No se pudieron guardar los cambios.", "error").then(() => {
                    spinnerModal.hide();
                });;
            }
        });
    });

    // ---------------------------
    // 5. Delegación de acciones de fila
    // ---------------------------
    tbody.addEventListener("click", function (e) {
        const tr = e.target.closest("tr");
        if (!tr) return;
        const id = parseInt(tr.dataset.id);

        // --- Eliminar empleado ---
        if (e.target.closest(".btn-eliminar")) {
            Swal.fire({
                title: "¿Eliminar?",
                text: "Esta acción no se puede deshacer.",
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: "Sí, eliminar",
                cancelButtonText: "Cancelar",
                customClass: {
                    confirmButton: 'btn btn-success', // verde
                    cancelButton: 'btn btn-danger'    // rojo
                },
                buttonsStyling: false // necesario para que tome las clases de Bootstrap
            }).then(result => {
                if (result.isConfirmed) {
                    spinnerModal.show();
                    $.ajax({
                        url: "/Admin/DeleteEmpleado",
                        method: "POST",
                        data: { id },
                        success: function (data) {
                            if (data.success) {
                                Swal.fire("Eliminado", "Empleado eliminado.", "success").then(() => {
                                    spinnerModal.hide();
                                    cargarSedes();
                                    cargarEmpleados();
                                });
                            } else {
                                Swal.fire("Error", data.message, "error").then(() => {
                                    spinnerModal.hide();
                                });
                            }
                        }
                    });
                }
            });
        }

        // --- Asignar horario ---
        if (e.target.closest(".btn-horario")) {
            empleadoIdHorario = id;
            cargarHorario(id);
            new bootstrap.Modal(document.getElementById("modalHorario")).show();
        }
    });

    // ---------------------------
    // 6. Cargar horarios en modal
    // ---------------------------
    function cargarHorario(empleadoId) {
        $.ajax({
            url: `/Admin/GetHorario?empleadoId=${empleadoId}`,
            method: "GET",
            success: function (data) {
                if (data.success) {
                    const tbody = document.querySelector("#modalHorario tbody");
                    tbody.innerHTML = ""; // limpiar tabla antes de llenar

                    const dias = [
                        "Lunes", "Martes", "Miércoles",
                        "Jueves", "Viernes", "Sábado", "Domingo"
                    ];

                    for (let i = 0; i < 7; i++) {
                        const h = data.horarios.find(x => x.diasemana === i);

                        const tr = document.createElement("tr");
                        tr.dataset.diasemana = i;

                        tr.innerHTML = `
                        <td>${dias[i]}</td>
                        <td><input type="time" class="hora-inicio form-control" value="${h ? h.horainicio : ""}"></td>
                        <td><input type="time" class="hora-fin form-control" value="${h ? h.horafin : ""}"></td>
                        <td class="text-center">
                        <div class="form-check form-switch d-flex justify-content-center">
                            <input class="form-check-input estado-switch" type="checkbox" ${h && h.estado === 1 ? "checked" : ""}>
                        </div>
                        </td>
                        `;

                        tbody.appendChild(tr);
                    }
                } else {
                    Swal.fire("Error", data.message, "error");
                }
            },
            error: function () {
                Swal.fire("Error", "No se pudo cargar el horario.", "error");
            }
        });
    }

    // ---------------------------
    // 7. Guardar horarios desde modal
    // ---------------------------
    document.querySelector("#modalHorario .btn-primary").addEventListener("click", function () {
        const filas = document.querySelectorAll("#modalHorario tbody tr");
        const horarios = [];
        let valido = true;

        filas.forEach(fila => {
            const activo = fila.querySelector("input[type='checkbox']").checked ? 1 : 0;
            const hInicio = fila.querySelector(".hora-inicio").value;
            const hFin = fila.querySelector(".hora-fin").value;
            const nombresDias = ["Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo"];
            const dia = parseInt(fila.dataset.diasemana);

            // Validar que todas las filas tengan hora asignada
            if (!hInicio || !hFin) {
                Swal.fire("Error", `Debes asignar hora de inicio y fin en el día ${nombresDias[dia]}.`, "error");
                valido = false;
                return; // corta solo esta fila
            }

            // Validar que hora inicio < hora fin si está activo
            if (hInicio >= hFin) {
                Swal.fire("Error", `La hora de inicio debe ser menor que la hora fin en el día ${nombresDias[dia]}.`, "error");
                valido = false;
                return;
            }

            horarios.push({
                diasemana: dia,
                horainicio: hInicio,
                horafin: hFin,
                estado: activo
            });
        });
        if (!valido) return; 
        spinnerModal.show();
        $.ajax({
            url: "/Admin/SaveHorario",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify({ empleadoId: empleadoIdHorario, horarios }),
            success: function (data) {
                if (data.success) {
                    Swal.fire("Éxito", data.message, "success").then(() => {
                        spinnerModal.hide();
                        bootstrap.Modal.getInstance(document.getElementById("modalHorario")).hide();
                    });
                } else {
                    Swal.fire("Error", data.message, "error").then(() => {
                        spinnerModal.hide();
                    });;
                }
            },
            error: function () {
                Swal.fire("Error", "No se pudieron guardar los horarios.", "error").then(() => {
                    spinnerModal.hide();
                });;
            }
        });
    });


    // ---------------------------
    // Inicialización
    // ---------------------------
    cargarSedes();
    cargarEmpleados();

    // ---------------------------
    // 8. Buscador en tabla
    // ---------------------------
    document.getElementById("buscador").addEventListener("keyup", function () {
        const filtro = this.value.toLowerCase();
        const filas = tbody.querySelectorAll("tr");

        filas.forEach(fila => {
            const textoFila = fila.innerText.toLowerCase();
            fila.style.display = textoFila.includes(filtro) ? "" : "none";
        });
    });
});
