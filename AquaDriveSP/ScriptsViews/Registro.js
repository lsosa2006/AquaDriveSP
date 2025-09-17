document.addEventListener("DOMContentLoaded", function () {
    const formRegistro = document.querySelector("form[action='/Login/Registro']");

    if (formRegistro) {
        formRegistro.addEventListener("submit", function (e) {
            e.preventDefault();

            // Captura de valores
            const usuarioId = document.getElementById("usuarioId").value.trim();
            const nombre = document.getElementById("nombre").value.trim();
            const apellido = document.getElementById("apellido").value.trim();
            const correo = document.getElementById("correo").value.trim();
            const telefono = document.getElementById("telefono").value.trim();
            const contrasena = document.getElementById("contrasena").value.trim();
            const tipoCuentaInput = document.querySelector("input[name='tipoCuenta']:checked");
            const tipoCuenta = tipoCuentaInput ? tipoCuentaInput.value : "";

            // Validaciones
            if (!usuarioId || isNaN(usuarioId)) {
                Swal.fire("Error", "El documento debe ser un numero valido.", "error");
                return;
            }
            if (nombre.length < 2) {
                Swal.fire("Error", "El nombre debe tener al menos 2 caracteres.", "error");
                return;
            }
            if (apellido.length < 2) {
                Swal.fire("Error", "El apellido debe tener al menos 2 caracteres.", "error");
                return;
            }
            const regexCorreo = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!regexCorreo.test(correo)) {
                Swal.fire("Error", "Ingresa un correo electronico valido.", "error");
                return;
            }
            const regexTelefono = /^[0-9]{7,15}$/;
            if (!regexTelefono.test(telefono)) {
                Swal.fire("Error", "El telefono debe contener solo numeros (7-15 dígitos).", "error");
                return;
            }
            if (contrasena.length < 4 || contrasena.lenght > 8) {
                Swal.fire("Error", "La contrasena debe entre 4 y 8 caracteres.", "error");
                return;
            }
            if (!tipoCuenta) {
                Swal.fire("Error", "Selecciona un tipo de cuenta.", "error");
                return;
            }

            // Enviar datos al controlador
            fetch('/Login/Registro', {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: new URLSearchParams({
                    usuarioId,
                    nombre,
                    apellido,
                    correo,
                    telefono,
                    contrasena,
                    tipoCuenta
                })
            })
                .then(res => res.json())
                .then(data => {
                    if (data.exito) {
                        Swal.fire("Exito", data.mensaje, "success").then(() => {
                            window.location.href = "/Login/IniciarSesion";
                        });
                    } else {
                        Swal.fire("Error", data.mensaje, "error");
                    }
                })
                .catch(err => {
                    console.error(err);
                    Swal.fire("Error", "Ocurrio un problema al registrar el usuario.", "error");
                });
        });
    }
});