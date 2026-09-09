using FluentValidation;
using GeneradoNominaSystem.Application.DTOs;

namespace GeneradoNominaSystem.Application.Validators;

public sealed class EmpleadoValidator : AbstractValidator<EmpleadoDto>
{
    public EmpleadoValidator()
    {
        RuleFor(x => x.EmpresaId).NotEmpty();
        RuleFor(x => x.NumeroDocumento).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Nombres).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Apellidos).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Telefono).MaximumLength(50);
        RuleFor(x => x.Cargo).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DepartamentoArea).MaximumLength(200);
        RuleFor(x => x.FechaIngreso).LessThanOrEqualTo(DateTime.Today);
        RuleFor(x => x.SalarioBaseMonto).GreaterThanOrEqualTo(0m);
        RuleFor(x => x.SalarioBaseMoneda).NotEmpty().Length(3);
        RuleFor(x => x.TipoDocumento).IsInEnum();
        RuleFor(x => x.TipoContrato).IsInEnum();
        RuleFor(x => x.Estado).IsInEnum();
    }
}
