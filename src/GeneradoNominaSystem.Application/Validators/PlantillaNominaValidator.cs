using FluentValidation;
using GeneradoNominaSystem.Application.DTOs;

namespace GeneradoNominaSystem.Application.Validators;

public sealed class PlantillaNominaValidator : AbstractValidator<PlantillaNominaDto>
{
    public PlantillaNominaValidator()
    {
        RuleFor(x => x.EmpresaId).NotEmpty();
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Descripcion).MaximumLength(1000);
    }
}
