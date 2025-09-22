document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("form-cambiar-contrasena");
    if (!form) return;

    form.addEventListener("submit", async function (e) {
        e.preventDefault();

        const usuarioid = document.querySelector("input[name='usuarioid']").value;
        const actual = document.getElementById("contrasenaActual");
        const nueva = document.getElementById("nuevaContrasena");
        const confirmar = document.getElementById("confirmarContrasena");

        let valid = true;
        let errors = [];

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

        if (nueva.value.trim() && nueva.value.length < 6) {
            valid = false;
            errors.push("La nueva contraseña debe tener al menos 6 caracteres.");
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

        try {
            const response = await fetch("/Cuenta/CambiarContrasena", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": document.querySelector('input[name="__RequestVerificationToken"]')?.value || ""
                },
                body: JSON.stringify({
                    usuarioid,
                    contrasenaActual: actual.value,
                    nuevaContrasena: nueva.value,
                    confirmarContrasena: confirmar.value
                })
            });

            const data = await response.json();

            Swal.fire({
                icon: data.success ? "success" : "error",
                title: data.success ? "Contraseña cambiada" : "Error",
                text: data.message,
                confirmButtonColor: data.success ? "#3085d6" : "#d33"
            }).then(() => {
                if (data.success) {
                    window.location.href = "/Home/Dashboard";
                }
            });

        } catch (error) {
            Swal.fire({
                icon: "error",
                title: "Error",
                text: "Hubo un problema al procesar la solicitud.",
                confirmButtonColor: "#d33"
            });
        }
    });
});
