using System.ComponentModel.DataAnnotations;
using BackEndWaterFloodApp.Domain.Constants;

namespace BackEndWaterFloodApp.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class WellMeasurementValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        var wellTypeCode = GetStringProperty(validationContext, "WellTypeCode");
        if (string.IsNullOrWhiteSpace(wellTypeCode))
            return ValidationResult.Success;

        if (wellTypeCode == WaterfloodWellTypeCodes.Injector)
            return ValidateInjector(validationContext);

        if (wellTypeCode == WaterfloodWellTypeCodes.Producer)
            return ValidateProducer(validationContext);

        return ValidationResult.Success;
    }

    private static ValidationResult? ValidateInjector(ValidationContext context)
    {
        if (GetDecimalProperty(context, "InjectionRate") is null)
            return new ValidationResult("InjectionRate is required for injector wells.");

        if (GetDecimalProperty(context, "InjectionPressure") is null)
            return new ValidationResult("InjectionPressure is required for injector wells.");

        return ValidateWaterCutRange(context);
    }

    private static ValidationResult? ValidateProducer(ValidationContext context)
    {
        if (GetDecimalProperty(context, "OilProductionRate") is null)
            return new ValidationResult("OilProductionRate is required for producer wells.");

        if (GetDecimalProperty(context, "WaterProductionRate") is null)
            return new ValidationResult("WaterProductionRate is required for producer wells.");

        var waterCut = GetDecimalProperty(context, "WaterCut");
        if (waterCut is null)
            return new ValidationResult("WaterCut is required for producer wells.");

        return ValidateWaterCutRange(context);
    }

    private static ValidationResult? ValidateWaterCutRange(ValidationContext context)
    {
        var waterCut = GetDecimalProperty(context, "WaterCut");
        if (waterCut is not null && (waterCut < 0m || waterCut > 100m))
            return new ValidationResult("WaterCut must be between 0 and 100.");

        return ValidationResult.Success;
    }

    private static string? GetStringProperty(ValidationContext context, string propertyName) =>
        context.ObjectType.GetProperty(propertyName)?.GetValue(context.ObjectInstance) as string;

    private static decimal? GetDecimalProperty(ValidationContext context, string propertyName)
    {
        var value = context.ObjectType.GetProperty(propertyName)?.GetValue(context.ObjectInstance);
        return value as decimal?;
    }
}
