import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  WaterfloodAnalyticsFilter,
  WaterfloodKpiSummary,
  WaterfloodTrendsResponse,
} from '../../models/waterflood-analytics.model';
import { GenericResponse } from '../../models/pagination.model';

@Injectable({ providedIn: 'root' })
export class WaterfloodAnalyticsService {
  private apiUrl = `${environment.apiUrl}waterflood-analytics`;

  constructor(private http: HttpClient) {}

  getKpi(
    filter?: WaterfloodAnalyticsFilter
  ): Observable<GenericResponse<WaterfloodKpiSummary>> {
    return this.http.get<GenericResponse<WaterfloodKpiSummary>>(`${this.apiUrl}/kpi`, {
      params: this.buildParams(filter),
    });
  }

  getTrends(
    filter?: WaterfloodAnalyticsFilter
  ): Observable<GenericResponse<WaterfloodTrendsResponse>> {
    return this.http.get<GenericResponse<WaterfloodTrendsResponse>>(`${this.apiUrl}/trends`, {
      params: this.buildParams(filter),
    });
  }

  private buildParams(filter?: WaterfloodAnalyticsFilter): HttpParams {
    let params = new HttpParams();
    if (!filter) return params;

    Object.entries(filter).forEach(([key, value]) => {
      if (value !== null && value !== undefined && value !== '') {
        params = params.set(key, String(value));
      }
    });

    return params;
  }
}
