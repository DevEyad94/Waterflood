export interface WaterfloodAlert {
  ruleIdentifier: string;
  message: string;
  alertStatus: string;
}

export interface WaterfloodRecord {
  id: string;
  wellName: string;
  wellTypeCode: string;
  wellTypeName: string;
  fieldName: string;
  latitude: number;
  longitude: number;
  injectionRate?: number;
  oilProductionRate?: number;
  waterProductionRate?: number;
  waterCut?: number;
  injectionPressure?: number;
  wellStatusCode: string;
  wellStatusName: string;
  statusColorCode: string;
  measurementDate: string;
  requiresAttention?: boolean;
  alerts?: WaterfloodAlert[];
}

export interface CreateWaterfloodRecordDto {
  wellName: string;
  wellTypeCode: string;
  fieldName: string;
  latitude: number;
  longitude: number;
  injectionRate?: number;
  oilProductionRate?: number;
  waterProductionRate?: number;
  waterCut?: number;
  injectionPressure?: number;
  wellStatusCode: string;
  measurementDate: string;
}

export interface UpdateWaterfloodRecordDto extends CreateWaterfloodRecordDto {
  id: string;
}

export interface WaterfloodFilter {
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
  search?: string;
  requiresAttentionOnly?: boolean;
}

export const WATERFLOOD_FIELD_NAMES = [
  'North Field',
  'Central Field',
  'South Field',
  'West Field',
];

export const WATERFLOOD_WELL_TYPE_CODES = {
  INJECTOR: 'INJ',
  PRODUCER: 'PROD',
} as const;

export const WATERFLOOD_WELL_STATUS_CODES = {
  ACTIVE: 'ACT',
  SHUT_IN: 'SHT',
  MAINTENANCE: 'MNT',
} as const;

export const WATERFLOOD_ALERT_RULES = {
  HIGH_WATER_CUT: 'RULE_HIGH_WATER_CUT',
  LOW_OIL_PROD: 'RULE_LOW_OIL_PROD',
  COMBINED_DEFICIT: 'RULE_COMBINED_DEFICIT',
  LOW_INJECTION: 'RULE_LOW_INJECTION',
  HIGH_PRESSURE: 'RULE_HIGH_PRESSURE',
  INACTIVE_WELL: 'RULE_INACTIVE_WELL',
  PRODUCTION_DECLINE: 'RULE_PRODUCTION_DECLINE',
} as const;

export interface WaterfloodHistoryPoint {
  measurementDate: string;
  injectionRate?: number;
  oilProductionRate?: number;
  waterProductionRate?: number;
  waterCut?: number;
  injectionPressure?: number;
  wellStatusCode: string;
}
