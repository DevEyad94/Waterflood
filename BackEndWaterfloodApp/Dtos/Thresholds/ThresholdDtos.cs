namespace BackEndWaterFloodApp.Application.Dtos.Thresholds;

public class AlertThresholdDto
{
    public int Id { get; set; }
    public decimal MaxWaterCutPercent { get; set; }
    public decimal MinOilProductionRate { get; set; }
    public decimal MinInjectionRate { get; set; }
    public decimal MaxInjectionPressure { get; set; }
    public decimal ProductionDeclinePercent { get; set; }
}

public class UpdateAlertThresholdDto
{
    public decimal MaxWaterCutPercent { get; set; }
    public decimal MinOilProductionRate { get; set; }
    public decimal MinInjectionRate { get; set; }
    public decimal MaxInjectionPressure { get; set; }
    public decimal ProductionDeclinePercent { get; set; }
}
