using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeneradoNominaSystem.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConceptosNomina",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Subtipo = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    EsPorcentaje = table.Column<bool>(type: "INTEGER", nullable: false),
                    PorcentajeBase = table.Column<decimal>(type: "TEXT", nullable: true),
                    ValorFijo_Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ValorFijo_Moneda = table.Column<string>(type: "TEXT", maxLength: 3, nullable: true),
                    FormulaCalculo = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Orden = table.Column<int>(type: "INTEGER", nullable: false),
                    RequiereBase = table.Column<bool>(type: "INTEGER", nullable: false),
                    AfectaBase = table.Column<bool>(type: "INTEGER", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConceptosNomina", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Empresas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RazonSocial = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    NombreComercial = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    Nit = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Direccion = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Ciudad = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Departamento = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Pais = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CodigoPostal = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Telefono = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    LogoRuta = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    NitFiscal = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    RazonSocialFiscal = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    DigitoVerificacion = table.Column<string>(type: "TEXT", maxLength: 5, nullable: true),
                    RegimenTributario = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    ResponsableIva = table.Column<bool>(type: "INTEGER", nullable: false),
                    RepresentanteLegal = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Documentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    ReferenciaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    NumeroDocumento = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Formato = table.Column<int>(type: "INTEGER", nullable: false),
                    RutaArchivo = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    TamanoBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    FechaGeneracion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GeneradoPor = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documentos_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Empleados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TipoDocumento = table.Column<int>(type: "INTEGER", nullable: false),
                    NumeroDocumento = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Nombres = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Apellidos = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Telefono = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Direccion = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Ciudad = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Departamento = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Pais = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CodigoPostal = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    FechaIngreso = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Cargo = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Empleado_Departamento = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    TipoContrato = table.Column<int>(type: "INTEGER", nullable: false),
                    SalarioBase_Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SalarioBase_Moneda = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empleados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Empleados_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PeriodosNomina",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Anio = table.Column<int>(type: "INTEGER", nullable: false),
                    Mes = table.Column<int>(type: "INTEGER", nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodosNomina", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeriodosNomina_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlantillasCotizacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Doc_MostrarLogo = table.Column<bool>(type: "INTEGER", nullable: false),
                    Doc_LogoPosicion = table.Column<int>(type: "INTEGER", nullable: false),
                    Doc_Encabezado = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Doc_PiePagina = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Doc_MostrarFirma = table.Column<bool>(type: "INTEGER", nullable: false),
                    Doc_TextoFirma = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Doc_NumeracionAutomatica = table.Column<bool>(type: "INTEGER", nullable: false),
                    Doc_Prefijo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Doc_SiguienteNumero = table.Column<int>(type: "INTEGER", nullable: false),
                    Doc_FormatoFecha = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Doc_SimboloMoneda = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Doc_DecimalesMoneda = table.Column<int>(type: "INTEGER", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantillasCotizacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlantillasCotizacion_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlantillasNomina",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Doc_MostrarLogo = table.Column<bool>(type: "INTEGER", nullable: false),
                    Doc_LogoPosicion = table.Column<int>(type: "INTEGER", nullable: false),
                    Doc_Encabezado = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Doc_PiePagina = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Doc_MostrarFirma = table.Column<bool>(type: "INTEGER", nullable: false),
                    Doc_TextoFirma = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Doc_NumeracionAutomatica = table.Column<bool>(type: "INTEGER", nullable: false),
                    Doc_Prefijo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Doc_SiguienteNumero = table.Column<int>(type: "INTEGER", nullable: false),
                    Doc_FormatoFecha = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Doc_SimboloMoneda = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Doc_DecimalesMoneda = table.Column<int>(type: "INTEGER", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantillasNomina", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlantillasNomina_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductosServicios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Unidad = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PrecioUnitario_Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrecioUnitario_Moneda = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductosServicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductosServicios_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cotizaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClienteNombre = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    ClienteDocumento = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ClienteEmail = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ClienteTelefono = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    NumeroCotizacion = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaVigencia = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    Observaciones = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Subtotal_Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Subtotal_Moneda = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    TotalDescuentos_Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDescuentos_Moneda = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    TotalNeto_Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalNeto_Moneda = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    Moneda = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    PlantillaCotizacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cotizaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cotizaciones_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cotizaciones_PlantillasCotizacion_PlantillaCotizacionId",
                        column: x => x.PlantillaCotizacionId,
                        principalTable: "PlantillasCotizacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Nominas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EmpleadoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PeriodoNominaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlantillaNominaId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaCalculo = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaAprobacion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Observaciones = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    SubtotalDevengos_Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubtotalDevengos_Moneda = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    SubtotalDeducciones_Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubtotalDeducciones_Moneda = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    TotalNeto_Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalNeto_Moneda = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nominas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Nominas_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Nominas_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Nominas_PeriodosNomina_PeriodoNominaId",
                        column: x => x.PeriodoNominaId,
                        principalTable: "PeriodosNomina",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Nominas_PlantillasNomina_PlantillaNominaId",
                        column: x => x.PlantillaNominaId,
                        principalTable: "PlantillasNomina",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PlantillasConcepto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlantillaNominaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConceptoNominaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Orden = table.Column<int>(type: "INTEGER", nullable: false),
                    Obligatorio = table.Column<bool>(type: "INTEGER", nullable: false),
                    ValorPorDefecto_Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ValorPorDefecto_Moneda = table.Column<string>(type: "TEXT", maxLength: 3, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantillasConcepto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlantillasConcepto_ConceptosNomina_ConceptoNominaId",
                        column: x => x.ConceptoNominaId,
                        principalTable: "ConceptosNomina",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlantillasConcepto_PlantillasNomina_PlantillaNominaId",
                        column: x => x.PlantillaNominaId,
                        principalTable: "PlantillasNomina",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetallesCotizacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CotizacionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductoServicioId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Cantidad = table.Column<decimal>(type: "TEXT", nullable: false),
                    PrecioUnitario_Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrecioUnitario_Moneda = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    DescuentoPorcentaje = table.Column<decimal>(type: "TEXT", nullable: true),
                    Subtotal_Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Subtotal_Moneda = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    Orden = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesCotizacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesCotizacion_Cotizaciones_CotizacionId",
                        column: x => x.CotizacionId,
                        principalTable: "Cotizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesCotizacion_ProductosServicios_ProductoServicioId",
                        column: x => x.ProductoServicioId,
                        principalTable: "ProductosServicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DetallesNomina",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    NominaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConceptoNominaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Valor_Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Valor_Moneda = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    Cantidad = table.Column<decimal>(type: "TEXT", nullable: true),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Orden = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesNomina", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesNomina_ConceptosNomina_ConceptoNominaId",
                        column: x => x.ConceptoNominaId,
                        principalTable: "ConceptosNomina",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesNomina_Nominas_NominaId",
                        column: x => x.NominaId,
                        principalTable: "Nominas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_EmpresaId_NumeroCotizacion",
                table: "Cotizaciones",
                columns: new[] { "EmpresaId", "NumeroCotizacion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_PlantillaCotizacionId",
                table: "Cotizaciones",
                column: "PlantillaCotizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesCotizacion_CotizacionId",
                table: "DetallesCotizacion",
                column: "CotizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesCotizacion_ProductoServicioId",
                table: "DetallesCotizacion",
                column: "ProductoServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesNomina_ConceptoNominaId",
                table: "DetallesNomina",
                column: "ConceptoNominaId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesNomina_NominaId",
                table: "DetallesNomina",
                column: "NominaId");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_EmpresaId",
                table: "Documentos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_EmpresaId_NumeroDocumento",
                table: "Empleados",
                columns: new[] { "EmpresaId", "NumeroDocumento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Empresas_Nit",
                table: "Empresas",
                column: "Nit",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Nominas_EmpleadoId",
                table: "Nominas",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Nominas_EmpresaId",
                table: "Nominas",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Nominas_PeriodoNominaId",
                table: "Nominas",
                column: "PeriodoNominaId");

            migrationBuilder.CreateIndex(
                name: "IX_Nominas_PlantillaNominaId",
                table: "Nominas",
                column: "PlantillaNominaId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodosNomina_EmpresaId",
                table: "PeriodosNomina",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasConcepto_ConceptoNominaId",
                table: "PlantillasConcepto",
                column: "ConceptoNominaId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasConcepto_PlantillaNominaId",
                table: "PlantillasConcepto",
                column: "PlantillaNominaId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasCotizacion_EmpresaId",
                table: "PlantillasCotizacion",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasNomina_EmpresaId",
                table: "PlantillasNomina",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosServicios_EmpresaId",
                table: "ProductosServicios",
                column: "EmpresaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetallesCotizacion");

            migrationBuilder.DropTable(
                name: "DetallesNomina");

            migrationBuilder.DropTable(
                name: "Documentos");

            migrationBuilder.DropTable(
                name: "PlantillasConcepto");

            migrationBuilder.DropTable(
                name: "Cotizaciones");

            migrationBuilder.DropTable(
                name: "ProductosServicios");

            migrationBuilder.DropTable(
                name: "Nominas");

            migrationBuilder.DropTable(
                name: "ConceptosNomina");

            migrationBuilder.DropTable(
                name: "PlantillasCotizacion");

            migrationBuilder.DropTable(
                name: "Empleados");

            migrationBuilder.DropTable(
                name: "PeriodosNomina");

            migrationBuilder.DropTable(
                name: "PlantillasNomina");

            migrationBuilder.DropTable(
                name: "Empresas");
        }
    }
}
