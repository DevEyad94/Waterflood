import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { NgxEchartsModule } from 'ngx-echarts';
import type { EChartsOption } from 'echarts';
import { WaterfloodRelationshipService } from '../../shared/services/relationship.service';
import { WaterfloodService } from '../../shared/services/waterflood.service';
import { ZskReferenceService } from '../../core/services/zsk/zsk-reference.service';
import {
  WaterfloodInjectorDetail,
  WaterfloodRelationship,
} from '../../models/relationship.model';
import {
  WaterfloodRecord,
  WATERFLOOD_WELL_TYPE_CODES,
} from '../../models/waterflood.model';
import { ZskReferenceData } from '../../core/services/zsk/zsk-reference.model';
import { HasRoleDirective } from '../../shared/directives/hasRole.directive';
import { Roles } from '../../core/enum/roles.enum';
import { ToastService } from '../../shared/services/toast.service';

@Component({
  selector: 'app-relationships',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NgxEchartsModule, HasRoleDirective],
  templateUrl: './relationships.component.html',
  styleUrls: ['./relationships.component.scss'],
})
export class RelationshipsComponent implements OnInit {
  role = Roles;
  relationships: WaterfloodRelationship[] = [];
  injectors: WaterfloodRecord[] = [];
  producers: WaterfloodRecord[] = [];
  zskReference?: ZskReferenceData;
  selectedInjectorId: string | null = null;
  injectorDetail?: WaterfloodInjectorDetail;
  correlationChart: EChartsOption = {};
  injectionTrendChart: EChartsOption = {};
  producerOilChart: EChartsOption = {};
  producerWaterCutChart: EChartsOption = {};
  relationshipForm!: FormGroup;
  isFormVisible = false;
  isEditMode = false;

  constructor(
    private waterfloodRelationshipService: WaterfloodRelationshipService,
    private waterfloodService: WaterfloodService,
    private zskReferenceService: ZskReferenceService,
    private fb: FormBuilder,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.relationshipForm = this.fb.group({
      id: [''],
      injectorWellId: ['', Validators.required],
      producerWellId: ['', Validators.required],
      distance: [0, [Validators.required, Validators.min(0)]],
      relationshipStatusCode: ['ACT'],
    });

    this.zskReferenceService.getReferenceData().subscribe((res) => {
      this.zskReference = res.data;
    });

    this.loadRelationships();
    this.loadWells();
  }

  loadRelationships(): void {
    this.waterfloodRelationshipService.getAll().subscribe((res) => {
      this.relationships = res.data ?? [];
    });
  }

  loadWells(): void {
    this.waterfloodService.getRecords(1, 500).subscribe((res) => {
      const all = res.result?.data ?? [];
      this.injectors = all.filter((w) => w.wellTypeCode === WATERFLOOD_WELL_TYPE_CODES.INJECTOR);
      this.producers = all.filter((w) => w.wellTypeCode === WATERFLOOD_WELL_TYPE_CODES.PRODUCER);
    });
  }

  selectInjector(injectorId: string): void {
    this.selectedInjectorId = injectorId;
    this.waterfloodRelationshipService.getInjectorDetail(injectorId).subscribe((res) => {
      this.injectorDetail = res.data;
      this.renderCharts();
    });
  }

