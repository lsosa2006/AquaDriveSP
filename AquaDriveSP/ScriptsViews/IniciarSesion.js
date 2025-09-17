// Archivo: Login.js
document.addEventListener("DOMContentLoaded", function () {

    const formLogin = document.querySelector("form[action='/Login/IniciarSesion']");

    if (formLogin) {
        formLogin.addEventListener("submit", function (e) {
            e.preventDefault();

            const usuarioId = document.querySelector("#documento").value;
            const contrasena = document.querySelector("#contrasena").value;

            fetch("/Login/IniciarSesion", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ usuarioId, contrasena })
            })
                .then(res => res.json())
                .then(data => {
                    if (data.exito) {
                        Swal.fire("Exito", data.mensaje, "success").then(() => {
                            document.querySelector("#contrasena").value = "";
                            window.location.href = "/Home/Dashboard";
                        });
                    } else {
                        Swal.fire("Error", data.mensaje, "error");
                    }
                })
                .catch(err => console.error(err));
        });
    }
});