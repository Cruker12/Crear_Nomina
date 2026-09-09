using FluentAssertions;
using GeneradoNominaSystem.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GeneradoNominaSystem.Infrastructure.Tests.Repositories;

public sealed class MigracionTests : IDisposable
{
    private readonly string _rutaBd = Path.Combine(Path.GetTempPath(), $"gns-mig-{Guid.NewGuid():N}.db");

    private static readonly string[] TablasEsperadas =
    [
        "Empresas", "Empleados", "PeriodosNomina", "ConceptosNomina",
        "Nominas", "DetallesNomina", "PlantillasNomina", "PlantillasConcepto",
        "Cotizaciones", "DetallesCotizacion", "ProductosServicios",
        "PlantillasCotizacion", "Documentos",
    ];

    private AppDbContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={_rutaBd}")
            .Options;

        return new AppDbContext(options);
    }

    private static IReadOnlyList<string> ListarTablas(string rutaBd)
    {
        using var conn = new SqliteConnection($"Data Source={rutaBd}");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' AND name <> '__EFMigrationsHistory';";
        using var reader = cmd.ExecuteReader();
        var tablas = new List<string>();
        while (reader.Read())
        {
            tablas.Add(reader.GetString(0));
        }

        return tablas;
    }

    [Fact]
    public void Migrate_DeberiaCrearLas13Tablas()
    {
        using var context = CrearContexto();
        context.Database.Migrate();

        var tablas = ListarTablas(_rutaBd);

        foreach (var esperada in TablasEsperadas)
        {
            tablas.Should().Contain(esperada);
        }
    }

    [Fact]
    public void Migrate_DowngradeACero_DeberiaEliminarTablas()
    {
        using var context = CrearContexto();
        context.Database.Migrate();
        ListarTablas(_rutaBd).Should().Contain("Empresas");

        context.GetService<IMigrator>().Migrate("0");

        ListarTablas(_rutaBd).Should().NotContain("Empresas");
    }

    public void Dispose()
    {
        try
        {
            if (File.Exists(_rutaBd))
            {
                File.Delete(_rutaBd);
            }
        }
        catch
        {
        }
    }
}
