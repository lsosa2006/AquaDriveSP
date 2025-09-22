document.addEventListener("DOMContentLoaded", function () {
    const tbody = document.querySelector(".servicio-container tbody");
    const btnAdd = document.getElementById("btn-add-servicio");
    const btnOpenCrear = document.getElementById("btn-open-crear-servicio");
    const modalEl = document.getElementById("modalServicio");
    const form = document.getElementById("form-servicio-modal");
    let servicios = [];

    function cargarServicios() {
        $.ajax({
            url: "/Admin/GetServicios",
            method: "GET",
            success: function (data) {
                if (data.success) {
                    servicios = data.servicio;
                    renderTabla();
                } else {
                    Swal.fire("Error", data.message, "error");
                }
            },
            error: function () {
                Swal.fire("Error", "No se pudieron cargar los servicios.", "error");
            }
        });
    }

    function renderTabla() {
        tbody.innerHTML = "";

        if (servicios.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="5" class="text-center text-muted py-3">
                        <i class="bi bi-info-circle"></i> Sin registros
                    </td>
                </tr>`;
            return;
        }

        servicios.forEach(s => {
            const tr = document.createElement("tr");
            tr.dataset.id = s.tiposervicioid;

            tr.innerHTML = `
                <td>${s.tiposervicioid}</td>
                <td>${s.nombre}</td>
                <td>${s.duracionminutos}</td>
                <td>$${s.precio.toFixed(2)}</td>
                <td>
                    <button class="btn btn-primary btn-sm btn-editar" title="Editar">
                        <i class="bi bi-pencil-square"></i>
                    </button>
                    <button class="btn btn-danger btn-sm btn-eliminar" title="Eliminar">
                        <i class="bi bi-x-lg"></i>
                    </button>
                </td>
            `;
            tbody.appendChild(tr);
        });
    }

    // CLICK en Editar / Eliminar (delegation)
    tbody.addEventListener("click", function (e) {
        const tr = e.target.closest("tr");
        if (!tr) return;
        const servicioid = parseInt(tr.dataset.id, 10);

        if (e.target.closest(".btn-eliminar")) {
            Swal.fire({
                title: "Eliminar servicio",
                text: "Esta accion no se puede deshacer.",
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: "Si, eliminar",
                cancelButtonText: "Cancelar"
            }).then(result => {
                if (result.isConfirmed) {
                    $.ajax({
                        url: "/Admin/DeleteServicio",
                        method: "POST",
                        data: { servicioid },
                        success: function (data) {
                            if (data.success) {
                                Swal.fire("Eliminado", "Servicio eliminado.", "success");
                                cargarServicios();
                            } else {
                                Swal.fire("Error", data.message, "error");
                            }
                        }
                    });
                }
            });
        }
        else if (e.target.closest(".btn-editar")) {
            // modo edición: llenar formulario
            const servicio = servicios.find(s => s.tiposervicioid === servicioid);
            if (!servicio) return;

            document.getElementById("nombre").value = servicio.nombre;
            document.getElementById("duracionminutos").value = servicio.duracionminutos;
            document.getElementById("precio").value = servicio.precio;

            // Guardar id en el botón para indicar edición
            btnAdd.dataset.editing = servicioid;
            btnAdd.textContent = "GUARDAR";

            // abrir modal (se asegura instancia)
            const modalInstance = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
            modalInstance.show();
        }
    });

    // Si se abre el modal desde "Crear Servicio" (limpiar estado edición)
    if (btnOpenCrear) {
        btnOpenCrear.addEventListener("click", function () {
            btnAdd.dataset.editing = "";
            btnAdd.textContent = "AGREGAR";
            form.reset();
        });
    }

    // Reseteo siempre que se cierre el modal
    modalEl.addEventListener("hidden.bs.modal", function () {
        form.reset();
        btnAdd.dataset.editing = "";
        btnAdd.textContent = "AGREGAR";
    });

    // Botón AGREGAR / GUARDAR - detecta si está en edición
    btnAdd.addEventListener("click", function () {
        const editingId = parseInt(this.dataset.editing, 10);
        const nombre = document.getElementById("nombre").value.trim();
        const duracionminutos = parseInt(document.getElementById("duracionminutos").value, 10);
        const precio = parseFloat(document.getElementById("precio").value);

        if (!nombre || isNaN(duracionminutos) || isNaN(precio)) {
            Swal.fire("Error", "Todos los campos son obligatorios.", "error");
            return;
        }

        const payload = {
            nombre,
            duracionminutos,
            precio
        };

        if (!isNaN(editingId) && editingId > 0) {
            // EDICIÓN
            payload.tiposervicioid = editingId;
            $.ajax({
                url: "/Admin/EditarServicio",
                method: "POST",
                contentType: "application/json",
                data: JSON.stringify(payload),
                success: function (data) {
                    if (data.success) {
                        Swal.fire("Exito", data.message, "success");
                        const modalInstance = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
                        modalInstance.hide();
                        form.reset();
                        btnAdd.dataset.editing = "";
                        btnAdd.textContent = "AGREGAR";
                        cargarServicios();
                    } else {
                        Swal.fire("Error", data.message, "error");
                    }
                },
                error: function () {
                    Swal.fire("Error", "No se pudo actualizar el servicio.", "error");
                }
            });
        } else {
            // CREACIÓN
            $.ajax({
                url: "/Admin/CrearServicio",
                method: "POST",
                contentType: "application/json",
                data: JSON.stringify(payload),
                success: function (data) {
                    if (data.success) {
                        Swal.fire("Exito", data.message, "success");
                        const modalInstance = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
                        modalInstance.hide();
                        form.reset();
                        cargarServicios();
                    } else {
                        Swal.fire("Error", data.message, "error");
                    }
                },
                error: function () {
                    Swal.fire("Error", "No se pudo crear el servicio.", "error");
                }
            });
        }
    });

    // cargar inicialmente
    cargarServicios();
});
