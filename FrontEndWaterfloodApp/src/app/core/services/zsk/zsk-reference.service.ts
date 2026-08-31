import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, shareReplay } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericResponse } from '../../../models/pagination.model';
import {
  ZskMonitoringRule,
  ZskReferenceData,
  ZskWellStatus,
} from './zsk-reference.model';

@Injectable({ providedIn: 'root' })
export class ZskReferenceService {
  private readonly apiUrl = `${environment.apiUrl}zsk`;
  private referenceCache$?: Observable<GenericResponse<ZskReferenceData>>;
  private rulesCache$?: Observable<GenericResponse<ZskMonitoringRule[]>>;

  constructor(private http: HttpClient) {}

  getReferenceData(): Observable<GenericResponse<ZskReferenceData>> {
    if (!this.referenceCache$) {
      this.referenceCache$ = this.http
        .get<GenericResponse<ZskReferenceData>>(`${this.apiUrl}/reference-data`)
        .pipe(shareReplay(1));
    }
    return this.referenceCache$;
  }

  getMonitoringRules(): Observable<GenericResponse<ZskMonitoringRule[]>> {
    if (!this.rulesCache$) {
      this.rulesCache$ = this.http
        .get<GenericResponse<ZskMonitoringRule[]>>(`${this.apiUrl}/rules`)
        .pipe(shareReplay(1));
    }
    return this.rulesCache$;
  }

  getWellTypeLabel(code: string, reference?: ZskReferenceData): string {
    return reference?.wellTypes.find((t) => t.code === code)?.name ?? code;
  }

  getWellStatus(
    reference?: ZskReferenceData,
    code?: string
  ): ZskWellStatus | undefined {
    if (!reference || !code) return undefined;
    return reference.wellStatuses.find((s) => s.code === code);
  }
}
