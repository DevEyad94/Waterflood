import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  WaterfloodRecord,
  CreateWaterfloodRecordDto,
  UpdateWaterfloodRecordDto,
  WaterfloodFilter,
  WaterfloodHistoryPoint,
} from '../../models/waterflood.model';
import {
  GenericResponse,
  PaginatedResult,
  Pagination,
} from '../../models/pagination.model';

@Injectable({ providedIn: 'root' })
export class WaterfloodService {
  private apiUrl = `${environment.apiUrl}waterflood-data`;

  constructor(private http: HttpClient) {}

  getRecords(
    pageNumber = 1,
    pageSize = 10,
    sortColumn = 'measurementDate',
    sortDirection = 'desc',
    filter?: WaterfloodFilter
  ): Observable<PaginatedResult<GenericResponse<WaterfloodRecord[]> | null>> {
    const paginatedResult = new PaginatedResult<GenericResponse<WaterfloodRecord[]> | null>();
    let params = this.buildPaginationParams(pageNumber, pageSize, sortColumn, sortDirection);
    params = this.appendFilterParams(params, filter);

    return this.http
      .get<GenericResponse<WaterfloodRecord[]>>(this.apiUrl, {
        observe: 'response',
        params,
      })
      .pipe(
        map((response) => {
          paginatedResult.result = response.body;
          const paginationHeader = response.headers.get('Pagination');
          if (paginationHeader) {
            paginatedResult.pagination = JSON.parse(paginationHeader);
          }
          return paginatedResult;
        })
      );
  }

  getRecord(id: string): Observable<GenericResponse<WaterfloodRecord>> {
    return this.http.get<GenericResponse<WaterfloodRecord>>(`${this.apiUrl}/${id}`);
  }

  createRecord(
    record: CreateWaterfloodRecordDto
  ): Observable<GenericResponse<WaterfloodRecord>> {
    return this.http.post<GenericResponse<WaterfloodRecord>>(this.apiUrl, record);
  }

  updateRecord(
    record: UpdateWaterfloodRecordDto
  ): Observable<GenericResponse<WaterfloodRecord>> {
    return this.http.put<GenericResponse<WaterfloodRecord>>(this.apiUrl, record);
  }

  deleteRecord(id: string): Observable<GenericResponse<boolean>> {
    return this.http.delete<GenericResponse<boolean>>(`${this.apiUrl}/${id}`);
  }

  getAlerts(): Observable<GenericResponse<WaterfloodRecord[]>> {
    return this.http.get<GenericResponse<WaterfloodRecord[]>>(
      `${environment.apiUrl}monitoring/alerts`
    );
  }

  exportData(
    filter?: WaterfloodFilter,
    format: 'csv' | 'excel' = 'csv'
  ): Observable<Blob> {
    let params = this.appendFilterParams(new HttpParams(), filter);
    params = params.set('format', format);
    return this.http.get(`${this.apiUrl}/export`, {
      params,
      responseType: 'blob',
    });
  }

  getHistory(id: string): Observable<GenericResponse<WaterfloodHistoryPoint[]>> {
    return this.http.get<GenericResponse<WaterfloodHistoryPoint[]>>(
      `${this.apiUrl}/${id}/history`
    );
  }

  private buildPaginationParams(
    pageNumber: number,
    pageSize: number,
    sortColumn: string,
    sortDirection: string
  ): HttpParams {
    return new HttpParams()
      .set('PageNumber', pageNumber.toString())
      .set('PageSize', pageSize.toString())
      .set('SortColumn', sortColumn)
      .set('SortDirection', sortDirection);
  }

  private appendFilterParams(
    params: HttpParams,
    filter?: WaterfloodFilter
  ): HttpParams {
    if (!filter) return params;

    if (filter.search) params = params.set('search', filter.search);
    if (filter.fieldName) params = params.set('fieldName', filter.fieldName);
    if (filter.wellTypeCode) params = params.set('wellTypeCode', filter.wellTypeCode);
    if (filter.wellStatusCode) params = params.set('wellStatusCode', filter.wellStatusCode);
    if (filter.minInjectionRate != null)
      params = params.set('minInjectionRate', filter.minInjectionRate.toString());
    if (filter.maxInjectionRate != null)
      params = params.set('maxInjectionRate', filter.maxInjectionRate.toString());
    if (filter.minOilProductionRate != null)
      params = params.set('minOilProductionRate', filter.minOilProductionRate.toString());
    if (filter.maxOilProductionRate != null)
      params = params.set('maxOilProductionRate', filter.maxOilProductionRate.toString());
    if (filter.minWaterCut != null)
      params = params.set('minWaterCut', filter.minWaterCut.toString());
    if (filter.maxWaterCut != null)
      params = params.set('maxWaterCut', filter.maxWaterCut.toString());
    if (filter.minInjectionPressure != null)
      params = params.set('minInjectionPressure', filter.minInjectionPressure.toString());
    if (filter.maxInjectionPressure != null)
      params = params.set('maxInjectionPressure', filter.maxInjectionPressure.toString());
    if (filter.fromDate) params = params.set('fromDate', filter.fromDate);
    if (filter.toDate) params = params.set('toDate', filter.toDate);
    if (filter.requiresAttentionOnly)
      params = params.set('requiresAttentionOnly', 'true');

    return params;
  }
}
