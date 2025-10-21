$(document).ready(function () {
    cargarResenas();

    // === CARGAR RESEÑAS ===
    function cargarResenas() {

        $.getJSON("/Cliente/GetResenas", function (response) {

            if (response.success) {
                let html = "";
                if (response.data.length === 0) {
                    html = "<p class='text-center mt-4'>No has realizado reseñas todavía.</p>";
                } else {
                    response.data.forEach(r => {
                        html += `
                            <div class="resena-card shadow-sm p-3 mb-3 bg-light rounded">
                                <div class="resena-header d-flex justify-content-between align-items-center">
                                    <h5>${r.Servicio}</h5>
                                    <div class="stars text-warning">${"⭐".repeat(r.Puntuacion)}</div>
                                </div>
                                <div class="resena-body mt-2">
                                    <p><strong>Empleado:</strong> ${r.Empleado}</p>
                                    <p><strong>Placa:</strong> ${r.Placa}</p>
                                    <p><strong>Comentario:</strong> ${r.Descripcion}</p>
                                    <p><small><strong>Fecha:</strong> ${r.Fecha}</small></p>
                                </div>
                                <div class="resena-actions text-end mt-2">
                                    <button class="btn btn-success btn-sm btn-edit" 
                                            data-id="${r.resenaid}" 
                                            data-puntuacion="${r.Puntuacion}" 
                                            data-descripcion="${r.Descripcion}">
                                        <i class="fa fa-edit"></i> Editar
                                    </button>
                                    <button class="btn btn-danger btn-sm btn-delete" data-id="${r.resenaid}">
                                        <i class="fa fa-trash"></i> Eliminar
                                    </button>
                                </div>
                            </div>
                        `;
                    });
                }
                $("#listaResenas").html(html);
            } else {
                Swal.fire({
                    icon: "error",
                    title: "Error",
                    text: response.message
                });
            }
        }).fail(() => {
            Swal.fire({
                icon: "error",
                title: "Error",
                text: "No se pudieron cargar las reseñas. Inténtalo nuevamente."
            });
        });
    }

    // === ABRIR MODAL PARA EDITAR ===
    $(document).on("click", ".btn-edit", function () {
        $("#resenaId").val($(this).data("id"));
        $("#puntuacion").val($(this).data("puntuacion"));
        $("#descripcion").val($(this).data("descripcion"));
        $("#modalResena").modal("show");
    });

    // === GUARDAR CAMBIOS ===
    $("#btnGuardarResena").click(function () {
        const data = {
            resenaid: $("#resenaId").val(),
            puntuacion: $("#puntuacion").val(),
            descripcion: $("#descripcion").val()
        };


        $.post("/Cliente/EditarResena", data, function (response) {

            Swal.fire({
                icon: response.success ? "success" : "error",
                title: response.success ? "Éxito" : "Error",
                text: response.message
            }).then(() => {
                if (response.success) {
                    $("#modalResena").modal("hide");
                    cargarResenas();
                }
            });
        }).fail(() => {
            Swal.fire({
                icon: "error",
                title: "Error",
                text: "Ocurrió un error al guardar los cambios."
            });
        });
    });

    // === ELIMINAR RESEÑA ===
    $(document).on("click", ".btn-delete", function () {
        const id = $(this).data("id");

        Swal.fire({
            title: "¿Eliminar reseña?",
            text: "Esta acción no se puede deshacer.",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#d33",
            cancelButtonColor: "#3085d6",
            confirmButtonText: "Sí, eliminar",
            cancelButtonText: "Cancelar"
        }).then(result => {
            if (result.isConfirmed) {

                $.post("/Cliente/EliminarResena", { resenaid: id }, function (response) {

                    Swal.fire({
                        icon: response.success ? "success" : "error",
                        title: response.success ? "Eliminado" : "Error",
                        text: response.message
                    }).then(() => {
                        if (response.success) cargarResenas();
                    });
                }).fail(() => {
                    Swal.fire({
                        icon: "error",
                        title: "Error",
                        text: "Ocurrió un error al eliminar la reseña."
                    });
                });
            }
        });
    });
});
