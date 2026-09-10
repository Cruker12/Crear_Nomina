using FluentValidation;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Interfaces;
using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.Interfaces;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;
using GeneradoNominaSystem.Domain.ValueObjects;

namespace GeneradoNominaSystem.Application.Services;

public sealed class EmpresaService : IEmpresaService
{
    private readonly IEmpresaRepository _empresas;
    private readonly IEmpleadoRepository _empleados;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<EmpresaDto> _validator;

    public EmpresaService(
        IEmpresaRepository empresas,
        IEmpleadoRepository empleados,
        IUnitOfWork uow,
        IValidator<EmpresaDto> validator)
    {
        _empresas = empresas;
        _empleados = empleados;
        _uow = uow;
        _validator = validator;
    }

    public async Task<IReadOnlyList<EmpresaDto>> ListarAsync(CancellationToken ct = default)
    {
        var entidades = await _empresas.ListarAsync(ct);
        return entidades.Select(Mapear).ToList();
    }

    public async Task<EmpresaDto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var entidad = await _empresas.ObtenerPorIdAsync(id, ct);
        return entidad is null ? null : Mapear(entidad);
    }

    public async Task<EmpresaDto> CrearAsync(EmpresaDto dto, CancellationToken ct = default)
    {
        await ValidarAsync(dto, ct);

        var existente = await _empresas.ObtenerPorNitAsync(dto.Nit.Trim(), ct);
        if (existente is not null)
        {
            throw new ReglaNegocioException($"Ya existe una empresa con NIT {dto.Nit}.");
        }

        var entidad = ConstruirEntidad(dto);
        await _empresas.AgregarAsync(entidad, ct);
        await _uow.GuardarCambiosAsync(ct);

        return Mapear(entidad);
    }

    public async Task<EmpresaDto> ActualizarAsync(EmpresaDto dto, CancellationToken ct = default)
    {
        await ValidarAsync(dto, ct);

        var entidad = await _empresas.ObtenerPorIdAsync(dto.Id, ct);
        if (entidad is null)
        {
            throw new ReglaNegocioException("La empresa no existe.");
        }

        var otraConMismoNit = await _empresas.ObtenerPorNitAsync(dto.Nit.Trim(), ct);
        if (otraConMismoNit is not null && otraConMismoNit.Id != entidad.Id)
        {
            throw new ReglaNegocioException($"Ya existe otra empresa con NIT {dto.Nit}.");
        }

        entidad.ActualizarDatos(dto.RazonSocial, dto.NombreComercial, dto.Telefono, dto.Email);
        entidad.ActualizarLogo(dto.LogoRuta);
        _empresas.Actualizar(entidad);
        await _uow.GuardarCambiosAsync(ct);

        return Mapear(entidad);
    }

    public async Task DesactivarAsync(Guid id, CancellationToken ct = default)
    {
        var entidad = await _empresas.ObtenerPorIdAsync(id, ct);
        if (entidad is null)
        {
            throw new ReglaNegocioException("La empresa no existe.");
        }

        entidad.Desactivar();
        _empresas.Actualizar(entidad);
        await _uow.GuardarCambiosAsync(ct);
    }

    public async Task ActivarAsync(Guid id, CancellationToken ct = default)
    {
        var entidad = await _empresas.ObtenerPorIdAsync(id, ct);
        if (entidad is null)
        {
            throw new ReglaNegocioException("La empresa no existe.");
        }

        entidad.Activar();
        _empresas.Actualizar(entidad);
        await _uow.GuardarCambiosAsync(ct);
    }

    public async Task EliminarAsync(Guid id, CancellationToken ct = default)
    {
        var entidad = await _empresas.ObtenerPorIdAsync(id, ct);
        if (entidad is null)
        {
            throw new ReglaNegocioException("La empresa no existe.");
        }

        var empleados = await _empleados.ListarPorEmpresaAsync(id, ct);
        if (empleados.Count > 0)
        {
            throw new ReglaNegocioException(
                "No se puede eliminar la empresa porque tiene empleados registrados. " +
                "Elimina o reasigna sus empleados primero.");
        }

        _empresas.Eliminar(entidad);
        await _uow.GuardarCambiosAsync(ct);
    }

    private async Task ValidarAsync(EmpresaDto dto, CancellationToken ct)
    {
        var resultado = await _validator.ValidateAsync(dto, ct);
        if (!resultado.IsValid)
        {
            var errores = string.Join("; ", resultado.Errors.Select(e => e.ErrorMessage));
            throw new ValidationException($"Datos de empresa inválidos: {errores}");
        }
    }

    private static Empresa ConstruirEntidad(EmpresaDto dto)
    {
        var direccion = new Direccion(
            dto.DireccionCompleta,
            dto.Ciudad,
            dto.Departamento,
            string.IsNullOrWhiteSpace(dto.Pais) ? "Colombia" : dto.Pais,
            dto.CodigoPostal);

        var fiscales = new DatosFiscales(
            dto.Nit,
            dto.RazonSocial,
            dto.DigitoVerificacion,
            dto.RegimenTributario,
            dto.ResponsableIva,
            dto.RepresentanteLegal);

        return new Empresa(
            dto.RazonSocial,
            dto.NombreComercial,
            dto.Nit,
            direccion,
            fiscales,
            dto.Telefono,
            dto.Email,
            dto.LogoRuta);
    }

    private static EmpresaDto Mapear(Empresa e)
    {
        return new EmpresaDto
        {
            Id = e.Id,
            RazonSocial = e.RazonSocial,
            NombreComercial = e.NombreComercial,
            Nit = e.Nit,
            Telefono = e.Telefono,
            Email = e.Email,
            LogoRuta = e.LogoRuta,
            DireccionCompleta = e.Direccion.DireccionCompleta,
            Ciudad = e.Direccion.Ciudad,
            Departamento = e.Direccion.Departamento,
            Pais = e.Direccion.Pais,
            CodigoPostal = e.Direccion.CodigoPostal,
            DigitoVerificacion = e.DatosFiscales.DigitoVerificacion,
            RegimenTributario = e.DatosFiscales.RegimenTributario,
            ResponsableIva = e.DatosFiscales.ResponsableIva,
            RepresentanteLegal = e.DatosFiscales.RepresentanteLegal,
            Activo = e.Activo,
        };
    }
}
