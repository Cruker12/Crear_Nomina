using FluentValidation;
using GeneradoNominaSystem.Application.DTOs;

namespace GeneradoNominaSystem.Application.Validators;

public sealed class CotizacionValidator : AbstractValidator<CotizacionDto>
{
    public CotizacionValidator()
    {
        RuleFor(x => x.EmpresaId).NotEmpty();
        RuleFor(x => x.ClienteNombre).NotEmpty().MaximumLength(250);
        RuleFor(x => x.ClienteDocumento).MaximumLength(50);
        RuleFor(x => x.ClienteEmail).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.ClienteEmail));
        RuleFor(x => x.ClienteTelefono).MaximumLength(50);
        RuleFor(x => x.FechaVigencia)
            .GreaterThanOrEqualTo(x => x.FechaEmision)
            .WithMessage("La vigencia no puede ser anterior a la emisión.");
        RuleFor(x => x.Moneda).NotEmpty().Length(3);
        RuleFor(x => x.Observaciones).MaximumLength(2000);
        RuleFor(x => x.Estado).IsInEnum();
    }
}
