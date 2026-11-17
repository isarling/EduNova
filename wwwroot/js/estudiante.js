$(function () {
    const config = window.estudiantesConfig || {};
    const modal = new bootstrap.Modal(
        document.getElementById("estudianteModal")
    );
    let isEdit = false;
    let saving = false;

    function showError(error) {
        Swal.fire(
            "Error",
            error.response?.data?.message ||
            error.message ||
            "No se pudo completar la operación.",
            "error"
        );
    }

    function checkResponse(response) {
        if (response.data?.success !== true) {
            throw new Error(
                response.data?.message ||
                "No se completó la operación. Revisa tu sesión."
            );
        }
    }

    const estudianteValidator = new JustValidate("#estudianteForm", {
        validateBeforeSubmitting: true,
        focusInvalidField: true,
        errorFieldCssClass: "is-invalid",
        successFieldCssClass: "is-valid"
    });

    estudianteValidator
        .addField("#estudianteNombre", [
            { rule: "required", errorMessage: "El nombre es obligatorio." },
            { rule: "minLength", value: 2, errorMessage: "Mínimo 2 caracteres." },
            { rule: "maxLength", value: 100, errorMessage: "Máximo 100 caracteres." }
        ])
        .addField("#estudianteApellido", [
            { rule: "required", errorMessage: "El apellido es obligatorio." },
            { rule: "minLength", value: 2, errorMessage: "Mínimo 2 caracteres." },
            { rule: "maxLength", value: 100, errorMessage: "Máximo 100 caracteres." }
        ])
        .addField("#estudianteEmail", [
            { rule: "required", errorMessage: "El correo es obligatorio." },
            { rule: "email", errorMessage: "Ingrese un correo válido." },
            { rule: "maxLength", value: 150, errorMessage: "Máximo 150 caracteres." }
        ]);

    const table = $("#estudianteTable").DataTable({
        processing: true,
        serverSide: true,
        ajax: function (request, callback) {
            $.ajax({
                url: "/Estudiante/GetData",
                type: "POST",
                data: request,
                dataType: "json"
            })
                .done(function (result) {
                    callback({
                        draw: result.draw,
                        recordsTotal: result.recordsTotal,
                        recordsFiltered:
                            result.recordsFiltered ??
                            result.recordsTotalFiltered ??
                            result.recordsTotal,
                        data: result.data || []
                    });
                })
                .fail(function () {
                    callback({
                        draw: request.draw,
                        recordsTotal: 0,
                        recordsFiltered: 0,
                        data: []
                    });

                    Swal.fire(
                        "Error",
                        "No se pudieron cargar los estudiantes. Revisa tu sesión.",
                        "error"
                    );
                });
        },
        columns: [
            { data: "id" },
            {
                data: "nombre",
                render: $.fn.dataTable.render.text()
            },
            {
                data: "apellido",
                render: $.fn.dataTable.render.text()
            },
            {
                data: "email",
                render: $.fn.dataTable.render.text()
            },
            {
                data: null,
                orderable: false,
                searchable: false,
                render: function (data) {
                    let buttons = "";

                    if (config.canUpdate) {
                        buttons += `
                            <button type="button"
                                    class="btn btn-sm btn-outline-primary me-1 btn-edit"
                                    data-id="${data.id}">
                                Editar
                            </button>`;
                    }

                    if (config.canDelete) {
                        buttons += `
                            <button type="button"
                                    class="btn btn-sm btn-outline-danger btn-delete"
                                    data-id="${data.id}">
                                Eliminar
                            </button>`;
                    }

                    return buttons || '<span class="text-muted">—</span>';
                }
            }
        ],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json"
        },
        order: [[1, "asc"]]
    });

    $("#btnNuevoEstudiante").on("click", function () {
        isEdit = false;
        document.getElementById("estudianteForm").reset();
        $("#estudianteId").val("");
        estudianteValidator.refresh();
        $("#estudianteModalTitle").text("Nuevo estudiante");
        modal.show();
    });

    $("#estudianteTable").on("click", ".btn-edit", async function () {
        try {
            const response = await axios.get("/Estudiante/GetById", {
                params: { id: this.dataset.id }
            });

            const estudiante = response.data;

            if (!estudiante?.id) {
                throw new Error("No se pudo cargar el estudiante. Revisa tu sesión.");
            }

            isEdit = true;
            estudianteValidator.refresh();
            $("#estudianteId").val(estudiante.id);
            $("#estudianteNombre").val(estudiante.nombre);
            $("#estudianteApellido").val(estudiante.apellido);
            $("#estudianteEmail").val(estudiante.email);
            $("#estudianteModalTitle").text("Editar estudiante");
            modal.show();
        } catch (error) {
            showError(error);
        }
    });

    $("#estudianteTable").on("click", ".btn-delete", async function () {
        const id = this.dataset.id;

        const confirmation = await Swal.fire({
            title: "¿Eliminar estudiante?",
            text: "Se eliminará el estudiante seleccionado.",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Sí, eliminar",
            cancelButtonText: "Cancelar",
            confirmButtonColor: "#d33"
        });

        if (!confirmation.isConfirmed) return;

        try {
            const response = await axios.post(
                "/Estudiante/Delete",
                null,
                { params: { id: id } }
            );

            checkResponse(response);
            table.ajax.reload(null, false);
            Swal.fire("Eliminado", response.data.message, "success");
        } catch (error) {
            showError(error);
        }
    });

    estudianteValidator.onSuccess(async (event) => {
        event.preventDefault();

        if (saving) return;
        saving = true;
        $("#btnGuardarEstudiante").prop("disabled", true);

        const payload = new URLSearchParams();

        if (isEdit) {
            payload.set("Id", $("#estudianteId").val());
        }

        payload.set("Nombre", $("#estudianteNombre").val().trim());
        payload.set("Apellido", $("#estudianteApellido").val().trim());
        payload.set("Email", $("#estudianteEmail").val().trim());

        try {
            const url = isEdit
                ? "/Estudiante/Update"
                : "/Estudiante/Create";

            const response = await axios.post(url, payload);

            checkResponse(response);
            modal.hide();
            table.ajax.reload(null, false);

            Swal.fire({
                title: "Guardado",
                text: response.data.message,
                icon: "success",
                timer: 1500,
                showConfirmButton: false
            });
        } catch (error) {
            showError(error);
        } finally {
            saving = false;
            $("#btnGuardarEstudiante").prop("disabled", false);
        }
    });
});