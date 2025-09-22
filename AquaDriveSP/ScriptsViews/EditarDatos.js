document.addEventListener("DOMContentLoaded", function () {
    const btnGuardar = document.getElementById("btn-guardar");

    btnGuardar.addEventListener("click", function () {
        let valid = true;
        let errors = [];

        const usuarioid = document.getElementById("usuarioid").value;
        const nombre = document.getElementById("nombre");
        const apellido = document.getElementById("apellido");
        const email = document.getElementById("email");
        const telefono = document.getElementById("telefono");

        // Validaciones
        if (!nombre.value.trim()) {
            valid = false;
            errors.push("El nombre es obligatorio.");
            nombre.classList.add("is-invalid");
        } else nombre.classList.remove("is-invalid");

        if (!apellido.value.trim()) {
            valid = false;
            errors.push("El apellido es obligatorio.");
            apellido.classList.add("is-invalid");
        } else apellido.classList.remove("is-invalid");

        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!email.value.trim() || !emailRegex.test(email.value)) {
            valid = false;
            errors.push("Ingresa un correo válido.");
            email.classList.add("is-invalid");
        } else email.classList.remove("is-invalid");

        const telefonoRegex = /^[0-9]{7,}$/;
        if (!telefono.value.trim() || !telefonoRegex.test(telefono.value)) {
            valid = false;
            errors.push("Ingresa un número de teléfono válido (mínimo 7 dígitos).");
            telefono.classList.add("is-invalid");
        } else telefono.classList.remove("is-invalid");

        if (!valid) {
            Swal.fire({
                icon: "error",
                title: "Campos inválidos",
                html: errors.map(err => `<p style="margin:0">${err}</p>`).join(""),
                confirmButtonColor: "#3085d6"
            });
            return;
        }

        // Construir objeto para enviar
        const usuario = {
            usuarioid: usuarioid,
            nombre: nombre.value.trim(),
            apellido: apellido.value.trim(),
            email: email.value.trim(),
            telefono: telefono.value.trim()
        };

        // Llamado AJAX
        $.ajax({
            url: "/Cuenta/ActualizarDatosPersonales",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify(usuario),
            success: function (data) {
                if (data.success) {
                    Swal.fire({
                        icon: "success",
                        title: "¡Éxito!",
                        text: data.message,
                        confirmButtonColor: "#3085d6"
                    });
                } else {
                    Swal.fire("Error", data.message, "error");
                }
            },
            error: function () {
                Swal.fire("Error", "No se pudo actualizar el usuario.", "error");
            }
        });
    });
});
