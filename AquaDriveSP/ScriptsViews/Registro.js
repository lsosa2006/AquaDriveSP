document.querySelector("form").addEventListener("submit", function (e) {
    e.preventDefault();

    // Captura de valores
    let usuarioId = document.getElementById("usuarioId").value.trim();
    let nombre = document.getElementById("nombre").value.trim();
    let apellido = document.getElementById("apellido").value.trim();
    let correo = document.getElementById("correo").value.trim();
    let telefono = document.getElementById("telefono").value.trim();
    let contrasena = document.getElementById("contrasena").value.trim();
    let tipoCuenta = document.querySelector("input[name='tipoCuenta']:checked");

    // Validaciones
    if (!usuarioId || isNaN(usuarioId)) {
        Swal.fire("Error", "El documento debe ser un número válido.", "error");
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
    let regexCorreo = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!regexCorreo.test(correo)) {
        Swal.fire("Error", "Ingresa un correo electrónico válido.", "error");
        return;
    }
    let regexTelefono = /^[0-9]{7,15}$/;
    if (!regexTelefono.test(telefono)) {
        Swal.fire("Error", "El teléfono debe contener solo números (7-15 dígitos).", "error");
        return;
    }
    if (contrasena.length < 6) {
        Swal.fire("Error", "La contraseña debe tener al menos 6 caracteres.", "error");
        return;
    }
    if (!tipoCuenta) {
        Swal.fire("Error", "Selecciona un tipo de cuenta.", "error");
        return;
    }

    // Enviar datos por AJAX
    fetch('@Url.Action("Registro", "Login")', {
        method: "POST",
        headers: {
            "Content-Type": "application/x-www-form-urlencoded"
        },
        body: new URLSearchParams({
            usuarioId: usuarioId,
            nombre: nombre,
            apellido: apellido,
            correo: correo,
            telefono: telefono,
            contrasena: contrasena,
            tipoCuenta: tipoCuenta.value
        })
    })
    .then(res => res.json())
    .then(data => {
        if (data.exito) {
            Swal.fire("Éxito", data.mensaje, "success")
                .then(() => {
                    window.location.href = '@Url.Action("IniciarSesion","Login")';
                });
        } else {
            Swal.fire("Error", data.mensaje, "error");
        }
    })
    .catch(err => {
        Swal.fire("Error", "Ocurrió un problema al registrar el usuario.", "error");
        console.error(err);
    });
});

