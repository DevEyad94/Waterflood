using BackEndWaterFloodApp.Application.Dtos.Waterflood;
using BackEndWaterFloodApp.Domain.Constants;
using FluentValidation;
using ZSK.Services.ReferenceData.Interfaces;

namespace BackEndWaterFloodApp.Application.Validators;

public class CreateWaterfloodRecordValidator : AbstractValidator<CreateWaterfloodRecordDto>
{
    public CreateWaterfloodRecordValidator(IZskReferenceService zskReferenceService)
    {
        RuleFor(x => x.WellName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.WellTypeCode)
            .NotEmpty()
            .MustAsync(
                async (code, cancellation) => await zskReferenceService.IsValidWellTypeCodeAsync(code)
            )
            .WithMessage("WellTypeCode must be a valid ZSK well type (INJ or PROD).");

        RuleFor(x => x.FieldName).NotEmpty().MaximumLength(100);

        RuleFor(x => x.Latitude).InclusiveBetween(-90m, 90m);
        RuleFor(x => x.Longitude).InclusiveBetween(-180m, 180m);

        RuleFor(x => x.WellStatusCode)
            .NotEmpty()
            .MustAsync(
                async (code, cancellation) =>
                    await zskReferenceService.IsValidWellStatusCodeAsync(code)
            )
            .WithMessage("WellStatusCode must be a valid ZSK well status (ACT, SHT, or MNT).");

        RuleFor(x => x.MeasurementDate)
            .LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(1))
            .WithMessage("MeasurementDate must not be a future date.");

        RuleFor(x => x.InjectionRate).GreaterThanOrEqualTo(0).When(x => x.InjectionRate.HasValue);
        RuleFor(x => x.InjectionPressure)
            .GreaterThanOrEqualTo(0)
            .When(x => x.InjectionPressure.HasValue);
        RuleFor(x => x.OilProductionRate)
            .GreaterThanOrEqualTo(0)
            .When(x => x.OilProductionRate.HasValue);
        RuleFor(x => x.WaterProductionRate)
            .GreaterThanOrEqualTo(0)
            .When(x => x.WaterProductionRate.HasValue);
        RuleFor(x => x.WaterCut).InclusiveBetween(0m, 100m).When(x => x.WaterCut.HasValue);

        When(
            x => x.WellTypeCode == WaterfloodWellTypeCodes.Injector,
            () =>
            {
                RuleFor(x => x.InjectionRate).NotNull().GreaterThanOrEqualTo(0);
                RuleFor(x => x.InjectionPressure).NotNull().GreaterThanOrEqualTo(0);
            }
        );

        When(
            x => x.WellTypeCode == WaterfloodWellTypeCodes.Producer,
            () =>
            {
                RuleFor(x => x.OilProductionRate).NotNull().GreaterThanOrEqualTo(0);
                RuleFor(x => x.WaterProductionRate).NotNull().GreaterThanOrEqualTo(0);
                RuleFor(x => x.WaterCut).NotNull().InclusiveBetween(0m, 100m);
            }
        );
    }
}

public class UpdateWaterfloodRecordValidator : AbstractValidator<UpdateWaterfloodRecordDto>
{
    public UpdateWaterfloodRecordValidator(CreateWaterfloodRecordValidator createValidator)
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x)
            .CustomAsync(
                async (dto, context, cancellation) =>
                {
                    var result = await createValidator.ValidateAsync(dto, cancellation);
                    foreach (var failure in result.Errors)
                        context.AddFailure(failure);
                }
            );
    }
}
