$(document).ready(function () {
    $("#formRecuperar").submit(function (e) {
        e.preventDefault();

        let datos = {
            usuarioId: $("#usuarioId").val(),
            correo: $("#correo").val()
        };

        $.ajax({
            url: '@Url.Action("RecuperarContrasena", "Login")',
            type: "POST",
            data: datos,
            success: function (resp) {
                if (resp.exito) {
                    Swal.fire({
                        icon: "success",
                        title: "¡Éxito!",
                        text: resp.mensaje,
                        confirmButtonColor: "#3085d6"
                    }).then(() => {
                        // Redirigir a login si quieres
                        window.location.href = '@Url.Action("IniciarSesion", "Login")';
                    });
                } else {
                    Swal.fire({
                        icon: "error",
                        title: "Error",
                        text: resp.mensaje,
                        confirmButtonColor: "#d33"
                    });
                }
            },
            error: function () {
                Swal.fire({
                    icon: "error",
                    title: "Error",
                    text: "Ocurrió un problema con el servidor.",
                    confirmButtonColor: "#d33"
                });
            }
        });
    });
});