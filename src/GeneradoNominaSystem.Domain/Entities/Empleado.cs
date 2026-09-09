using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.ValueObjects;

namespace GeneradoNominaSystem.Domain.Entities;

public class Empleado : EntityBase
{
    public Guid EmpresaId { get; private set; }

    public TipoDocumentoIdentidad TipoDocumento { get; private set; }

    public string NumeroDocumento { get; private set; }

    public string Nombres { get; private set; }

    public string Apellidos { get; private set; }

    public string? Email { get; private set; }

    public string? Telefono { get; private set; }

    public Direccion? Direccion { get; private set; }

    public DateTime FechaIngreso { get; private set; }

    public string Cargo { get; private set; }

    public string? Departamento { get; private set; }

    public TipoContrato TipoContrato { get; private set; }

    public Dinero SalarioBase { get; private set; }

    public EstadoEmpleado Estado { get; private set; } = EstadoEmpleado.Activo;

    protected Empleado()
    {
        NumeroDocumento = string.Empty;
        Nombres = string.Empty;
        Apellidos = string.Empty;
        Cargo = string.Empty;
        SalarioBase = Dinero.Cero();
    }

    public Empleado(
        Guid empresaId,
        TipoDocumentoIdentidad tipoDocumento,
        string numeroDocumento,
        string nombres,
        string apellidos,
        DateTime fechaIngreso,
        string cargo,
        Dinero salarioBase,
        TipoContrato tipoContrato = TipoContrato.Indefinido)
    {
        if (empresaId == Guid.Empty)
        {
            throw new ReglaNegocioException("El empleado debe pertenecer a una empresa.");
        }

        if (string.IsNullOrWhiteSpace(numeroDocumento))
        {
            throw new ReglaNegocioException("El número de documento es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(nombres))
        {
            throw new ReglaNegocioException("Los nombres son obligatorios.");
        }

        if (string.IsNullOrWhiteSpace(apellidos))
        {
            throw new ReglaNegocioException("Los apellidos son obligatorios.");
        }

        if (string.IsNullOrWhiteSpace(cargo))
        {
            throw new ReglaNegocioException("El cargo es obligatorio.");
        }

        if (salarioBase is null)
        {
            throw new ReglaNegocioException("El salario base es obligatorio.");
        }

        if (salarioBase.Monto < 0m)
        {
            throw new ReglaNegocioException("El salario base no puede ser negativo.");
        }

        if (fechaIngreso.Date > DateTime.UtcNow.Date)
        {
            throw new ReglaNegocioException("La fecha de ingreso no puede ser futura.");
        }

        EmpresaId = empresaId;
        TipoDocumento = tipoDocumento;
        NumeroDocumento = numeroDocumento.Trim();
        Nombres = nombres.Trim();
        Apellidos = apellidos.Trim();
        FechaIngreso = fechaIngreso;
        Cargo = cargo.Trim();
        SalarioBase = salarioBase;
        TipoContrato = tipoContrato;
    }

    public string NombreCompleto => $"{Nombres} {Apellidos}";

    public void ActualizarContacto(string? email, string? telefono, Direccion? direccion)
    {
        Email = email?.Trim();
        Telefono = telefono?.Trim();
        Direccion = direccion;
        MarcarModificacion();
    }

    public void ActualizarSalario(Dinero nuevoSalario)
    {
        if (nuevoSalario is null)
        {
            throw new ReglaNegocioException("El salario base es obligatorio.");
        }

        if (nuevoSalario.Monto < 0m)
        {
            throw new ReglaNegocioException("El salario base no puede ser negativo.");
        }

        SalarioBase = nuevoSalario;
        MarcarModificacion();
    }

    public void ActualizarDatosLaborales(string cargo, string? departamentoArea)
    {
        if (string.IsNullOrWhiteSpace(cargo))
        {
            throw new ReglaNegocioException("El cargo es obligatorio.");
        }

        Cargo = cargo.Trim();
        Departamento = departamentoArea?.Trim();
        MarcarModificacion();
    }

    public void CambiarEstado(EstadoEmpleado nuevoEstado)
    {
        Estado = nuevoEstado;
        MarcarModificacion();
    }
}
