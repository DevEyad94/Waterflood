import { Component, OnInit, ViewChild, ElementRef, AfterViewInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { NgxEchartsModule } from 'ngx-echarts';
import type { EChartsOption } from 'echarts';
import * as L from 'leaflet';
import { WaterfloodService } from '../../shared/services/waterflood.service';
import { WaterfloodRelationshipService } from '../../shared/services/relationship.service';
import { ZskReferenceService } from '../../core/services/zsk/zsk-reference.service';
import {
  WaterfloodRecord,
  WaterfloodHistoryPoint,
  WATERFLOOD_FIELD_NAMES,
} from '../../models/waterflood.model';
import { WaterfloodRelationship } from '../../models/relationship.model';
import { ZskReferenceData } from '../../core/services/zsk/zsk-reference.model';

@Component({
  selector: 'app-map',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NgxEchartsModule],
  templateUrl: './map.component.html',
  styleUrls: ['./map.component.scss'],
})
export class MapComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('mapContainer', { static: false }) mapElement!: ElementRef;

  wells: WaterfloodRecord[] = [];
  relationships: WaterfloodRelationship[] = [];
  map?: L.Map;
  markers: L.CircleMarker[] = [];
  connectionLines: L.Polyline[] = [];
  selectedWell: WaterfloodRecord | null = null;
  wellHistory: WaterfloodHistoryPoint[] = [];
  historyChart: EChartsOption = {};
  filterForm!: FormGroup;
  zskReference?: ZskReferenceData;
  showConnectivity = true;

  fieldOptions = WATERFLOOD_FIELD_NAMES.map((f) => ({ value: f, label: f }));
  wellTypeOptions: { value: string; label: string }[] = [];
  wellStatusOptions: { value: string; label: string }[] = [];

  constructor(
    private waterfloodService: WaterfloodService,
    private relationshipService: WaterfloodRelationshipService,
    private zskReferenceService: ZskReferenceService,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.filterForm = this.fb.group({
      fieldName: [null],
      wellTypeCode: [null],
      wellStatusCode: [null],
      minInjectionRate: [null],
      maxInjectionRate: [null],
      minOilProductionRate: [null],
      maxOilProductionRate: [null],
      minWaterCut: [null],
      maxWaterCut: [null],
      minInjectionPressure: [null],
      maxInjectionPressure: [null],
      fromDate: [null],
      toDate: [null],
    });

    this.zskReferenceService.getReferenceData().subscribe((res) => {
      this.zskReference = res.data;
      this.wellTypeOptions =
        res.data?.wellTypes.map((t) => ({ value: t.code, label: t.name })) ?? [];
      this.wellStatusOptions =
        res.data?.wellStatuses.map((s) => ({ value: s.code, label: s.name })) ?? [];
    });

    this.relationshipService.getAll().subscribe((res) => {
      this.relationships = res.data ?? [];
      if (this.map) this.drawConnections();
    });

    this.filterForm.valueChanges.subscribe(() => this.loadWells());
    this.loadWells();
  }

  ngAfterViewInit(): void {
    if (!this.mapElement) return;
    this.map = L.map(this.mapElement.nativeElement).setView([22.0, 56.0], 7);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap contributors',
    }).addTo(this.map);
    this.placeMarkers();
    this.drawConnections();
  }

  ngOnDestroy(): void {
    this.map?.remove();
  }

  loadWells(): void {
    this.waterfloodService
      .getRecords(1, 500, 'wellName', 'asc', this.filterForm.value)
      .subscribe({
        next: (res) => {
          this.wells = res.result?.data ?? [];
          if (this.map) {
            this.placeMarkers();
            this.drawConnections();
          }
        },
      });
  }

  toggleConnectivity(): void {
    this.showConnectivity = !this.showConnectivity;
    this.drawConnections();
  }

  placeMarkers(): void {
    if (!this.map) return;

    this.markers.forEach((m) => m.remove());
    this.markers = [];

    this.wells.forEach((well) => {
      const color = well.statusColorCode || this.getStatusColor(well.wellStatusCode);
      const radius = well.wellTypeCode === 'INJ' ? 10 : 7;
      const marker = L.circleMarker([well.latitude, well.longitude], {
        radius,
        fillColor: color,
        color: well.wellTypeCode === 'INJ' ? '#1565C0' : '#2E7D32',
        weight: 2,
        fillOpacity: 0.9,
      }).addTo(this.map!);

      marker.bindPopup(`<strong>${well.wellName}</strong><br>${well.wellTypeName}`);
      marker.on('click', () => this.selectWell(well));
      this.markers.push(marker);
    });
  }

  drawConnections(): void {
    if (!this.map) return;
    this.connectionLines.forEach((line) => line.remove());
    this.connectionLines = [];
    if (!this.showConnectivity) return;

    const wellById = new Map(this.wells.map((w) => [w.id, w]));
    this.relationships
      .filter((r) => r.relationshipStatusCode === 'ACT')
      .forEach((rel) => {
        const injector = wellById.get(rel.injectorWellId);
        const producer = wellById.get(rel.producerWellId);
        if (!injector || !producer) return;

        const line = L.polyline(
          [
            [injector.latitude, injector.longitude],
            [producer.latitude, producer.longitude],
          ],
          { color: '#607D8B', weight: 2, dashArray: '6 4', opacity: 0.8 }
        ).addTo(this.map!);
        line.bindPopup(
          `${injector.wellName} → ${producer.wellName}<br>${rel.distance} km`
        );
        this.connectionLines.push(line);
      });
  }

  selectWell(well: WaterfloodRecord): void {
    this.selectedWell = well;
    this.waterfloodService.getHistory(well.id).subscribe((res) => {
      this.wellHistory = res.data ?? [];
      this.renderHistoryChart(well);
    });
  }

  renderHistoryChart(well: WaterfloodRecord): void {
    const dates = this.wellHistory.map((p) => p.measurementDate.split('T')[0]);
    const isInjector = well.wellTypeCode === 'INJ';
    this.historyChart = {
      tooltip: { trigger: 'axis' },
      legend: {
        data: isInjector ? ['Injection Rate', 'Injection Pressure'] : ['Oil Production', 'Water Cut'],
      },
      xAxis: { type: 'category', data: dates },
      yAxis: isInjector
        ? [
            { type: 'value', name: 'bbl/d' },
            { type: 'value', name: 'psi' },
          ]
        : [
            { type: 'value', name: 'bbl/d' },
            { type: 'value', name: '%', max: 100 },
          ],
      series: isInjector
        ? [
            {
              name: 'Injection Rate',
              type: 'line',
              data: this.wellHistory.map((p) => p.injectionRate ?? 0),
              itemStyle: { color: '#1565C0' },
            },
            {
              name: 'Injection Pressure',
              type: 'line',
              yAxisIndex: 1,
              data: this.wellHistory.map((p) => p.injectionPressure ?? 0),
              itemStyle: { color: '#9C27B0' },
            },
          ]
        : [
            {
              name: 'Oil Production',
              type: 'line',
              data: this.wellHistory.map((p) => p.oilProductionRate ?? 0),
              itemStyle: { color: '#2E7D32' },
            },
            {
              name: 'Water Cut',
              type: 'line',
              yAxisIndex: 1,
              data: this.wellHistory.map((p) => p.waterCut ?? 0),
              itemStyle: { color: '#FF9800' },
            },
          ],
    };
  }

  getStatusColor(code: string): string {
    return (
      this.zskReference?.wellStatuses.find((s) => s.code === code)?.colorCode ?? '#9E9E9E'
    );
  }

  closeDrawer(): void {
    this.selectedWell = null;
    this.wellHistory = [];
  }
}
