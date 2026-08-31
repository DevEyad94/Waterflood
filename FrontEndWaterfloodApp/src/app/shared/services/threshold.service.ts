import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AlertThreshold, UpdateAlertThresholdDto } from '../../models/threshold.model';
import { GenericResponse } from '../../models/pagination.model';

@Injectable({ providedIn: 'root' })
export class WaterfloodThresholdService {
  private apiUrl = `${environment.apiUrl}monitoring/thresholds`;

  constructor(private http: HttpClient) {}

  get(): Observable<GenericResponse<AlertThreshold>> {
    return this.http.get<GenericResponse<AlertThreshold>>(this.apiUrl);
  }

  update(dto: UpdateAlertThresholdDto): Observable<GenericResponse<AlertThreshold>> {
    return this.http.put<GenericResponse<AlertThreshold>>(this.apiUrl, dto);
  }
}