  renderCharts(): void {
    if (!this.injectorDetail) return;

    const producers = this.injectorDetail.linkedProducers;
    this.correlationChart = {
      tooltip: { trigger: 'axis' },
      legend: { data: ['Oil Production', 'Water Cut'] },
      xAxis: { type: 'category', data: producers.map((p) => p.wellName) },
      yAxis: [
        { type: 'value', name: 'Oil (bbl/d)' },
        { type: 'value', name: 'Water Cut %', max: 100 },
      ],
      series: [
        {
          name: 'Oil Production',
          type: 'bar',
          data: producers.map((p) => p.oilProductionRate ?? 0),
          itemStyle: { color: '#4CAF50' },
        },
        {
          name: 'Water Cut',
          type: 'line',
          yAxisIndex: 1,
          data: producers.map((p) => p.waterCut ?? 0),
          itemStyle: { color: '#FF9800' },
        },
      ],
    };

    const injectorDates = this.injectorDetail.injectorTrend.map((p) =>
      p.measurementDate.split('T')[0]
    );
    this.injectionTrendChart = {
      tooltip: { trigger: 'axis' },
      xAxis: { type: 'category', data: injectorDates },
      yAxis: { type: 'value', name: 'bbl/d' },
      series: [
        {
          name: 'Injection Rate',
          type: 'line',
          data: this.injectorDetail.injectorTrend.map((p) => p.injectionRate ?? 0),
          itemStyle: { color: '#1565C0' },
        },
      ],
    };

    const allDates = [
      ...new Set(
        this.injectorDetail.producerTrends.flatMap((t) =>
          t.points.map((p) => p.measurementDate.split('T')[0])
        )
      ),
    ].sort();

    this.producerOilChart = {
      tooltip: { trigger: 'axis' },
      legend: { data: this.injectorDetail.producerTrends.map((t) => t.wellName) },
      xAxis: { type: 'category', data: allDates },
      yAxis: { type: 'value', name: 'Oil (bbl/d)' },
      series: this.injectorDetail.producerTrends.map((trend) => ({
        name: trend.wellName,
        type: 'line',
        data: allDates.map((date) => {
          const point = trend.points.find((p) => p.measurementDate.split('T')[0] === date);
          return point?.oilProductionRate ?? null;
        }),
      })),
    };

    this.producerWaterCutChart = {
      tooltip: { trigger: 'axis' },
      legend: { data: this.injectorDetail.producerTrends.map((t) => t.wellName) },
      xAxis: { type: 'category', data: allDates },
      yAxis: { type: 'value', name: 'Water Cut %', max: 100 },
      series: this.injectorDetail.producerTrends.map((trend) => ({
        name: trend.wellName,
        type: 'line',
        data: allDates.map((date) => {
          const point = trend.points.find((p) => p.measurementDate.split('T')[0] === date);
          return point?.waterCut ?? null;
        }),
      })),
    };
  }

  openCreateForm(): void {
    this.isEditMode = false;
    this.isFormVisible = true;
    this.relationshipForm.reset({ relationshipStatusCode: 'ACT', distance: 0 });
  }

  openEditForm(rel: WaterfloodRelationship): void {
    this.isEditMode = true;
    this.isFormVisible = true;
    this.relationshipForm.patchValue(rel);
  }

  submitRelationship(): void {
    if (this.relationshipForm.invalid) return;
    const value = this.relationshipForm.value;
    const request = this.isEditMode
      ? this.waterfloodRelationshipService.update(value)
      : this.waterfloodRelationshipService.create(value);

    request.subscribe({
      next: () => {
        this.toastService.success(
          this.isEditMode ? 'Relationship updated' : 'Waterflood relationship created'
        );
        this.isFormVisible = false;
        this.loadRelationships();
        if (this.selectedInjectorId) this.selectInjector(this.selectedInjectorId);
      },
      error: () => this.toastService.error('Failed to save waterflood relationship'),
    });
  }

  deleteRelationship(id: string): void {
    if (!confirm('Delete this injector–producer relationship?')) return;
    this.waterfloodRelationshipService.delete(id).subscribe({
      next: () => {
        this.toastService.success('Relationship deleted');
        this.loadRelationships();
        if (this.selectedInjectorId) this.selectInjector(this.selectedInjectorId);
      },
      error: () => this.toastService.error('Failed to delete relationship'),
    });
  }

  getProducerOil(producerId: string): number | string {
    const producer = this.injectorDetail?.linkedProducers.find((p) => p.id === producerId);
    return producer?.oilProductionRate ?? '-';
  }

  getProducerWaterCut(producerId: string): number | string {
    const producer = this.injectorDetail?.linkedProducers.find((p) => p.id === producerId);
    return producer?.waterCut ?? '-';
  }
}
