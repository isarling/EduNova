$(function () {
    const config = window.cursosConfig || {};
    const modal = new bootstrap.Modal(
        document.getElementById("cursoModal")
    );
    let isEdit = false;

    function showError(error) {
        const data = error.response?.data;
        const message = typeof data === "string"
            ? data
            : data?.message || error.message ||
            "No se pudo completar la operación.";

        Swal.fire("Error", message, "error");
    }

    function checkResponse(response) {
        if (response.data?.success !== true) {
            throw new Error(
                response.data?.message ||
                "No se completó la operación. Revisa tu sesión."
            );
        }
    }

    const table = $("#cursoTable").DataTable({
        processing: true,
        serverSide: true,
        ajax: function (request, callback) {
            $("#cursoError").addClass("d-none");

            $.ajax({
                url: "/Curso/GetData",
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
                    $("#cursoError")
                        .text("No se pudieron cargar los cursos. Revisa tu sesión.")
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
            {
                data: "nombre",
                render: $.fn.dataTable.render.text()
            },
            {
                data: "descripcion",
                defaultContent: "",
                render: $.fn.dataTable.render.text()
            },
            { data: "precio", render: $.fn.dataTable.render.number(",", ".", 2) },
            { data: "totalEstudiantes", defaultContent: 0 },
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
            emptyTable: "No hay cursos registrados",
            paginate: {
                next: "Siguiente",
                previous: "Anterior"
            }
        },
        order: [[1, "asc"]]
    });

    $("#btnNuevoCurso").on("click", function () {
        isEdit = false;
        document.getElementById("cursoForm").reset();
        $("#cursoId").val("");
        $("#cursoModalTitle").text("Nuevo curso");
        modal.show();
    });

    $("#cursoTable").on("click", ".btn-edit", async function () {
        try {
            const response = await axios.get("/Curso/GetById", {
                params: { id: this.dataset.id }
            });

            const curso = response.data;

            if (!curso?.id) {
                throw new Error("No se pudo cargar el curso. Revisa tu sesión.");
            }

            isEdit = true;
            $("#cursoId").val(curso.id);
            $("#cursoNombre").val(curso.nombre);
            $("#cursoDescripcion").val(curso.descripcion || "");
            $("#cursoPrecio").val(curso.precio);
            $("#cursoDuracion").val(curso.duracionDias);
            $("#cursoCategoria").val(curso.categoriaCursoId || "");
            $("#cursoModalidad").val(curso.modalidadCursoId || "");
            $("#cursoModalTitle").text("Editar curso");
            modal.show();
        } catch (error) {
            showError(error);
        }
    });

    $("#cursoTable").on("click", ".btn-delete", async function () {
        const id = this.dataset.id;

        const confirmation = await Swal.fire({
            title: "¿Eliminar curso?",
            text: "Se eliminará el curso seleccionado.",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Sí, eliminar",
            cancelButtonText: "Cancelar",
            confirmButtonColor: "#d33"
        });

        if (!confirmation.isConfirmed) return;

        try {
            const response = await axios.post(
                "/Curso/Delete",
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

    $("#cursoForm").on("submit", async function (event) {
        event.preventDefault();

        const button = document.getElementById("btnGuardarCurso");
        if (button.disabled) return;

        const nombre = $("#cursoNombre").val().trim();

        if (!nombre) {
            Swal.fire("Revisa los datos", "El nombre es obligatorio.", "warning");
            return;
        }

        button.disabled = true;

        const payload = new URLSearchParams();
        if (isEdit) payload.set("Id", $("#cursoId").val());
        payload.set("Nombre", nombre);
        payload.set("Precio", $("#cursoPrecio").val());
        payload.set("DuracionDias", $("#cursoDuracion").val());
        payload.set("CategoriaCursoId", $("#cursoCategoria").val());
        payload.set("ModalidadCursoId", $("#cursoModalidad").val());
        payload.set("Descripcion", $("#cursoDescripcion").val().trim());

        try {
            const url = isEdit ? "/Curso/Update" : "/Curso/Create";
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
            button.disabled = false;
        }
    });
});