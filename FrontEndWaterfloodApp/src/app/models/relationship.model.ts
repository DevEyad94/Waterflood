export interface WaterfloodRelationship {
  id: string;
  injectorWellId: string;
  injectorWellName: string;
  producerWellId: string;
  producerWellName: string;
  distance: number;
  relationshipStatusCode: string;
  relationshipStatusName: string;
}

export interface CreateWaterfloodRelationshipDto {
  injectorWellId: string;
  producerWellId: string;
  distance: number;
  relationshipStatusCode: string;
}

export interface UpdateWaterfloodRelationshipDto extends CreateWaterfloodRelationshipDto {
  id: string;
}

export interface WaterfloodInjectorDetail {
  injector: import('./waterflood.model').WaterfloodRecord;
  relationships: WaterfloodRelationship[];
  linkedProducers: import('./waterflood.model').WaterfloodRecord[];
  injectorTrend: import('./waterflood.model').WaterfloodHistoryPoint[];
  producerTrends: WaterfloodProducerTrend[];
}

export interface WaterfloodProducerTrend {
  wellId: string;
  wellName: string;
  points: import('./waterflood.model').WaterfloodHistoryPoint[];
}
