document.addEventListener("DOMContentLoaded", function () {
    // ---------------------------
    // Variables globales
    // ---------------------------
    const tbody = document.querySelector(".admin-container tbody");
    const btnAdd = document.getElementById("btn-add");
    let administradores = []; // Cache de administradores
    const spinnerModal = new bootstrap.Modal(document.getElementById("spinnerModal"));

    // ---------------------------
    // 1. Cargar administradores desde backend
    // ---------------------------
    function cargarAdministradores() {
        $.ajax({
            url: "/Admin/GetAdministradores",
            method: "GET",
            success: function (data) {
                if (data.success) {
                    administradores = data.administradores;
                    renderTabla();
                } else {
                    Swal.fire("Error", data.message, "error");
                }
            },
            error: function () {
                Swal.fire("Error", "No se pudieron cargar los administradores.", "error");
            }
        });
    }

    // ---------------------------
    // 2. Renderizar tabla de administradores
    // ---------------------------
    function renderTabla() {
        tbody.innerHTML = "";

        if (administradores.length === 0) {
            const tr = document.createElement("tr");
            tr.innerHTML = `
                <td colspan="7" class="text-center text-muted py-3">
                    <i class="bi bi-info-circle"></i> Sin registros
                </td>
            `;
            tbody.appendChild(tr);
            return;
        }

        administradores.forEach(admin => {
            const tr = document.createElement("tr");
            tr.dataset.id = admin.administradorid;

            tr.innerHTML = `
                <td>${admin.usuarioid}</td>
                <td>${admin.usuario.nombre}</td>
                <td>${admin.usuario.apellido}</td>
                <td>${admin.usuario.email}</td>
                <td>${admin.usuario.telefono}</td>
                <td>${admin.usuario.contrasena || ""}</td>
                <td>
                    <button class="btn btn-danger btn-sm btn-eliminar" title="Eliminar">
                        <i class="bi bi-x-lg"></i>
                    </button>
                </td>
            `;
            tbody.appendChild(tr);
        });
    }

    // ---------------------------
    // 3. Crear administrador
    // ---------------------------
    btnAdd.addEventListener("click", function () {
        let valido = true;
        const nuevoAdmin = {};
        const campos = ["documento", "nombre", "apellido", "email", "telefono", "contrasena"];

        campos.forEach(id => {
            const valor = document.getElementById(id).value.trim();
            nuevoAdmin[id] = valor;

            if (!valor) valido = false;
        });

        // Validación de campos específicos
        const docRegex = /^\d{10}$/; // documento 10 dígitos
        const telRegex = /^\d{10}$/;   // teléfono 10 dígitos
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/; // formato email
        const passRegex = /^.{4,8}$/;  // contraseña 4-8 caracteres

        if (!valido) {
            Swal.fire("Error", "Todos los campos son obligatorios.", "error");
            return;
        }

        if (!docRegex.test(nuevoAdmin.documento)) {
            Swal.fire("Error", "El documento debe tener 10 dígitos numéricos.", "error");
            return;
        }

        if (!telRegex.test(nuevoAdmin.telefono)) {
            Swal.fire("Error", "El teléfono debe tener 10 dígitos numéricos.", "error");
            return;
        }

        if (!emailRegex.test(nuevoAdmin.email)) {
            Swal.fire("Error", "Ingresa un email válido.", "error");
            return;
        }

        if (!passRegex.test(nuevoAdmin.contrasena)) {
            Swal.fire("Error", "La contraseña debe tener entre 4 y 8 caracteres.", "error");
            return;
        }
        spinnerModal.show();
        // AJAX POST para crear administrador usando el método Registro
        $.ajax({
            url: "/Login/Registro",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify({
                usuarioId: nuevoAdmin.documento,
                nombre: nuevoAdmin.nombre,
                apellido: nuevoAdmin.apellido,
                correo: nuevoAdmin.email,
                telefono: nuevoAdmin.telefono,
                contrasena: nuevoAdmin.contrasena,
                tipoCuenta: "admin"
            }),
            success: function (data) {
                if (data.exito) {
                    Swal.fire("Éxito", data.mensaje, "success").then(() => {
                        spinnerModal.hide();
                        // Limpiar campos
                        campos.forEach(id => document.getElementById(id).value = "");
                        // Cerrar modal
                        bootstrap.Modal.getInstance(document.getElementById("modalAdmin")).hide();
                        cargarAdministradores();
                    });
                } else {
                    Swal.fire("Error", data.mensaje, "error").then(() => {
                        spinnerModal.hide();
                    });
                }
            },
            error: function () {
                Swal.fire("Error", "No se pudo crear el administrador.", "error").then(() => {
                    spinnerModal.hide();
                });
            }
        });
    });

    // ---------------------------
    // 4. Delegación de acciones de fila (Eliminar)
    // ---------------------------
    tbody.addEventListener("click", function (e) {
        const tr = e.target.closest("tr");
        if (!tr) return;
        const adminid = parseInt(tr.dataset.id);

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
                        url: "/Admin/DeleteAdministrador",
                        method: "POST",
                        data: { adminid },
                        success: function (data) {
                            if (data.success) {
                                Swal.fire("Eliminado", "Administrador eliminado.", "success").then(() => {
                                    spinnerModal.hide();
                                    cargarAdministradores();
                                });
                                
                            } else {
                                Swal.fire("Error", data.message, "error").then(() => {
                                    spinnerModal.hide();
                                });
                            }
                        },
                        error: function () {
                            Swal.fire("Error", "No se pudo eliminar el administrador.", "error").then(() => {
                                spinnerModal.hide();
                            });
                        }
                    });
                }
            });
        }
    });

    // ---------------------------
    // Inicialización
    // ---------------------------
    cargarAdministradores();
});
