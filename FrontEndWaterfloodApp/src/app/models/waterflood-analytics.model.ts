export interface WaterfloodKpiSummary {
  totalInjectionRate: number;
  totalOilProductionRate: number;
  totalWaterProductionRate: number;
  averageWaterCut: number;
  activeInjectorCount: number;
  activeProducerCount: number;
  wellsRequiringAttention: number;
  injectionEfficiencyPercent: number;
}

export interface WaterfloodTrendDataPoint {
  period: string;
  totalInjectionRate: number;
  totalOilProductionRate: number;
  averageWaterCut: number;
  averageInjectionPressure: number;
}

export interface WaterfloodStatusDistribution {
  wellStatusCode: string;
  wellStatusName: string;
  colorCode: string;
  count: number;
}

export interface WaterfloodWellRate {
  wellName: string;
  wellTypeCode: string;
  rate: number;
}

export interface WaterfloodTrendsResponse {
  trends: WaterfloodTrendDataPoint[];
  statusDistribution: WaterfloodStatusDistribution[];
  injectionByWell: WaterfloodWellRate[];
  oilProductionByWell: WaterfloodWellRate[];
}

export interface WaterfloodAnalyticsFilter {
  fieldName?: string;
  wellTypeCode?: string;
  wellStatusCode?: string;
  minInjectionRate?: number;
  maxInjectionRate?: number;
  minOilProductionRate?: number;
  maxOilProductionRate?: number;
  minWaterCut?: number;
  maxWaterCut?: number;
  minInjectionPressure?: number;
  maxInjectionPressure?: number;
  fromDate?: string;
  toDate?: string;
}
