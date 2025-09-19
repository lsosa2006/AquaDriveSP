document.addEventListener("DOMContentLoaded", function () {
    const tbody = document.querySelector(".admin-container tbody");
    const btnGuardar = document.querySelector(".admin-container .btn-success");

    let sedes = []; // se llena desde backend
    let empleados = []; // cache empleados actuales
    let empleadoIdHorario = null; // empleado en edición de horario

    // 1. Cargar sedes
    function cargarSedes() {
        return fetch("/Admin/GetSedes")
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    sedes = data.sedes;
                } else {
                    Swal.fire("Error", data.message, "error");
                }
            })
            .catch(err => {
                console.error(err);
                Swal.fire("Error", "No se pudieron cargar las sedes.", "error");
            });
    }

    // 2. Cargar empleados
    function cargarEmpleados() {
        fetch("/Admin/GetEmpleados")
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    empleados = data.empleados;
                    renderTabla();
                } else {
                    Swal.fire("Error", data.message, "error");
                }
            })
            .catch(err => {
                console.error(err);
                Swal.fire("Error", "No se pudieron cargar los empleados.", "error");
            });
    }

    // 3. Renderizar tabla
    function renderTabla() {
        tbody.innerHTML = "";
        empleados.forEach(emp => {
            const tr = document.createElement("tr");
            tr.innerHTML = `
                <td>${emp.usuarioid}</td>
                <td>${emp.usuario.nombre}</td>
                <td>${emp.usuario.apellido}</td>
                <td>${emp.usuario.email}</td>
                <td>${emp.usuario.telefono}</td>
                <td>${emp.usuario.contrasena}</td>
                <td>
                    <select class="form-select">
                        <option value="0">Seleccionar</option>
                        ${sedes.map(s => `<option value="${s.sedeid}" ${s.sedeid === emp.sedeid ? "selected" : ""}>${s.nombre}</option>`).join("")}
                    </select>
                </td>
                <td><input type="date" class="form-control" value="${emp.fechacontratacion ? emp.fechacontratacion.split("T")[0] : ""}"></td>
                <td>
                    <label class="switch">
                        <input type="checkbox" ${emp.estado === "Aceptado" ? "checked" : ""}>
                        <span class="slider"></span>
                    </label>
                </td>
                <td>
                    <div class="action-buttons">
                        <button class="btn-blue btn-sm btn-horario" title="Asignar horario"><i class="bi bi-clock"></i></button>
                        <button class="btn btn-danger btn-sm btn-eliminar" title="Eliminar"><i class="bi bi-x-lg"></i></button>
                    </div>
                </td>
            `;
            tr.dataset.id = emp.empleadoid;
            tbody.appendChild(tr);
        });
    }

    // 4. Guardar cambios (bulk update)
    btnGuardar.addEventListener("click", function () {
        const filas = tbody.querySelectorAll("tr");
        const empleadosActualizados = [];

        filas.forEach(tr => {
            const id = parseInt(tr.dataset.id);
            const sedeid = parseInt(tr.querySelector("select").value);
            const fecha = tr.querySelector("input[type='date']").value;
            const estado = tr.querySelector("input[type='checkbox']").checked ? "aceptado" : "pendiente";

            // validaciones
            if (estado === "aceptado" && (isNaN(sedeid) || sedeid === 0)) {
                Swal.fire("Error", "Un empleado aceptado debe tener una sede asignada.", "error");
                return;
            }

            empleadosActualizados.push({
                empleadoid: id,
                sedeid: sedeid || null,
                fechacontratacion: fecha || null,
                estado
            });
        });

        fetch("/Admin/BulkUpdateEmpleados", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(empleadosActualizados)
        })
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    Swal.fire("Exito", data.message, "success");
                    cargarEmpleados();
                } else {
                    Swal.fire("Error", data.message, "error");
                }
            })
            .catch(err => {
                console.error(err);
                Swal.fire("Error", "No se pudieron guardar los cambios.", "error");
            });
    });

    // 5. Delegación para acciones de cada fila
    tbody.addEventListener("click", function (e) {
        const tr = e.target.closest("tr");
        if (!tr) return;
        const id = parseInt(tr.dataset.id);

        // Eliminar
        if (e.target.closest(".btn-eliminar")) {
            Swal.fire({
                title: "¿Eliminar?",
                text: "Esta accion no se puede deshacer.",
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: "Si, eliminar"
            }).then(result => {
                if (result.isConfirmed) {
                    fetch("/Admin/DeleteEmpleado", {
                        method: "POST",
                        headers: { "Content-Type": "application/x-www-form-urlencoded" },
                        body: new URLSearchParams({ id })
                    })
                        .then(res => res.json())
                        .then(data => {
                            if (data.success) {
                                Swal.fire("Eliminado", "Empleado eliminado.", "success");
                                cargarEmpleados();
                            } else {
                                Swal.fire("Error", data.message, "error");
                            }
                        });
                }
            });
        }

        // Horario
        if (e.target.closest(".btn-horario")) {
            empleadoIdHorario = id;
            cargarHorario(id);
            new bootstrap.Modal(document.getElementById("modalHorario")).show();
        }
    });

    // 6. Cargar horarios en modal
    function cargarHorario(empleadoId) {
        fetch(`/Admin/GetHorario?empleadoId=${empleadoId}`)
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    const filas = document.querySelectorAll("#modalHorario tbody tr");
                    filas.forEach((fila, i) => {
                        const h = data.horarios.find(x => x.diasemana === i + 1);
                        if (h) {
                            fila.querySelectorAll("input[type='time']")[0].value = h.horainicio;
                            fila.querySelectorAll("input[type='time']")[1].value = h.horafin;
                            fila.querySelector("input[type='checkbox']").checked = true;
                        } else {
                            fila.querySelectorAll("input[type='time']").forEach(inp => inp.value = "");
                            fila.querySelector("input[type='checkbox']").checked = false;
                        }
                    });
                }
            });
    }

    // 7. Guardar horarios
    document.querySelector("#modalHorario .btn-primary").addEventListener("click", function () {
        const filas = document.querySelectorAll("#modalHorario tbody tr");
        const horarios = [];

        filas.forEach((fila, i) => {
            const activo = fila.querySelector("input[type='checkbox']").checked;
            if (activo) {
                const hInicio = fila.querySelectorAll("input[type='time']")[0].value;
                const hFin = fila.querySelectorAll("input[type='time']")[1].value;
                if (!hInicio || !hFin) return;
                horarios.push({
                    diasemana: i + 1,
                    horainicio: hInicio,
                    horafin: hFin
                });
            }
        });

        fetch("/Admin/SaveHorario", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ empleadoId: empleadoIdHorario, horarios })
        })
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    Swal.fire("Éxito", data.message, "success");
                    bootstrap.Modal.getInstance(document.getElementById("modalHorario")).hide();
                } else {
                    Swal.fire("Error", data.message, "error");
                }
            });
    });

    // Inicialización
    cargarSedes().then(cargarEmpleados);
});