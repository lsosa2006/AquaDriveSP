document.addEventListener("DOMContentLoaded", function () {
    const tbody = document.querySelector(".sede-container tbody");
    const btnAdd = document.getElementById("btn-add-sede");
    const modalEl = document.getElementById("modalSede");
    const form = document.getElementById("form-sede-modal");
    let sedes = [];
    const spinnerModal = new bootstrap.Modal(document.getElementById("spinnerModal"));

    function cargarSedes() {
        $.ajax({
            url: "/Admin/GetSedes",
            method: "GET",
            success: function (data) {
                if (data.success) {
                    sedes = data.sedes;
                    renderTabla();
                } else {
                    Swal.fire("Error", data.message, "error");
                }
            },
            error: function () {
                Swal.fire("Error", "No se pudieron cargar las sedes.", "error");
            }
        });
    }

    function renderTabla() {
        tbody.innerHTML = "";

        if (sedes.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="5" class="text-center text-muted py-3">
                        <i class="bi bi-info-circle"></i> Sin registros
                    </td>
                </tr>`;
            return;
        }

        sedes.forEach(s => {
            const tr = document.createElement("tr");
            tr.dataset.id = s.sedeid;

            tr.innerHTML = `
                <td>${s.sedeid}</td>
                <td>${s.nombre}</td>
                <td>${s.direccion}</td>
                <td>${s.telefono}</td>
                <td>
                    <button class="btn btn-primary btn-sm btn-editar" title="Editar">
                        <i class="bi bi-pencil-square"></i>
                    </button>
                    <button class="btn btn-danger btn-sm btn-eliminar" title="Eliminar">
                        <i class="bi bi-x-lg"></i>
                    </button>
                </td>
            `;
            if (s.sedeid !== 0) tbody.appendChild(tr);
        });
    }

    // click en editar / eliminar
    tbody.addEventListener("click", function (e) {
        const tr = e.target.closest("tr");
        if (!tr) return;
        const sedeid = parseInt(tr.dataset.id, 10);

        if (e.target.closest(".btn-eliminar")) {
            Swal.fire({
                title: "Eliminar sede",
                text: "Esta accion no se puede deshacer.",
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
                        url: "/Admin/DeleteSede",
                        method: "POST",
                        data: { sedeid },
                        success: function (data) {
                            if (data.success) {
                                Swal.fire("Eliminado", "Sede eliminada.", "success").then(() => {
                                    spinnerModal.hide();
                                    cargarSedes();
                                });
                                
                            } else {
                                Swal.fire("Error", data.message, "error").then(() => {
                                    spinnerModal.hide();
                                });;
                            }
                        }
                    });
                }
            });
        }
        else if (e.target.closest(".btn-editar")) {
            const sede = sedes.find(s => s.sedeid === sedeid);
            if (!sede) return;

            document.getElementById("nombre").value = sede.nombre;
            document.getElementById("direccion").value = sede.direccion;
            document.getElementById("telefono").value = sede.telefono;

            btnAdd.dataset.editing = sedeid;
            btnAdd.textContent = "GUARDAR";

            const modalInstance = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
            modalInstance.show();
        }
    });

    // reset al cerrar modal
    modalEl.addEventListener("hidden.bs.modal", function () {
        form.reset();
        btnAdd.dataset.editing = "";
        btnAdd.textContent = "AGREGAR";
    });

    // agregar o editar sede
    btnAdd.addEventListener("click", function () {
        const editingId = parseInt(this.dataset.editing, 10);
        const nuevaSede = {
            nombre: document.getElementById("nombre").value.trim(),
            direccion: document.getElementById("direccion").value.trim(),
            telefono: document.getElementById("telefono").value.trim()
        };

        if (!nuevaSede.nombre || !nuevaSede.direccion || !nuevaSede.telefono) {
            Swal.fire("Error", "Todos los campos son obligatorios.", "error");
            return;
        }

        if (!isNaN(editingId) && editingId > 0) {
            // modo edición
            nuevaSede.sedeid = editingId;
            spinnerModal.show();
            $.ajax({
                url: "/Admin/EditarSede",
                method: "POST",
                contentType: "application/json",
                data: JSON.stringify(nuevaSede),
                success: function (data) {
                    if (data.success) {
                        Swal.fire("Exito", data.message, "success").then(() => {
                            spinnerModal.hide();
                            bootstrap.Modal.getInstance(modalEl).hide();
                            cargarSedes();
                        });
                    } else {
                        Swal.fire("Error", data.message, "error").then(() => {
                            spinnerModal.hide();
                        });
                    }
                }
            });
        } else {
            // modo crear
            spinnerModal.show();
            $.ajax({
                url: "/Admin/CrearSede",
                method: "POST",
                contentType: "application/json",
                data: JSON.stringify(nuevaSede),
                success: function (data) {
                    if (data.success) {
                        Swal.fire("Exito", data.message, "success").then(() => {
                            spinnerModal.hide();
                            bootstrap.Modal.getInstance(modalEl).hide();
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

    cargarSedes();
});
