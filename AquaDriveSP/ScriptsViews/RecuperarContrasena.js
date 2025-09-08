document.addEventListener("DOMContentLoaded", function () {
    const formRecuperar = document.getElementById("formRecuperar");

    if (formRecuperar) {
        formRecuperar.addEventListener("submit", function (e) {
            e.preventDefault();

            const usuarioId = document.getElementById("usuarioId").value.trim();
            const correo = document.getElementById("correo").value.trim();

            if (!usuarioId || isNaN(usuarioId)) {
                Swal.fire("Error", "El documento debe ser un número válido.", "error");
                return;
            }
            if (!correo) {
                Swal.fire("Error", "Ingresa un correo válido.", "error");
                return;
            }

            fetch('/Login/RecuperarContrasena', {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: new URLSearchParams({ usuarioId, correo })
            })
                .then(res => res.json())
                .then(data => {
                    if (data.exito) {
                        Swal.fire({
                            icon: "success",
                            title: "¡Exito!",
                            text: data.mensaje,
                            confirmButtonColor: "#3085d6"
                        }).then(() => {
                            window.location.href = "/Login/IniciarSesion";
                        });
                    } else {
                        Swal.fire({
                            icon: "error",
                            title: "Error",
                            text: data.mensaje,
                            confirmButtonColor: "#d33"
                        });
                    }
                })
                .catch(err => {
                    console.error(err);
                    Swal.fire({
                        icon: "error",
                        title: "Error",
                        text: "Ocurrió un problema con el servidor.",
                        confirmButtonColor: "#d33"
                    });
                });
        });
    }
});