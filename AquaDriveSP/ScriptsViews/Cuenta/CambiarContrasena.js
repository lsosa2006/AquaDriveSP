document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("form-cambiar-contrasena");
    if (!form) return;

    const spinnerModal = new bootstrap.Modal(document.getElementById("spinnerModal"));

    form.addEventListener("submit", function (e) {
        e.preventDefault();

        const usuarioid = document.querySelector("input[name='usuarioid']").value;
        const actual = document.getElementById("contrasenaActual");
        const nueva = document.getElementById("nuevaContrasena");
        const confirmar = document.getElementById("confirmarContrasena");

        let valid = true;
        let errors = [];

        // VALIDACIONES
        if (!actual.value.trim()) {
            valid = false;
            errors.push("Debes ingresar tu contraseña actual.");
            actual.classList.add("is-invalid");
        } else actual.classList.remove("is-invalid");

        if (!nueva.value.trim()) {
            valid = false;
            errors.push("Debes ingresar una nueva contraseña.");
            nueva.classList.add("is-invalid");
        } else nueva.classList.remove("is-invalid");

        if ((nueva.value.trim() && nueva.value.length < 4) || nueva.value.length > 8) {
            valid = false;
            errors.push("La nueva contraseña debe tener entre 4 y 8 caracteres.");
            nueva.classList.add("is-invalid");
        }

        if (confirmar.value.trim() !== nueva.value.trim()) {
            valid = false;
            errors.push("La confirmación de contraseña no coincide.");
            confirmar.classList.add("is-invalid");
        } else confirmar.classList.remove("is-invalid");

        if (!valid) {
            Swal.fire({
                icon: "error",
                title: "Error",
                html: errors.map(e => `<p style="margin:0">${e}</p>`).join(""),
                confirmButtonColor: "#d33"
            });
            return;
        }

        // MOSTRAR SPINNER
        spinnerModal.show();

        $.ajax({
            url: "/Cuenta/CambiarContrasena",
            type: "POST",
            contentType: "application/json",
            headers: {
                "RequestVerificationToken": document.querySelector('input[name="__RequestVerificationToken"]')?.value || ""
            },
            data: JSON.stringify({
                usuarioid,
                contrasenaActual: actual.value,
                nuevaContrasena: nueva.value,
                confirmarContrasena: confirmar.value
            }),
            success: function (data) {
                Swal.fire({
                    icon: data.success ? "success" : "error",
                    title: data.success ? "Contraseña cambiada" : "Error",
                    text: data.message,
                    confirmButtonColor: data.success ? "#3085d6" : "#d33"
                }).then(() => {
                    spinnerModal.hide();
                    if (data.success) {
                        actual.value = "";
                        nueva.value = "";
                        confirmar.value = "";
                        window.location.href = "/Home/Dashboard";
                    }
                });
            },
            error: function (xhr, status, error) {
                console.error("Error AJAX:", error);
                Swal.fire({
                    icon: "error",
                    title: "Error",
                    text: "Hubo un problema al procesar la solicitud.",
                    confirmButtonColor: "#d33"
                }).then(() => {
                    spinnerModal.hide();
                });
            }
        });
    });
});
