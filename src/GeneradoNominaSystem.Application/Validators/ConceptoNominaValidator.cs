using FluentValidation;
using GeneradoNominaSystem.Application.DTOs;

namespace GeneradoNominaSystem.Application.Validators;

public sealed class ConceptoNominaValidator : AbstractValidator<ConceptoNominaDto>
{
    public ConceptoNominaValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Subtipo).MaximumLength(150);
        RuleFor(x => x.Tipo).IsInEnum();
        RuleFor(x => x.Orden).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PorcentajeBase)
            .InclusiveBetween(0m, 100m)
            .When(x => x.EsPorcentaje)
            .WithMessage("El porcentaje debe estar entre 0 y 100.");
        RuleFor(x => x.PorcentajeBase)
            .NotNull()
            .When(x => x.EsPorcentaje);
        RuleFor(x => x.ValorFijoMonto)
            .GreaterThanOrEqualTo(0m)
            .When(x => !x.EsPorcentaje && x.ValorFijoMonto.HasValue);
        RuleFor(x => x.ValorFijoMoneda).NotEmpty().Length(3);
    }
}
