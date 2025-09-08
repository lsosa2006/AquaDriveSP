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
                            // Redirige según tipo de cuenta
                            switch (data.tipoCuenta) {
                                case "cliente":
                                    window.location.href = "/Cliente/Dashboard";
                                    break;
                                case "empleado":
                                    window.location.href = "/Empleado/Dashboard";
                                    break;
                                case "admin":
                                    window.location.href = "/Admin/Dashboard";
                                    break;
                            }
                        });
                    } else {
                        Swal.fire("Error", data.mensaje, "error");
                    }
                })
                .catch(err => console.error(err));
        });
    }

});