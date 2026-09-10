using FluentValidation;
using GeneradoNominaSystem.Application.DTOs;

namespace GeneradoNominaSystem.Application.Validators;

public sealed class ProductoServicioValidator : AbstractValidator<ProductoServicioDto>
{
    public ProductoServicioValidator()
    {
        RuleFor(x => x.EmpresaId).NotEmpty();
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Descripcion).MaximumLength(1000);
        RuleFor(x => x.Unidad).NotEmpty().MaximumLength(50);
        RuleFor(x => x.PrecioUnitarioMonto).GreaterThanOrEqualTo(0m);
        RuleFor(x => x.PrecioUnitarioMoneda).NotEmpty().Length(3);
    }
}
