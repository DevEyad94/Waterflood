using BackEndWaterFloodApp.Application.Dtos.Waterflood;
using BackEndWaterFloodApp.Domain.Constants;
using BackEndWaterFloodApp.Domain.Entities;
using ZSK.Services.ReferenceData.Interfaces;

namespace BackEndWaterFloodApp.Services.Monitoring;

public static class WaterfloodAlertEvaluator
{
    public static (bool RequiresAttention, List<WaterfloodAlertDto> Alerts) Evaluate(
        WaterfloodRecord record,
        ZskEffectiveThresholds thresholds,
        decimal? previousOilProductionRate = null
    )
    {
        var alerts = new List<WaterfloodAlertDto>();
        var rules = thresholds.Rules.ToDictionary(r => r.RuleCode, r => r);

        if (
            record.WellStatusCode
                is WaterfloodWellStatusCodes.ShutIn
                    or WaterfloodWellStatusCodes.Maintenance
        )
        {
            alerts.Add(
                BuildAlert(
                    WaterfloodAlertRuleIdentifiers.InactiveWell,
                    $"Well status is {record.WellStatus?.Name ?? record.WellStatusCode}",
                    rules
                )
            );
        }

        if (record.WellTypeCode == WaterfloodWellTypeCodes.Producer)
        {
            var highWaterCut = record.WaterCut > thresholds.MaxWaterCutPercent;
            var lowOil = record.OilProductionRate < thresholds.MinOilProductionRate;

            if (highWaterCut && lowOil)
            {
                alerts.Add(
                    BuildAlert(
                        WaterfloodAlertRuleIdentifiers.CombinedDeficit,
                        $"Water cut {record.WaterCut}% exceeds {thresholds.MaxWaterCutPercent}% and oil production {record.OilProductionRate} bbl/d is below {thresholds.MinOilProductionRate} bbl/d",
                        rules
                    )
                );
            }
            else
            {
                if (highWaterCut)
                {
                    alerts.Add(
                        BuildAlert(
                            WaterfloodAlertRuleIdentifiers.HighWaterCut,
                            $"Water cut {record.WaterCut}% exceeds threshold of {thresholds.MaxWaterCutPercent}%",
                            rules
                        )
                    );
                }

                if (lowOil)
                {
                    alerts.Add(
                        BuildAlert(
                            WaterfloodAlertRuleIdentifiers.LowOilProduction,
                            $"Oil production {record.OilProductionRate} bbl/d is below threshold of {thresholds.MinOilProductionRate} bbl/d",
                            rules
                        )
                    );
                }
            }

            if (
                previousOilProductionRate.HasValue
                && previousOilProductionRate.Value > 0
                && record.OilProductionRate.HasValue
            )
            {
                var declinePercent =
                    (previousOilProductionRate.Value - record.OilProductionRate.Value)
                    / previousOilProductionRate.Value
                    * 100m;

                if (declinePercent >= thresholds.ProductionDeclinePercent)
                {
                    alerts.Add(
                        BuildAlert(
                            WaterfloodAlertRuleIdentifiers.ProductionDecline,
                            $"Oil production declined {Math.Round(declinePercent, 1)}% from {previousOilProductionRate.Value} to {record.OilProductionRate} bbl/d (threshold {thresholds.ProductionDeclinePercent}%)",
                            rules
                        )
                    );
                }
            }
        }

        if (record.WellTypeCode == WaterfloodWellTypeCodes.Injector)
        {
            if (record.InjectionRate < thresholds.MinInjectionRate)
            {
                alerts.Add(
                    BuildAlert(
                        WaterfloodAlertRuleIdentifiers.LowInjection,
                        $"Injection rate {record.InjectionRate} bbl/d is below target of {thresholds.MinInjectionRate} bbl/d",
                        rules
                    )
                );
            }

            if (record.InjectionPressure > thresholds.MaxInjectionPressure)
            {
                alerts.Add(
                    BuildAlert(
                        WaterfloodAlertRuleIdentifiers.HighPressure,
                        $"Injection pressure {record.InjectionPressure} psi exceeds maximum of {thresholds.MaxInjectionPressure} psi",
                        rules
                    )
                );
            }
        }

        return (alerts.Count > 0, alerts);
    }

    private static WaterfloodAlertDto BuildAlert(
        string ruleCode,
        string message,
        Dictionary<string, ZSK.Services.ReferenceData.Dtos.ZskMonitoringRuleDto> rules
    ) =>
        new()
        {
            RuleIdentifier = ruleCode,
            Message = message,
            AlertStatus = rules.TryGetValue(ruleCode, out var rule) ? rule.Severity : "Warning",
        };
}
