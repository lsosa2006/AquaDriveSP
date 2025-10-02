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

    // Cargar sedes desde backend
    function cargarSedes() {
        $.ajax({
            url: "/Admin/GetSedes",
            method: "GET",
            success: function (data) {
                if (data.success) {
                    sedes = data.sedes;
                    cargarEmpleados();
                } else {
                    Swal.fire("Error", data.message, "error");
                }
            },
            error: function () {
                Swal.fire("Error", "No se pudieron cargar las sedes.", "error");
            }
        });
    }

    // Cargar empleados desde backend
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

    // Renderizar tabla de empleados
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

            // Normalizar la fecha
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
                    <input type="checkbox" ${emp.estado === 1 ? "checked" : ""}>
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

    // Guardar cambios
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

    // Delegación de acciones de fila
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
                    confirmButton: 'btn btn-success',
                    cancelButton: 'btn btn-danger'
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

        // Asignar horario individualmente
        if (e.target.closest(".btn-horario")) {
            empleadoIdHorario = id;
            cargarHorario(id);
            new bootstrap.Modal(document.getElementById("modalHorario")).show();
        }
    });

    // Cargar horario en modal
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

    // Guardar horario desde modal a un solo empleado
    document.querySelector("#modalHorario .btn-primary").addEventListener("click", function () {
        const filas = document.querySelectorAll("#modalHorario tbody tr");
        const horarios = [];
        let valido = true;
        const nombresDias = ["Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo"];

        for (const fila of filas) {
            const activo = fila.querySelector("input[type='checkbox']").checked ? 1 : 0;
            const hInicio = fila.querySelector(".hora-inicio").value;
            const hFin = fila.querySelector(".hora-fin").value;
            const dia = parseInt(fila.dataset.diasemana);

            if (!hInicio || !hFin) {
                Swal.fire("Error", `Debes asignar hora de inicio y fin en el día ${nombresDias[dia]}.`, "error");
                valido = false;
                break;
            }

            if (hInicio >= hFin) {
                Swal.fire("Error", `La hora de inicio debe ser menor que la hora fin en el día ${nombresDias[dia]}.`, "error");
                valido = false;
                break;
            }

            horarios.push({
                diasemana: dia,
                horainicio: hInicio,
                horafin: hFin,
                estado: activo
            });
        }
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

    // Buscador en tabla
    document.getElementById("buscador").addEventListener("keyup", function () {
        const filtro = this.value.toLowerCase();
        const filas = tbody.querySelectorAll("tr");

        filas.forEach(fila => {
            const textoFila = fila.innerText.toLowerCase();
            fila.style.display = textoFila.includes(filtro) ? "" : "none";
        });
    });

    // Renderizar tabla de horarios de todos los empleados
    function renderHorarioTodos() {
        const tbody = document.querySelector("#modalHorarioTodos tbody");
        tbody.innerHTML = ""; // limpiar antes de renderizar

        const dias = ["Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo"];

        for (let i = 0; i < dias.length; i++) {
            const tr = document.createElement("tr");
            tr.dataset.diasemana = i;

            tr.innerHTML = `
            <td>${dias[i]}</td>
            <td><input type="time" class="hora-inicio form-control"></td>
            <td><input type="time" class="hora-fin form-control"></td>
            <td class="text-center">
                <div class="form-check form-switch d-flex justify-content-center">
                    <input class="form-check-input estado-switch" type="checkbox">
                </div>
            </td>
        `;

            tbody.appendChild(tr);
        }
    }

    // Abrir modal de horario de todos los empleados
    document.getElementById("btnHorarioTodos").addEventListener("click", function () {
        renderHorarioTodos();
    });

    // Guardar horarios de todos los empleados
    document.querySelector("#modalHorarioTodos .btn-primary").addEventListener("click", function () {
        const filas = document.querySelectorAll("#modalHorarioTodos tbody tr");
        const horarios = [];
        let valido = true;
        const nombresDias = ["Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo"];

        for (const fila of filas) {
            const activo = fila.querySelector("input[type='checkbox']").checked ? 1 : 0;
            const hInicio = fila.querySelector(".hora-inicio").value;
            const hFin = fila.querySelector(".hora-fin").value;
            const dia = parseInt(fila.dataset.diasemana);

            if (!hInicio || !hFin) {
                Swal.fire("Error", `Debes asignar hora de inicio y fin en el día ${nombresDias[dia]}.`, "error");
                valido = false;
                break;
            }
            if (hInicio >= hFin) {
                Swal.fire("Error", `La hora de inicio debe ser menor que la hora fin en el día ${nombresDias[dia]}.`, "error");
                valido = false;
                break;
            }     
                

            horarios.push({
                diasemana: dia,
                horainicio: hInicio || null,
                horafin: hFin || null,
                estado: activo
            });
        }

        if (!valido) return;

        // Recoger todos los empleados desde la tabla principal
        const filasEmpleados = document.querySelectorAll(".admin-container tbody tr");
        const empleadosHorarios = [];

        filasEmpleados.forEach(tr => {
            const empleadoId = parseInt(tr.dataset.id);
            if (empleadoId > 0) {
                empleadosHorarios.push({
                    EmpleadoId: empleadoId,
                    Horarios: horarios
                });
            }
        });

        if (empleadosHorarios.length === 0) {
            Swal.fire("Error", "No hay empleados para asignar horarios.", "error");
            return;
        }

        // Enviar con AJAX 
        spinnerModal.show();
        $.ajax({
            url: "/Admin/SaveHorarios",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify(empleadosHorarios),
            success: function (data) {
                if (data.success) {
                    Swal.fire("Éxito", data.message, "success").then(() => {
                        spinnerModal.hide();
                        bootstrap.Modal.getInstance(document.getElementById("modalHorarioTodos")).hide();
                    });
                } else {
                    Swal.fire("Error", data.message, "error").then(() => {
                        spinnerModal.hide();
                    });
                }
            },
            error: function () {
                Swal.fire("Error", "No se pudieron guardar los horarios.", "error").then(() => {
                    spinnerModal.hide();
                });
            }
        });
    });

    // ---------------------------
    // Inicialización
    // ---------------------------
    cargarSedes();
});
