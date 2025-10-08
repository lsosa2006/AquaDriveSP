document.addEventListener("DOMContentLoaded", function () {
    const formRegistro = document.querySelector("form[action='/Login/Registro']");

    if (!formRegistro) return;

    formRegistro.addEventListener("submit", function (e) {
        e.preventDefault();

        // Captura de valores
        const usuarioId = document.getElementById("usuarioId").value.trim();
        const nombre = document.getElementById("nombre").value.trim();
        const apellido = document.getElementById("apellido").value.trim();
        const correo = document.getElementById("correo").value.trim();
        const telefono = document.getElementById("telefono").value.trim();
        const direccion = document.getElementById("direccion").value.trim();
        const contrasena = document.getElementById("contrasena").value.trim();
        const tipoCuentaInput = document.querySelector("input[name='tipoCuenta']:checked");
        const tipoCuenta = tipoCuentaInput ? tipoCuentaInput.value : "";

        // Expresiones regulares
        const regexLetras = /^[a-zA-Z·ÈÌÛ˙¡…Õ”⁄Ò—\s]+$/;
        const regexCorreo = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        const regexTelefono = /^[0-9]{10}$/;
        /*const regexDireccion = /^[a-zA-Z0-9\s.,#-]{5,}$/;*/
        const regexDireccion = /^(?=.*\p{L})(?=.*\d)[\p{L}\d\s.,#-]{5,}$/u;
        // Validaciones
        if (!usuarioId || isNaN(usuarioId)) {
            Swal.fire("Error", "El documento debe ser un n˙mero v·lido.", "error");
            return;
        }

        if (nombre.length < 2 || !regexLetras.test(nombre)) {
            Swal.fire("Error", "El nombre debe tener al menos 2 caracteres y solo contener letras.", "error");
            return;
        }

        if (apellido.length < 2 || !regexLetras.test(apellido)) {
            Swal.fire("Error", "El apellido debe tener al menos 2 caracteres y solo contener letras.", "error");
            return;
        }

        if (!regexCorreo.test(correo)) {
            Swal.fire("Error", "Ingresa un correo electrÛnico v·lido.", "error");
            return;
        }

        if (!regexTelefono.test(telefono)) {
            Swal.fire("Error", "El telefono debe contener solo numeros (10 digitos).", "error");
            return;
        }

        if (tipoCuenta === "cliente" && (!direccion || !regexDireccion.test(direccion))) {
            Swal.fire("Error", "Ingresa una direcciÛn v·lida (mÌnimo 5 caracteres, letras, n˙meros y sÌmbolos , . # -).", "error");
            return;
        }

        if (contrasena.length < 4 || contrasena.length > 8) {
            Swal.fire("Error", "La contraseÒa debe tener entre 4 y 8 caracteres.", "error");
            return;
        }

        if (!tipoCuenta) {
            Swal.fire("Error", "Selecciona un tipo de cuenta.", "error");
            return;
        }
        // Mostrar spinner modal
        const spinnerModal = new bootstrap.Modal(document.getElementById('spinnerModal'));
        spinnerModal.show();

        // Enviar datos al controlador por AJAX
        $.ajax({
            url: '/Login/Registro',
            type: 'POST',
            data: {
                usuarioId,
                nombre,
                apellido,
                correo,
                telefono,
                direccion,
                contrasena,
                tipoCuenta
            },
            success: function (data) {
                spinnerModal.hide(); // Oculta spinner

                if (data.exito) {
                    Swal.fire("Exito", data.mensaje, "success").then(() => {
                        spinnerModal.hide();
                        window.location.href = "/Login/IniciarSesion";
                    });
                } else {
                    Swal.fire("Error", data.mensaje, "error").then(() => {
                        spinnerModal.hide();
                    });;
                }
            },
            error: function (xhr, status, error) {
                spinnerModal.hide();
                console.error("Error AJAX:", error);
                Swal.fire("Error", "OcurriÛ un problema al registrar el usuario.", "error").then(() => {
                    spinnerModal.hide();
                });;
            }
        });
    });
});