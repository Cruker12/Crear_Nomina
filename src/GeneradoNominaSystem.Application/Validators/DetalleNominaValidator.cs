using FluentValidation;
using GeneradoNominaSystem.Application.DTOs;

namespace GeneradoNominaSystem.Application.Validators;

public sealed class DetalleNominaValidator : AbstractValidator<DetalleNominaDto>
{
    public DetalleNominaValidator()
    {
        RuleFor(x => x.ConceptoNominaId).NotEmpty();
        RuleFor(x => x.ValorMonto).GreaterThanOrEqualTo(0m);
        RuleFor(x => x.ValorMoneda).NotEmpty().Length(3);
        RuleFor(x => x.Cantidad).GreaterThanOrEqualTo(0m).When(x => x.Cantidad.HasValue);
        RuleFor(x => x.Descripcion).MaximumLength(500);
        RuleFor(x => x.Orden).GreaterThanOrEqualTo(0);
    }
}
