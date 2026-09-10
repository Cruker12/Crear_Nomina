using FluentValidation;
using GeneradoNominaSystem.Application.DTOs;

namespace GeneradoNominaSystem.Application.Validators;

public sealed class DetalleCotizacionValidator : AbstractValidator<DetalleCotizacionDto>
{
    public DetalleCotizacionValidator()
    {
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Cantidad).GreaterThan(0m);
        RuleFor(x => x.PrecioUnitarioMonto).GreaterThanOrEqualTo(0m);
        RuleFor(x => x.PrecioUnitarioMoneda).NotEmpty().Length(3);
        RuleFor(x => x.DescuentoPorcentaje)
            .InclusiveBetween(0m, 100m)
            .When(x => x.DescuentoPorcentaje.HasValue);
        RuleFor(x => x.Orden).GreaterThanOrEqualTo(0);
    }
}
