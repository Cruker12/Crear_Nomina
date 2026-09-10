using FluentValidation;
using GeneradoNominaSystem.Application.DTOs;

namespace GeneradoNominaSystem.Application.Validators;

public sealed class PlantillaCotizacionValidator : AbstractValidator<PlantillaCotizacionDto>
{
    public PlantillaCotizacionValidator()
    {
        RuleFor(x => x.EmpresaId).NotEmpty();
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
    }
}
