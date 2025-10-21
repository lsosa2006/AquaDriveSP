document.addEventListener("DOMContentLoaded", function () {
    const formLogin = document.querySelector("form[action='/Login/IniciarSesion']");
    if (!formLogin) return;

    formLogin.addEventListener("submit", function (e) {
        e.preventDefault();

        const usuarioId = document.querySelector("#documento").value.trim();
        const contrasena = document.querySelector("#contrasena").value.trim();

        // Validaciones simples
        if (!usuarioId || isNaN(usuarioId)) {
            Swal.fire("Error", "El documento debe ser un número válido.", "error");
            return;
        }
        if (!contrasena) {
            Swal.fire("Error", "La contraseña es obligatoria.", "error");
            return;
        }

        // Mostrar spinner
        const spinnerModal = new bootstrap.Modal(document.getElementById("spinnerModal"));
        spinnerModal.show();

        $.ajax({
            url: '/Login/IniciarSesion',
            type: 'POST',
            data: { usuarioId, contrasena },
            success: function (data) {
                spinnerModal.hide();

                if (data.exito) {
                    Swal.fire({
                        icon: "success",
                        title: "Éxito",
                        text: data.mensaje,
                        confirmButtonColor: "#3085d6"
                    }).then(() => {
                        spinnerModal.hide();
                        document.querySelector("#contrasena").value = "";
                        window.location.href = "/Home/Dashboard";
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
                    text: "Ocurrió un problema con el servidor.",
                    confirmButtonColor: "#d33"
                }).then(() => {
                    spinnerModal.hide();
                });
            }
        });
    });
});
