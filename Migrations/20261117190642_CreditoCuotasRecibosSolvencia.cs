using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduNova.Migrations
{
    /// <inheritdoc />
    public partial class CreditoCuotasRecibosSolvencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Estudiantes_Cursos_CursoId",
                table: "Estudiantes");

            migrationBuilder.AddColumn<decimal>(
                name: "MinimoInicialAplicado",
                table: "Matriculas",
                type: "decimal(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "OperacionId",
                table: "Matriculas",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CursoId",
                table: "Estudiantes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Estudiantes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Descuento",
                table: "DetallesMatricula",
                type: "decimal(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaFin",
                table: "DetallesMatricula",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioCatalogo",
                table: "DetallesMatricula",
                type: "decimal(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "CategoriaCursoId",
                table: "Cursos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DuracionDias",
                table: "Cursos",
                type: "int",
                nullable: false,
                defaultValue: 90);

            migrationBuilder.AddColumn<int>(
                name: "ModalidadCursoId",
                table: "Cursos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Precio",
                table: "Cursos",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "BeneficiosDescuento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EstudianteId = table.Column<int>(type: "int", nullable: false),
                    MatriculaOrigenId = table.Column<int>(type: "int", nullable: false),
                    MatriculaDestinoId = table.Column<int>(type: "int", nullable: true),
                    Emitido = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Vence = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Porcentaje = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeneficiosDescuento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BeneficiosDescuento_Estudiantes_EstudianteId",
                        column: x => x.EstudianteId,
                        principalTable: "Estudiantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BeneficiosDescuento_Matriculas_MatriculaDestinoId",
                        column: x => x.MatriculaDestinoId,
                        principalTable: "Matriculas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BeneficiosDescuento_Matriculas_MatriculaOrigenId",
                        column: x => x.MatriculaOrigenId,
                        principalTable: "Matriculas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CategoriasCurso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasCurso", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConstanciasSolvencia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EstudianteId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NombreEstudiante = table.Column<string>(type: "nvarchar(301)", maxLength: 301, nullable: false),
                    TotalContratado = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    TotalPagado = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConstanciasSolvencia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConstanciasSolvencia_Estudiantes_EstudianteId",
                        column: x => x.EstudianteId,
                        principalTable: "Estudiantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cuotas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DetalleMatriculaId = table.Column<int>(type: "int", nullable: false),
                    Numero = table.Column<int>(type: "int", nullable: false),
                    Vencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Pagado = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cuotas", x => x.Id);
                    table.CheckConstraint("CK_Cuota_Montos", "[Monto] > 0 AND [Pagado] >= 0 AND ([Monto] - [Pagado]) >= 0");
                    table.ForeignKey(
                        name: "FK_Cuotas_DetallesMatricula_DetalleMatriculaId",
                        column: x => x.DetalleMatriculaId,
                        principalTable: "DetallesMatricula",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ModalidadesCurso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModalidadesCurso", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PoliticasCredito",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    DeudaMaxima = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    MaxCursosSinDeuda = table.Column<int>(type: "int", nullable: false),
                    MaxCursosConDeuda = table.Column<int>(type: "int", nullable: false),
                    PorcentajeInicial = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    AbonoMinimo = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    MaxCuotas = table.Column<int>(type: "int", nullable: false),
                    DiasEntreCuotas = table.Column<int>(type: "int", nullable: false),
                    DiasGracia = table.Column<int>(type: "int", nullable: false),
                    PorcentajeBeneficio = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    DiasVigenciaBeneficio = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticasCredito", x => x.Id);
                    table.CheckConstraint("CK_Politica_Unica", "[Id] = 1");
                });

            migrationBuilder.CreateTable(
                name: "RecibosPago",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PagoId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NombreEstudiante = table.Column<string>(type: "nvarchar(301)", maxLength: 301, nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    SaldoPosterior = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecibosPago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecibosPago_Pagos_PagoId",
                        column: x => x.PagoId,
                        principalTable: "Pagos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesSolvencia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConstanciaSolvenciaId = table.Column<int>(type: "int", nullable: false),
                    MatriculaId = table.Column<int>(type: "int", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Pagado = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesSolvencia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesSolvencia_ConstanciasSolvencia_ConstanciaSolvenciaId",
                        column: x => x.ConstanciaSolvenciaId,
                        principalTable: "ConstanciasSolvencia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesSolvencia_Matriculas_MatriculaId",
                        column: x => x.MatriculaId,
                        principalTable: "Matriculas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesPago",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PagoId = table.Column<int>(type: "int", nullable: false),
                    CuotaId = table.Column<int>(type: "int", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesPago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesPago_Cuotas_CuotaId",
                        column: x => x.CuotaId,
                        principalTable: "Cuotas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesPago_Pagos_PagoId",
                        column: x => x.PagoId,
                        principalTable: "Pagos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesRecibo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReciboPagoId = table.Column<int>(type: "int", nullable: false),
                    Curso = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NumeroCuota = table.Column<int>(type: "int", nullable: false),
                    Vencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesRecibo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesRecibo_RecibosPago_ReciboPagoId",
                        column: x => x.ReciboPagoId,
                        principalTable: "RecibosPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Pago_Positivo",
                table: "Pagos",
                sql: "[Monto] > 0");

            migrationBuilder.CreateIndex(
                name: "IX_Matriculas_OperacionId",
                table: "Matriculas",
                column: "OperacionId",
                unique: true,
                filter: "[OperacionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Estudiantes_UsuarioId",
                table: "Estudiantes",
                column: "UsuarioId",
                unique: true,
                filter: "[UsuarioId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_CategoriaCursoId",
                table: "Cursos",
                column: "CategoriaCursoId");

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_ModalidadCursoId",
                table: "Cursos",
                column: "ModalidadCursoId");

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiosDescuento_EstudianteId",
                table: "BeneficiosDescuento",
                column: "EstudianteId");

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiosDescuento_MatriculaDestinoId",
                table: "BeneficiosDescuento",
                column: "MatriculaDestinoId",
                unique: true,
                filter: "[MatriculaDestinoId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiosDescuento_MatriculaOrigenId",
                table: "BeneficiosDescuento",
                column: "MatriculaOrigenId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasCurso_Nombre",
                table: "CategoriasCurso",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConstanciasSolvencia_EstudianteId",
                table: "ConstanciasSolvencia",
                column: "EstudianteId");

            migrationBuilder.CreateIndex(
                name: "IX_Cuotas_DetalleMatriculaId_Numero",
                table: "Cuotas",
                columns: new[] { "DetalleMatriculaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPago_CuotaId",
                table: "DetallesPago",
                column: "CuotaId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPago_PagoId",
                table: "DetallesPago",
                column: "PagoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesRecibo_ReciboPagoId",
                table: "DetallesRecibo",
                column: "ReciboPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesSolvencia_ConstanciaSolvenciaId",
                table: "DetallesSolvencia",
                column: "ConstanciaSolvenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesSolvencia_MatriculaId",
                table: "DetallesSolvencia",
                column: "MatriculaId");

            migrationBuilder.CreateIndex(
                name: "IX_ModalidadesCurso_Nombre",
                table: "ModalidadesCurso",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecibosPago_PagoId",
                table: "RecibosPago",
                column: "PagoId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Cursos_CategoriasCurso_CategoriaCursoId",
                table: "Cursos",
                column: "CategoriaCursoId",
                principalTable: "CategoriasCurso",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cursos_ModalidadesCurso_ModalidadCursoId",
                table: "Cursos",
                column: "ModalidadCursoId",
                principalTable: "ModalidadesCurso",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Estudiantes_Cursos_CursoId",
                table: "Estudiantes",
                column: "CursoId",
                principalTable: "Cursos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Estudiantes_Usuarios_UsuarioId",
                table: "Estudiantes",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Conserva matrículas y pagos del avance anterior. El plan antiguo se
            // representa con una cuota por curso y los pagos se distribuyen por orden.
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM Matriculas m OUTER APPLY
 (SELECT SUM(Costo) Total FROM DetallesMatricula d WHERE d.MatriculaId=m.Id) t
 OUTER APPLY (SELECT SUM(Monto) Pagado FROM Pagos p WHERE p.MatriculaId=m.Id) a
 WHERE COALESCE(a.Pagado,0)>COALESCE(t.Total,0))
 THROW 51000, 'Hay pagos históricos que superan el costo contratado. Revise los datos antes de migrar.', 1;
UPDATE d SET PrecioCatalogo=d.Costo,
 FechaFin=DATEADD(DAY,c.DuracionDias,CAST(DATEADD(HOUR,-6,m.Fecha) AS date))
 FROM DetallesMatricula d JOIN Matriculas m ON m.Id=d.MatriculaId JOIN Cursos c ON c.Id=d.CursoId;
INSERT INTO Cuotas (DetalleMatriculaId,Numero,Vencimiento,Monto,Pagado)
 SELECT d.Id,1,DATEADD(DAY,30,CAST(DATEADD(HOUR,-6,m.Fecha) AS date)),d.Costo,0
 FROM DetallesMatricula d JOIN Matriculas m ON m.Id=d.MatriculaId WHERE d.Costo>0;
;WITH pagosOrdenados AS
 (SELECT p.Id,p.MatriculaId,p.Monto,
 SUM(p.Monto) OVER(PARTITION BY p.MatriculaId ORDER BY p.Fecha,p.Id ROWS UNBOUNDED PRECEDING) Hasta FROM Pagos p),
 cuotasOrdenadas AS
 (SELECT q.Id,d.MatriculaId,q.Monto,
 SUM(q.Monto) OVER(PARTITION BY d.MatriculaId ORDER BY d.Id,q.Numero ROWS UNBOUNDED PRECEDING) Hasta
 FROM Cuotas q JOIN DetallesMatricula d ON d.Id=q.DetalleMatriculaId),
 intersecciones AS
 (SELECT p.Id PagoId,q.Id CuotaId,
 (CASE WHEN p.Hasta<q.Hasta THEN p.Hasta ELSE q.Hasta END)-
 (CASE WHEN p.Hasta-p.Monto>q.Hasta-q.Monto THEN p.Hasta-p.Monto ELSE q.Hasta-q.Monto END) Aplicado
 FROM pagosOrdenados p JOIN cuotasOrdenadas q ON q.MatriculaId=p.MatriculaId)
INSERT INTO DetallesPago (PagoId,CuotaId,Monto) SELECT PagoId,CuotaId,Aplicado FROM intersecciones WHERE Aplicado>0;
UPDATE q SET Pagado=x.Total FROM Cuotas q CROSS APPLY
 (SELECT COALESCE(SUM(d.Monto),0) Total FROM DetallesPago d WHERE d.CuotaId=q.Id) x;
;WITH pagosOrdenados AS
 (SELECT p.*,SUM(p.Monto) OVER(PARTITION BY p.MatriculaId ORDER BY p.Fecha,p.Id ROWS UNBOUNDED PRECEDING) Acumulado FROM Pagos p)
INSERT INTO RecibosPago (PagoId,Fecha,NombreEstudiante,Monto,SaldoPosterior)
 SELECT p.Id,p.Fecha,CONCAT(e.Nombre,' ',e.Apellido),p.Monto,t.Total-p.Acumulado
 FROM pagosOrdenados p JOIN Matriculas m ON m.Id=p.MatriculaId JOIN Estudiantes e ON e.Id=m.EstudianteId
 CROSS APPLY (SELECT COALESCE(SUM(Costo),0) Total FROM DetallesMatricula d WHERE d.MatriculaId=m.Id) t;
INSERT INTO DetallesRecibo (ReciboPagoId,Curso,NumeroCuota,Vencimiento,Monto)
 SELECT r.Id,c.Nombre,q.Numero,q.Vencimiento,d.Monto FROM DetallesPago d
 JOIN RecibosPago r ON r.PagoId=d.PagoId JOIN Cuotas q ON q.Id=d.CuotaId
 JOIN DetallesMatricula dm ON dm.Id=q.DetalleMatriculaId JOIN Cursos c ON c.Id=dm.CursoId;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cursos_CategoriasCurso_CategoriaCursoId",
                table: "Cursos");

            migrationBuilder.DropForeignKey(
                name: "FK_Cursos_ModalidadesCurso_ModalidadCursoId",
                table: "Cursos");

            migrationBuilder.DropForeignKey(
                name: "FK_Estudiantes_Cursos_CursoId",
                table: "Estudiantes");

            migrationBuilder.DropForeignKey(
                name: "FK_Estudiantes_Usuarios_UsuarioId",
                table: "Estudiantes");

            migrationBuilder.DropTable(
                name: "BeneficiosDescuento");

            migrationBuilder.DropTable(
                name: "CategoriasCurso");

            migrationBuilder.DropTable(
                name: "DetallesPago");

            migrationBuilder.DropTable(
                name: "DetallesRecibo");

            migrationBuilder.DropTable(
                name: "DetallesSolvencia");

            migrationBuilder.DropTable(
                name: "ModalidadesCurso");

            migrationBuilder.DropTable(
                name: "PoliticasCredito");

            migrationBuilder.DropTable(
                name: "Cuotas");

            migrationBuilder.DropTable(
                name: "RecibosPago");

            migrationBuilder.DropTable(
                name: "ConstanciasSolvencia");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Pago_Positivo",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Matriculas_OperacionId",
                table: "Matriculas");

            migrationBuilder.DropIndex(
                name: "IX_Estudiantes_UsuarioId",
                table: "Estudiantes");

            migrationBuilder.DropIndex(
                name: "IX_Cursos_CategoriaCursoId",
                table: "Cursos");

            migrationBuilder.DropIndex(
                name: "IX_Cursos_ModalidadCursoId",
                table: "Cursos");

            migrationBuilder.DropColumn(
                name: "MinimoInicialAplicado",
                table: "Matriculas");

            migrationBuilder.DropColumn(
                name: "OperacionId",
                table: "Matriculas");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Estudiantes");

            migrationBuilder.DropColumn(
                name: "Descuento",
                table: "DetallesMatricula");

            migrationBuilder.DropColumn(
                name: "FechaFin",
                table: "DetallesMatricula");

            migrationBuilder.DropColumn(
                name: "PrecioCatalogo",
                table: "DetallesMatricula");

            migrationBuilder.DropColumn(
                name: "CategoriaCursoId",
                table: "Cursos");

            migrationBuilder.DropColumn(
                name: "DuracionDias",
                table: "Cursos");

            migrationBuilder.DropColumn(
                name: "ModalidadCursoId",
                table: "Cursos");

            migrationBuilder.DropColumn(
                name: "Precio",
                table: "Cursos");

            migrationBuilder.AlterColumn<int>(
                name: "CursoId",
                table: "Estudiantes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Estudiantes_Cursos_CursoId",
                table: "Estudiantes",
                column: "CursoId",
                principalTable: "Cursos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
