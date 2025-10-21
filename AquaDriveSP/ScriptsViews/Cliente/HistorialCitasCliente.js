document.addEventListener("DOMContentLoaded", function () {
    let citaSeleccionada = null;
    const modal = new bootstrap.Modal(document.getElementById("modalCalificar"));
    const estrellas = document.querySelectorAll(".star");

    // === ABRIR MODAL DE CALIFICACIÓN ===
    document.querySelectorAll(".btn-calificar").forEach(btn => {
        btn.addEventListener("click", () => {
            citaSeleccionada = btn.dataset.citaid;
            resetStars();
            document.getElementById("txtDescripcion").value = "";
            modal.show();
        });
    });

    // === SELECCIÓN DE ESTRELLAS ===
    estrellas.forEach(star => {
        star.addEventListener("click", () => {
            const val = parseInt(star.dataset.value);
            estrellas.forEach(s => s.classList.toggle("selected", parseInt(s.dataset.value) <= val));
            document.getElementById("stars").dataset.valor = val;
        });
    });

    // === ENVIAR RESEÑA ===
    document.getElementById("btnEnviarResena").addEventListener("click", () => {
        const puntuacion = parseInt(document.getElementById("stars").dataset.valor || "0");
        const descripcion = document.getElementById("txtDescripcion").value.trim();

        if (!puntuacion) {
            Swal.fire({
                icon: "warning",
                title: "Selecciona una puntuación",
                text: "Debes elegir entre 1 y 5 estrellas antes de enviar la reseña.",
                confirmButtonColor: "#3085d6"
            });
            return;
        }

        Swal.fire({
            title: "¿Confirmar reseña?",
            text: "Una vez enviada, podrás editarla desde el apartado de reseñas.",
            icon: "question",
            showCancelButton: true,
            confirmButtonText: "Enviar",
            cancelButtonText: "Cancelar",
            confirmButtonColor: "#00cfff",
            cancelButtonColor: "#d33"
        }).then(result => {
            if (result.isConfirmed) {
                $("#spinnerModal").modal("show");

                fetch("/Cliente/CrearResena", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ citaid: citaSeleccionada, puntuacion, descripcion })
                })
                    .then(res => res.json())
                    .then(data => {
                        $("#spinnerModal").modal("hide");

                        Swal.fire({
                            icon: data.success ? "success" : "error",
                            title: data.success ? "Reseña enviada" : "Error",
                            text: data.message
                        }).then(() => {
                            if (data.success) location.reload();
                        });
                    })
                    .catch(() => {
                        $("#spinnerModal").modal("hide");
                        Swal.fire({
                            icon: "error",
                            title: "Error",
                            text: "Ocurrió un problema al enviar la reseña."
                        });
                    });
            }
        });
    });

    // === RESETEAR ESTRELLAS ===
    function resetStars() {
        estrellas.forEach(s => s.classList.remove("selected"));
        document.getElementById("stars").dataset.valor = "0";
    }
});
