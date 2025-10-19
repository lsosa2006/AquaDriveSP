$(document).ready(function () {
    // Referencias globales
    const $tabla = $("#tablaVehiculos tbody");
    const $modal = $("#modalVehiculo");
    const $spinner = $("#spinnerModal");
    const $btnGuardar = $("#btnGuardar");
    const $form = $("#formVehiculo");

    // Cargar lista al iniciar
    listarVehiculos();

    

    // 🔹 Listar vehículos
    function listarVehiculos() {

        $.ajax({
            url: "/Cliente/GetVehiculos",
            type: "GET",
            dataType: "json",
            success: function (response) {
                $tabla.empty();

                if (response.success && response.data.length > 0) {
                    $.each(response.data, function (i, v) {
                        const fila = `
                            <tr>
                                <td>${v.placa}</td>
                                <td>${v.marca}</td>
                                <td>${v.modelo}</td>
                                <td>${v.color}</td>
                                <td>
                                    <button class="btn btn-green btn-sm btnEditar" data-placa="${v.placa}">
                                        <i class="bi bi-pencil"></i>
                                    </button>
                                    <button class="btn btn-red btn-sm btnEliminar" data-placa="${v.placa}">
                                        <i class="bi bi-trash"></i>
                                    </button>
                                </td>
                            </tr>`;
                        $tabla.append(fila);
                    });
                } else {
                    $tabla.append(`<tr><td colspan="5" class="text-center text-muted">No hay vehículos registrados</td></tr>`);
                }
            },
            error: function () {
                Swal.fire("Error", "No se pudo cargar la lista de vehículos", "error");
            }
        });
    }

    // 🔹 Nuevo vehículo
    $("#btnNuevo").click(function () {
        limpiarFormulario();
        $btnGuardar.attr("data-accion", "crear");
        $("#modalVehiculoLabel").text("Agregar Vehículo");
        $modal.modal("show");
    });

    // 🔹 Guardar (crear o editar)
    $btnGuardar.click(function (e) {
        e.preventDefault();

        const accion = $(this).attr("data-accion");
        const vehiculo = {
            placa: $("#txtPlaca").val().trim(),
            marca: $("#txtMarca").val().trim(),
            modelo: $("#txtModelo").val().trim(),
            color: $("#txtColor").val().trim()
        };

        if (!vehiculo.placa || !vehiculo.marca || !vehiculo.modelo || !vehiculo.color) {
            Swal.fire("Atención", "Todos los campos son obligatorios", "warning");
            return;
        }

        const url = accion === "crear" ? "/Cliente/CrearVehiculo" : "/Cliente/EditarVehiculo";


        $.ajax({
            url: url,
            type: "POST",
            data: vehiculo,
            success: function (response) {
                if (response.success) {
                    Swal.fire("Éxito", response.message, "success");
                    $modal.modal("hide");
                    listarVehiculos();
                } else {
                    Swal.fire("Atención", response.message, "warning");
                }
            },
            error: function () {
                Swal.fire("Error", "Error al guardar el vehículo", "error");
            }
        });
    });

    // 🔹 Editar
    $tabla.on("click", ".btnEditar", function () {
        const placa = $(this).data("placa");

        // Buscar datos de la fila
        const fila = $(this).closest("tr");
        const marca = fila.find("td:eq(1)").text();
        const modelo = fila.find("td:eq(2)").text();
        const color = fila.find("td:eq(3)").text();

        $("#txtPlaca").val(placa).prop("disabled", true);
        $("#txtMarca").val(marca);
        $("#txtModelo").val(modelo);
        $("#txtColor").val(color);

        $btnGuardar.attr("data-accion", "editar");
        $("#modalVehiculoLabel").text("Editar Vehículo");
        $modal.modal("show");
    });

    // 🔹 Eliminar
    $tabla.on("click", ".btnEliminar", function () {
        const placa = $(this).data("placa");

        Swal.fire({
            title: "¿Eliminar vehículo?",
            text: `¿Seguro que deseas eliminar el vehículo con placa ${placa}?`,
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Sí, eliminar",
            cancelButtonText: "Cancelar",
            confirmButtonColor: "#d33"
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: "/Cliente/EliminarVehiculo",
                    type: "POST",
                    data: { placa: placa },
                    success: function (response) {
                        if (response.success) {
                            Swal.fire("Eliminado", response.message, "success");
                            listarVehiculos();
                        } else {
                            Swal.fire("Atención", response.message, "warning");
                        }
                    },
                    error: function () {
                        Swal.fire("Error", "Error al eliminar el vehículo", "error");
                    }
                });
            }
        });
    });

    // 🔹 Limpiar formulario
    function limpiarFormulario() {
        $form.trigger("reset");
        $("#txtPlaca").prop("disabled", false);
    }
});
