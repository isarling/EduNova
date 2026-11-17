$(function () {
    const tableEl = $("#userPermissionsTable");
    if (!tableEl.length) return;

    let busy = false;

    tableEl.DataTable({
        language: {
            search: "Buscar:",
            lengthMenu: "Mostrar _MENU_ registros",
            info: "Mostrando _START_ a _END_ de _TOTAL_",
            infoEmpty: "Sin registros",
            infoFiltered: "(de _MAX_ registros)",
            zeroRecords: "No se encontraron resultados",
            emptyTable: "No hay permisos registrados",
            paginate: {
                next: "Siguiente",
                previous: "Anterior"
            }
        },
        order: [[0, "asc"]],
        columnDefs: [
            { orderable: false, searchable: false, targets: 3 }
        ],
        pageLength: 10
    });

    async function execute(button, url, payload, params) {
        if (busy) return;

        busy = true;
        button.disabled = true;

        try {
            const response = await axios.post(url, payload, {
                params: params
            });

            const data = response.data;

            if (!data || data.success !== true) {
                throw new Error(
                    data?.message ||
                    "No se pudo completar la operación. Revisa tu sesión."
                );
            }

            await Swal.fire({
                title: "Guardado",
                text: data.message || "Cambio guardado correctamente.",
                icon: "success",
                timer: 1200,
                showConfirmButton: false
            });

            window.location.reload();
        } catch (error) {
            const data = error.response?.data;
            const message = typeof data === "string"
                ? data
                : data?.message || data?.errorMessage ||
                error.message || "No se pudo guardar el cambio.";

            Swal.fire("Error", message, "error");
        } finally {
            busy = false;
            button.disabled = false;
        }
    }

    $("#btnUpdateRole").on("click", async function () {
        const roleId = Number($("#userRoleSelect").val());
        if (!roleId) return;

        await execute(
            this,
            "/Admin/UpdateRole",
            { roleId: roleId },
            { userId: Number(this.dataset.userId) }
        );
    });

    tableEl.on(
        "click",
        ".btn-user-grant, .btn-user-deny",
        async function () {
            const isGranted = this.classList.contains("btn-user-grant");

            await execute(
                this,
                "/Admin/AssignPermissionToUser",
                {
                    permissionId: Number(this.dataset.permissionId),
                    isGranted: isGranted
                },
                { userId: Number(this.dataset.userId) }
            );
        }
    );

    tableEl.on("click", ".btn-user-remove", async function () {
        await execute(
            this,
            "/Admin/RemoveUserPermission",
            null,
            {
                userId: Number(this.dataset.userId),
                permissionId: Number(this.dataset.permissionId)
            }
        );
    });
});