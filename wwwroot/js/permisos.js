$(function () {
    const config = window.permisosConfig || {};
    const modal = new bootstrap.Modal(
        document.getElementById("permisoModal")
    );
    let isEdit = false;

    function showError(error) {
        const data = error.response?.data;
        const message = typeof data === "string"
            ? data
            : data?.message || data?.errorMessage ||
            error.message || "No se pudo completar la operación.";

        Swal.fire("Error", message, "error");
    }

    function checkResponse(response) {
        if (!response.data || response.data.success !== true) {
            throw new Error(
                response.data?.message ||
                "La operación no se completó. Revisa tu sesión."
            );
        }
    }

    const validator = new JustValidate("#permisoForm", {
        validateBeforeSubmitting: true,
        focusInvalidField: true,
        errorFieldCssClass: "is-invalid",
        successFieldCssClass: "is-valid"
    });

    validator
        .addField("#permisoNombre", [
            {
                rule: "required",
                errorMessage: "El nombre es obligatorio."
            },
            {
                rule: "maxLength",
                value: 150,
                errorMessage: "Máximo 150 caracteres."
            },
            {
                rule: "customRegexp",
                value: /^[a-z][a-z0-9_]*(\.[a-z][a-z0-9_]*)+$/,
                errorMessage: "Usa el formato: estudiante.crear"
            }
        ])
        .addField("#permisoDescripcion", [
            {
                rule: "maxLength",
                value: 250,
                errorMessage: "Máximo 250 caracteres."
            }
        ]);

    const textRenderer = $.fn.dataTable.render.text();

    const table = $("#permisosTable").DataTable({
        processing: true,
        serverSide: true,
        ajax: function (request, callback) {
            $("#permisosError").addClass("d-none");

            $.ajax({
                url: "/Admin/GetPermissionData",
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
                    $("#permisosError")
                        .text("No se pudieron cargar los permisos. Revisa tu sesión y vuelve a intentar.")
                        .removeClass("d-none");

                    callback({
                        draw: request.draw,
                        recordsTotal: 0,
                        recordsFiltered: 0,
                        data: []
                    });
                });
        },
        columns: [
            { data: "id" },
            { data: "name", render: textRenderer },
            {
                data: "description",
                defaultContent: "",
                render: textRenderer
            },
            {
                data: null,
                orderable: false,
                searchable: false,
                render: function (data, type, row) {
                    let buttons = "";

                    if (config.canUpdate) {
                        buttons += `
                            <button type="button"
                                    class="btn btn-sm btn-outline-primary me-1 btn-edit"
                                    data-id="${row.id}">
                                Editar
                            </button>`;
                    }

                    if (config.canDelete) {
                        buttons += `
                            <button type="button"
                                    class="btn btn-sm btn-outline-danger btn-delete"
                                    data-id="${row.id}">
                                Eliminar
                            </button>`;
                    }

                    return buttons || '<span class="text-muted">—</span>';
                }
            }
        ],
        language: {
            processing: "Cargando...",
            search: "Buscar:",
            lengthMenu: "Mostrar _MENU_ registros",
            info: "Mostrando _START_ a _END_ de _TOTAL_",
            infoEmpty: "Sin registros",
            infoFiltered: "(de _MAX_ registros)",
            zeroRecords: "No se encontraron resultados",
            emptyTable: "No hay permisos registrados",
            paginate: {
                first: "Primero",
                last: "Último",
                next: "Siguiente",
                previous: "Anterior"
            }
        },
        order: [[1, "asc"]]
    });

    $("#btnNuevoPermiso").on("click", function () {
        isEdit = false;
        document.getElementById("permisoForm").reset();
        $("#permisoId").val("");
        validator.refresh();
        $("#permisoModalTitle").text("Nuevo permiso");
        modal.show();
    });

    $("#permisosTable").on("click", ".btn-edit", async function () {
        const id = $(this).data("id");

        try {
            const response = await axios.get(
                "/Admin/GetPermissionById",
                { params: { id: id } }
            );

            const permission = response.data;

            if (!permission || !permission.id) {
                throw new Error("No se pudo cargar el permiso.");
            }

            isEdit = true;
            validator.refresh();
            $("#permisoId").val(permission.id);
            $("#permisoNombre").val(permission.name);
            $("#permisoDescripcion").val(permission.description || "");
            $("#permisoModalTitle").text("Editar permiso");
            modal.show();
        } catch (error) {
            showError(error);
        }
    });

    $("#permisosTable").on("click", ".btn-delete", async function () {
        const id = $(this).data("id");

        const confirmation = await Swal.fire({
            title: "¿Eliminar permiso?",
            text: "Esta acción eliminará el permiso seleccionado.",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Sí, eliminar",
            cancelButtonText: "Cancelar",
            confirmButtonColor: "#d33"
        });

        if (!confirmation.isConfirmed) return;

        try {
            const response = await axios.post(
                "/Admin/DeletePermission",
                null,
                { params: { id: id } }
            );

            checkResponse(response);
            table.ajax.reload(null, false);
            Swal.fire(
                "Eliminado",
                response.data.message || "Permiso eliminado.",
                "success"
            );
        } catch (error) {
            showError(error);
        }
    });

    validator.onSuccess(async function (event) {
        event.preventDefault();

        const button = document.getElementById("btnGuardarPermiso");
        if (button.disabled) return;
        button.disabled = true;

        const payload = {
            id: isEdit ? Number($("#permisoId").val()) : null,
            name: $("#permisoNombre").val().trim(),
            descripcion: $("#permisoDescripcion").val().trim() || null
        };

        try {
            const url = isEdit
                ? "/Admin/UpdatePermission"
                : "/Admin/CreatePermission";

            const response = await axios.post(url, payload);
            checkResponse(response);

            modal.hide();
            table.ajax.reload(null, false);

            Swal.fire({
                title: "Guardado",
                text: response.data.message || "Permiso guardado.",
                icon: "success",
                timer: 1500,
                showConfirmButton: false
            });
        } catch (error) {
            showError(error);
        } finally {
            button.disabled = false;
        }
    });
});