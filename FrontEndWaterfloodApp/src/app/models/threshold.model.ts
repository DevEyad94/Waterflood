export interface AlertThreshold {
  id: number;
  maxWaterCutPercent: number;
  minOilProductionRate: number;
  minInjectionRate: number;
  maxInjectionPressure: number;
  productionDeclinePercent: number;
}

export interface UpdateAlertThresholdDto {
  maxWaterCutPercent: number;
  minOilProductionRate: number;
  minInjectionRate: number;
  maxInjectionPressure: number;
  productionDeclinePercent: number;
}
