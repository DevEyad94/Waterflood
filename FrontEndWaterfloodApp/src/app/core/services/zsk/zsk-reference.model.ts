export interface ZskWellType {
  code: string;
  name: string;
  description: string;
}

export interface ZskWellStatus {
  code: string;
  name: string;
  description: string;
  colorCode: string;
}

export interface ZskRelationshipStatus {
  code: string;
  name: string;
  description: string;
}

export interface ZskMonitoringRule {
  ruleCode: string;
  name: string;
  description: string;
  targetWellType: string;
  defaultThresholdValue: number;
  severity: string;
}

export interface ZskReferenceData {
  wellTypes: ZskWellType[];
  wellStatuses: ZskWellStatus[];
  relationshipStatuses: ZskRelationshipStatus[];
}
