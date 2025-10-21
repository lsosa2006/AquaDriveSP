document.addEventListener("DOMContentLoaded", function () {
    const formRecuperar = document.getElementById("formRecuperar");

    if (!formRecuperar) return;

    formRecuperar.addEventListener("submit", function (e) {
        e.preventDefault();

        const usuarioId = document.getElementById("usuarioId").value.trim();
        const correo = document.getElementById("correo").value.trim();

        // Validaciones simples
        if (!usuarioId || isNaN(usuarioId)) {
            Swal.fire("Error", "El documento debe ser un número válido.", "error");
            return;
        }
        if (!correo) {
            Swal.fire("Error", "Ingresa un correo válido.", "error");
            return;
        }

        // Mostrar spinner modal
        const spinnerModal = new bootstrap.Modal(document.getElementById("spinnerModal"));
        spinnerModal.show();

        // Llamada AJAX
        $.ajax({
            url: '/Login/RecuperarContrasena',
            type: 'POST',
            data: { usuarioId, correo },
            success: function (data) {
                spinnerModal.hide();

                if (data.exito) {
                    Swal.fire({
                        icon: "success",
                        title: "Exito",
                        text: data.mensaje,
                        confirmButtonColor: "#3085d6"
                    }).then(() => {
                        spinnerModal.hide();
                        window.location.href = "/Login/IniciarSesion";
                    });
                } else {
                    Swal.fire({
                        icon: "error",
                        title: "Error",
                        text: data.mensaje,
                        confirmButtonColor: "#d33"
                    }).then(() => {
                        spinnerModal.hide();
                    });
                }
            },
            error: function (xhr, status, error) {
                spinnerModal.hide();
                console.error("Error AJAX:", error);
                Swal.fire({
                    icon: "error",
                    title: "Error",
                    text: "Ocurrio un problema con el servidor.",
                    confirmButtonColor: "#d33"
                }).then(() => {
                    spinnerModal.hide();
                });
            }
        });
    });
});
