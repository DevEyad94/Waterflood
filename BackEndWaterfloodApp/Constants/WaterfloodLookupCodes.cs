namespace BackEndWaterFloodApp.Domain.Constants;

public static class WaterfloodWellTypeCodes
{
    public const string Injector = "INJ";
    public const string Producer = "PROD";
}

public static class WaterfloodWellStatusCodes
{
    public const string Active = "ACT";
    public const string ShutIn = "SHT";
    public const string Maintenance = "MNT";
}

public static class WaterfloodRelationshipStatusCodes
{
    public const string Active = "ACT";
    public const string Inactive = "INA";
}

public static class WaterfloodAlertRuleIdentifiers
{
    public const string HighWaterCut = "RULE_HIGH_WATER_CUT";
    public const string LowOilProduction = "RULE_LOW_OIL_PROD";
    public const string CombinedDeficit = "RULE_COMBINED_DEFICIT";
    public const string LowInjection = "RULE_LOW_INJECTION";
    public const string HighPressure = "RULE_HIGH_PRESSURE";
    public const string InactiveWell = "RULE_INACTIVE_WELL";
    public const string ProductionDecline = "RULE_PRODUCTION_DECLINE";
}
