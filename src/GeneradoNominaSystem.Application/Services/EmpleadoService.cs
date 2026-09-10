using FluentValidation;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Interfaces;
using GeneradoNominaSystem.Domain.Comun;
using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.Interfaces;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;
using GeneradoNominaSystem.Domain.ValueObjects;

namespace GeneradoNominaSystem.Application.Services;

public sealed class EmpleadoService : IEmpleadoService
{
    private readonly IEmpleadoRepository _empleados;
    private readonly IEmpresaRepository _empresas;
    private readonly INominaRepository _nominas;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<EmpleadoDto> _validator;

    public EmpleadoService(
        IEmpleadoRepository empleados,
        IEmpresaRepository empresas,
        INominaRepository nominas,
        IUnitOfWork uow,
        IValidator<EmpleadoDto> validator)
    {
        _empleados = empleados;
        _empresas = empresas;
        _nominas = nominas;
        _uow = uow;
        _validator = validator;
    }

    public async Task<IReadOnlyList<EmpleadoDto>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default)
    {
        var entidades = await _empleados.ListarPorEmpresaAsync(empresaId, ct);
        var lista = new List<EmpleadoDto>();
        foreach (var e in entidades)
        {
            lista.Add(await MapearAsync(e, ct));
        }

        return lista;
    }

    public async Task<EmpleadoDto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var entidad = await _empleados.ObtenerPorIdAsync(id, ct);
        return entidad is null ? null : await MapearAsync(entidad, ct);
    }

    public async Task<EmpleadoDto> CrearAsync(EmpleadoDto dto, CancellationToken ct = default)
    {
        NormalizarDesconocidos(dto);
        await ValidarAsync(dto, ct);
        await ValidarEmpresaExisteAsync(dto.EmpresaId, ct);
        await ValidarDocumentoUnicoAsync(dto.EmpresaId, dto.NumeroDocumento.Trim(), null, ct);

        var entidad = ConstruirEntidad(dto);
        await _empleados.AgregarAsync(entidad, ct);
        await _uow.GuardarCambiosAsync(ct);

        return await MapearAsync(entidad, ct);
    }

    public async Task<EmpleadoDto> ActualizarAsync(EmpleadoDto dto, CancellationToken ct = default)
    {
        NormalizarDesconocidos(dto);
        await ValidarAsync(dto, ct);

        var entidad = await _empleados.ObtenerPorIdAsync(dto.Id, ct);
        if (entidad is null)
        {
            throw new ReglaNegocioException("El empleado no existe.");
        }

        await ValidarDocumentoUnicoAsync(entidad.EmpresaId, dto.NumeroDocumento.Trim(), entidad.Id, ct);

        entidad.ActualizarDatosLaborales(dto.Cargo, dto.DepartamentoArea);
        entidad.ActualizarContacto(dto.Email, dto.Telefono, ConstruirDireccion(dto));
        entidad.ActualizarSalario(new Dinero(dto.SalarioBaseMonto, dto.SalarioBaseMoneda));
        _empleados.Actualizar(entidad);
        await _uow.GuardarCambiosAsync(ct);

        return await MapearAsync(entidad, ct);
    }

    public async Task CambiarEstadoAsync(Guid id, EstadoEmpleado nuevoEstado, CancellationToken ct = default)
    {
        var entidad = await _empleados.ObtenerPorIdAsync(id, ct);
        if (entidad is null)
        {
            throw new ReglaNegocioException("El empleado no existe.");
        }

        entidad.CambiarEstado(nuevoEstado);
        _empleados.Actualizar(entidad);
        await _uow.GuardarCambiosAsync(ct);
    }

    public async Task EliminarAsync(Guid id, CancellationToken ct = default)
    {
        var entidad = await _empleados.ObtenerPorIdAsync(id, ct);
        if (entidad is null)
        {
            throw new ReglaNegocioException("El empleado no existe.");
        }

        var nominas = await _nominas.ListarPorEmpleadoAsync(id, ct);
        if (nominas.Count > 0)
        {
            throw new ReglaNegocioException(
                "No se puede eliminar el empleado porque tiene nóminas registradas. " +
                "Desactívalo en su lugar.");
        }

        _empleados.Eliminar(entidad);
        await _uow.GuardarCambiosAsync(ct);
    }

    private async Task ValidarAsync(EmpleadoDto dto, CancellationToken ct)
    {
        var resultado = await _validator.ValidateAsync(dto, ct);
        if (!resultado.IsValid)
        {
            var errores = string.Join("; ", resultado.Errors.Select(e => e.ErrorMessage));
            throw new ValidationException($"Datos de empleado inválidos: {errores}");
        }
    }

    private async Task ValidarEmpresaExisteAsync(Guid empresaId, CancellationToken ct)
    {
        var empresa = await _empresas.ObtenerPorIdAsync(empresaId, ct);
        if (empresa is null)
        {
            throw new ReglaNegocioException("La empresa no existe.");
        }
    }

    private async Task ValidarDocumentoUnicoAsync(Guid empresaId, string numeroDocumento, Guid? excluirId, CancellationToken ct)
    {
        var existente = await _empleados.ObtenerPorDocumentoAsync(empresaId, numeroDocumento, ct);
        if (existente is not null && existente.Id != excluirId)
        {
            throw new ReglaNegocioException($"Ya existe un empleado con documento {numeroDocumento} en esta empresa.");
        }
    }

    private static Empleado ConstruirEntidad(EmpleadoDto dto)
    {
        var empleado = new Empleado(
            dto.EmpresaId,
            dto.TipoDocumento,
            dto.NumeroDocumento,
            dto.Nombres,
            dto.Apellidos,
            dto.FechaIngreso,
            dto.Cargo,
            new Dinero(dto.SalarioBaseMonto, dto.SalarioBaseMoneda),
            dto.TipoContrato);

        empleado.ActualizarDatosLaborales(dto.Cargo, dto.DepartamentoArea);
        empleado.ActualizarContacto(dto.Email, dto.Telefono, ConstruirDireccion(dto));
        return empleado;
    }

    private static void NormalizarDesconocidos(EmpleadoDto dto)
    {
        dto.Nombres = DatoDesconocido.Normalizar(dto.Nombres) ?? string.Empty;
        dto.Apellidos = DatoDesconocido.Normalizar(dto.Apellidos) ?? string.Empty;
        dto.Cargo = DatoDesconocido.Normalizar(dto.Cargo) ?? string.Empty;
        dto.DepartamentoArea = DatoDesconocido.Normalizar(dto.DepartamentoArea);
        dto.Email = DatoDesconocido.Normalizar(dto.Email);
        dto.Telefono = DatoDesconocido.Normalizar(dto.Telefono);
        dto.DireccionCompleta = DatoDesconocido.Normalizar(dto.DireccionCompleta);
        dto.Ciudad = DatoDesconocido.Normalizar(dto.Ciudad);
        dto.Departamento = DatoDesconocido.Normalizar(dto.Departamento);
        dto.Pais = DatoDesconocido.Normalizar(dto.Pais) ?? "Colombia";
        dto.CodigoPostal = DatoDesconocido.Normalizar(dto.CodigoPostal);
    }

    private static Direccion? ConstruirDireccion(EmpleadoDto dto)    {
        if (string.IsNullOrWhiteSpace(dto.DireccionCompleta)
            || string.IsNullOrWhiteSpace(dto.Ciudad)
            || string.IsNullOrWhiteSpace(dto.Departamento))
        {
            return null;
        }

        return new Direccion(
            dto.DireccionCompleta!,
            dto.Ciudad!,
            dto.Departamento!,
            string.IsNullOrWhiteSpace(dto.Pais) ? "Colombia" : dto.Pais,
            dto.CodigoPostal);
    }

    private async Task<EmpleadoDto> MapearAsync(Empleado e, CancellationToken ct)
    {
        var empresa = await _empresas.ObtenerPorIdAsync(e.EmpresaId, ct);
        return new EmpleadoDto
        {
            Id = e.Id,
            EmpresaId = e.EmpresaId,
            EmpresaNombre = empresa?.NombreComercial ?? string.Empty,
            TipoDocumento = e.TipoDocumento,
            NumeroDocumento = e.NumeroDocumento,
            Nombres = e.Nombres,
            Apellidos = e.Apellidos,
            Email = e.Email,
            Telefono = e.Telefono,
            DireccionCompleta = e.Direccion?.DireccionCompleta,
            Ciudad = e.Direccion?.Ciudad,
            Departamento = e.Direccion?.Departamento,
            Pais = e.Direccion?.Pais ?? "Colombia",
            CodigoPostal = e.Direccion?.CodigoPostal,
            FechaIngreso = e.FechaIngreso,
            Cargo = e.Cargo,
            DepartamentoArea = e.Departamento,
            TipoContrato = e.TipoContrato,
            SalarioBaseMonto = e.SalarioBase.Monto,
            SalarioBaseMoneda = e.SalarioBase.Moneda,
            Estado = e.Estado,
        };
    }
}
