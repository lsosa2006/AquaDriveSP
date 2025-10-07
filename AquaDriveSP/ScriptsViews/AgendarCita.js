$(document).ready(function () {
    const sedeSelect = $("#sede");
    const empleadoSelect = $("#empleado");
    const fechaInput = $("#fecha");
    const horaInput = $("#hora");
    const form = $("#formAgendarCita");

    // ---------------------------
    // Actualizar empleados según sede
    // ---------------------------
    sedeSelect.on("change", function () {
        const sedeId = $(this).val();
        empleadoSelect.html('<option value="0">:: Por seleccionar ::</option>');

        if (!sedeId) return;

        $.ajax({
            url: "/Cliente/GetEmpleadosPorSede",
            method: "GET",
            data: { sedeId },
            success: function (data) {
                if (data.success) {
                    data.empleados.forEach(e => {
                        empleadoSelect.append(`<option value="${e.empleadoid}">${e.Nombre}</option>`);
                    });
                } else {
                    Swal.fire("Error", data.message, "error");
                }
            },
            error: function () {
                Swal.fire("Error", "No se pudieron cargar los empleados.", "error");
            }
        });
    });

    // ---------------------------
    // Validar horario del empleado
    // ---------------------------
    function validarHorario() {
        const empleadoId = empleadoSelect.val();
        const fecha = fechaInput.val();
        const hora = horaInput.val();

        if (!empleadoId || !fecha || !hora) return;

        $.ajax({
            url: "/Cliente/ValidarHorario",
            method: "GET",
            data: { empleadoId, fecha, hora },
            success: function (data) {
                if (data.success) {
                    if (data.disponible) {
                        fechaInput.removeClass("is-invalid").addClass("is-valid");
                        horaInput.removeClass("is-invalid").addClass("is-valid");
                    } else {
                        fechaInput.removeClass("is-valid").addClass("is-invalid");
                        horaInput.removeClass("is-valid").addClass("is-invalid");
                        Swal.fire("Horario no disponible", data.mensaje, "error");
                    }
                } else {
                    Swal.fire("Error", data.message, "error");
                }
            },
            error: function () {
                Swal.fire("Error", "No se pudo validar el horario.", "error");
            }
        });
    }

    fechaInput.on("change", validarHorario);
    horaInput.on("change", validarHorario);
    empleadoSelect.on("change", validarHorario);

    // ---------------------------
    // Guardar cita
    // ---------------------------
    form.on("submit", function (e) {
        e.preventDefault();

        const vehiculo = $("#vehiculo").val();
        const sedeId = sedeSelect.val();
        const tipoServicioId = $("#tipoServicio").val();
        const empleadoId = empleadoSelect.val();
        const fecha = fechaInput.val();
        const hora = horaInput.val();

        if (!vehiculo || !sedeId || !tipoServicioId || !empleadoId || !fecha || !hora) {
            Swal.fire("Error", "Debe completar todos los campos", "error");
            return;
        }

        // Validar horario antes de guardar
        $.ajax({
            url: "/Cliente/ValidarHorario",
            method: "GET",
            data: { empleadoId, fecha, hora },
            success: function (data) {
                if (!data.success) {
                    Swal.fire("Error", data.message, "error");
                    return;
                }

                if (!data.disponible) {
                    fechaInput.removeClass("is-valid").addClass("is-invalid");
                    horaInput.removeClass("is-valid").addClass("is-invalid");
                    Swal.fire("Horario no disponible", data.mensaje, "error");
                    return;
                }

                // Guardar cita
                $.ajax({
                    url: "/Cliente/AgregarCita",
                    method: "POST",
                    contentType: "application/json",
                    data: JSON.stringify({
                        placa: vehiculo,
                        sedeid: parseInt(sedeId),
                        tiposervicioid: parseInt(tipoServicioId),
                        empleadoid: parseInt(empleadoId),
                        clienteid: "",
                        fechahorainicio: fecha + " " + hora,
                        estado: 1,
                        observaciones: ""
                    }),
                    success: function (res) {
                        if (res.success) {
                            Swal.fire("Cita Agendada", res.mensaje, "success").then(() => {
                                form[0].reset();
                                fechaInput.removeClass("is-valid is-invalid");
                                horaInput.removeClass("is-valid is-invalid");
                                empleadoSelect.html('<option value="">Seleccione...</option>');
                            });
                        } else {
                            Swal.fire("Error", res.mensaje, "error");
                        }
                    },
                    error: function () {
                        Swal.fire("Error", "No se pudo agendar la cita.", "error");
                    }
                });
            },
            error: function () {
                Swal.fire("Error", "No se pudo validar el horario.", "error");
            }
        });
    });
});
