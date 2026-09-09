using FluentValidation;
using GeneradoNominaSystem.Application.DTOs;

namespace GeneradoNominaSystem.Application.Validators;

public sealed class EmpresaValidator : AbstractValidator<EmpresaDto>
{
    public EmpresaValidator()
    {
        RuleFor(x => x.RazonSocial).NotEmpty().MaximumLength(250);
        RuleFor(x => x.NombreComercial).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Nit).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Telefono).MaximumLength(50);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.LogoRuta).MaximumLength(500);
        RuleFor(x => x.DireccionCompleta).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Ciudad).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Departamento).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Pais).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CodigoPostal).MaximumLength(20);
        RuleFor(x => x.DigitoVerificacion).MaximumLength(5);
        RuleFor(x => x.RegimenTributario).MaximumLength(150);
        RuleFor(x => x.RepresentanteLegal).MaximumLength(250);
    }
}
