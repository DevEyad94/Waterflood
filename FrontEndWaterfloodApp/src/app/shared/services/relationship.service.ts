import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateWaterfloodRelationshipDto,
  WaterfloodInjectorDetail,
  WaterfloodRelationship,
  UpdateWaterfloodRelationshipDto,
} from '../../models/relationship.model';
import { GenericResponse } from '../../models/pagination.model';

@Injectable({ providedIn: 'root' })
export class WaterfloodRelationshipService {
  private apiUrl = `${environment.apiUrl}relationships`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<GenericResponse<WaterfloodRelationship[]>> {
    return this.http.get<GenericResponse<WaterfloodRelationship[]>>(this.apiUrl);
  }

  getInjectorDetail(
    injectorWellId: string
  ): Observable<GenericResponse<WaterfloodInjectorDetail>> {
    return this.http.get<GenericResponse<WaterfloodInjectorDetail>>(
      `${this.apiUrl}/injector/${injectorWellId}`
    );
  }

  create(
    dto: CreateWaterfloodRelationshipDto
  ): Observable<GenericResponse<WaterfloodRelationship>> {
    return this.http.post<GenericResponse<WaterfloodRelationship>>(this.apiUrl, dto);
  }

  update(
    dto: UpdateWaterfloodRelationshipDto
  ): Observable<GenericResponse<WaterfloodRelationship>> {
    return this.http.put<GenericResponse<WaterfloodRelationship>>(this.apiUrl, dto);
  }

  delete(id: string): Observable<GenericResponse<boolean>> {
    return this.http.delete<GenericResponse<boolean>>(`${this.apiUrl}/${id}`);
  }
}
