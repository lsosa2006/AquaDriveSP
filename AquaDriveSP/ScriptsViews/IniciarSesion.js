// Archivo: Login.js
document.addEventListener("DOMContentLoaded", function () {
    const formLogin = document.querySelector("form[action='/Login/IniciarSesion']");
    const spinnerModal = new bootstrap.Modal(document.getElementById("spinnerModal"));

    if (formLogin) {
        formLogin.addEventListener("submit", function (e) {
            e.preventDefault();

            const usuarioId = document.querySelector("#documento").value;
            const contrasena = document.querySelector("#contrasena").value;

            // Mostrar spinner
            spinnerModal.show();

            fetch("/Login/IniciarSesion", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ usuarioId, contrasena })
            })
                .then(res => res.json())
                .then(data => {
                    spinnerModal.hide(); // Ocultar spinner
                    if (data.exito) {
                        Swal.fire("Éxito", data.mensaje, "success").then(() => {
                            document.querySelector("#contrasena").value = "";
                            spinnerModal.hide(); // Ocultar spinner
                            window.location.href = "/Home/Dashboard";
                        });
                    } else {
                        Swal.fire("Error", data.mensaje, "error").then(() => {
                            spinnerModal.hide(); // Ocultar spinner
                        });   
                    }
                })
                .catch(err => {
                    spinnerModal.hide(); // Ocultar spinner también en caso de error
                    console.error(err);
                });
        });
    }
});