import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { NgxEchartsModule } from 'ngx-echarts';
import type { EChartsOption } from 'echarts';
import { WaterfloodAnalyticsService } from '../../shared/services/waterflood-analytics.service';
import { ZskReferenceService } from '../../core/services/zsk/zsk-reference.service';
import {
  WaterfloodAnalyticsFilter,
  WaterfloodKpiSummary,
  WaterfloodTrendsResponse,
} from '../../models/waterflood-analytics.model';
import { WATERFLOOD_FIELD_NAMES } from '../../models/waterflood.model';
import { ZskReferenceData } from '../../core/services/zsk/zsk-reference.model';
import { ZskSelectComponent } from '../../shared/components/zsk/zsk-select.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NgxEchartsModule, ZskSelectComponent],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent implements OnInit {
  filterForm!: FormGroup;
  kpi?: WaterfloodKpiSummary;
  trends?: WaterfloodTrendsResponse;
  zskReference?: ZskReferenceData;

  fieldOptions = WATERFLOOD_FIELD_NAMES.map((f) => ({ value: f, label: f }));
  wellTypeOptions: { value: string; label: string }[] = [];
  wellStatusOptions: { value: string; label: string }[] = [];

  injectionVsOilChart: EChartsOption = {};
  waterCutChart: EChartsOption = {};
  pressureChart: EChartsOption = {};
  statusChart: EChartsOption = {};
  injectionByWellChart: EChartsOption = {};
  oilByWellChart: EChartsOption = {};

  constructor(
    private waterfloodAnalyticsService: WaterfloodAnalyticsService,
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

    this.filterForm.valueChanges.subscribe(() => this.loadData());
    this.loadData();
  }

  loadData(): void {
    const filter: WaterfloodAnalyticsFilter = this.filterForm.value;
    this.waterfloodAnalyticsService.getKpi(filter).subscribe((res) => {
      this.kpi = res.data;
    });
    this.waterfloodAnalyticsService.getTrends(filter).subscribe((res) => {
      this.trends = res.data;
      this.renderCharts();
    });
  }

  resetFilters(): void {
    this.filterForm.reset();
  }

  renderCharts(): void {
    if (!this.trends) return;

    const periods = this.trends.trends.map((t) => t.period);

    this.injectionVsOilChart = {
      tooltip: { trigger: 'axis' },
      legend: { data: ['Waterflood Injection', 'Oil Production'] },
      xAxis: { type: 'category', data: periods },
      yAxis: { type: 'value', name: 'bbl/day' },
      series: [
        {
          name: 'Waterflood Injection',
          type: 'line',
          data: this.trends.trends.map((t) => t.totalInjectionRate),
          itemStyle: { color: '#2196F3' },
        },
        {
          name: 'Oil Production',
          type: 'line',
          data: this.trends.trends.map((t) => t.totalOilProductionRate),
          itemStyle: { color: '#4CAF50' },
        },
      ],
    };

    this.waterCutChart = {
      tooltip: { trigger: 'axis' },
      xAxis: { type: 'category', data: periods },
      yAxis: { type: 'value', name: 'Water Cut %' },
      series: [
        {
          name: 'Water Cut',
          type: 'line',
          data: this.trends.trends.map((t) => t.averageWaterCut),
          areaStyle: { color: 'rgba(255,152,0,0.2)' },
          itemStyle: { color: '#FF9800' },
        },
      ],
    };

    this.pressureChart = {
      tooltip: { trigger: 'axis' },
      xAxis: { type: 'category', data: periods },
      yAxis: { type: 'value', name: 'psi' },
      series: [
        {
          name: 'Injection Pressure',
          type: 'line',
          data: this.trends.trends.map((t) => t.averageInjectionPressure),
          itemStyle: { color: '#9C27B0' },
        },
      ],
    };

    this.statusChart = {
      tooltip: { trigger: 'item' },
      series: [
        {
          type: 'pie',
          radius: ['40%', '70%'],
          data: this.trends.statusDistribution.map((s) => ({
            name: s.wellStatusName,
            value: s.count,
            itemStyle: { color: s.colorCode },
          })),
        },
      ],
    };

    this.injectionByWellChart = {
      tooltip: { trigger: 'axis' },
      xAxis: {
        type: 'category',
        data: this.trends.injectionByWell.map((w) => w.wellName),
        axisLabel: { rotate: 30 },
      },
      yAxis: { type: 'value', name: 'bbl/day' },
      series: [
        {
          name: 'Injection Rate',
          type: 'bar',
          data: this.trends.injectionByWell.map((w) => w.rate),
          itemStyle: { color: '#1565C0' },
        },
      ],
    };

    this.oilByWellChart = {
      tooltip: { trigger: 'axis' },
      xAxis: {
        type: 'category',
        data: this.trends.oilProductionByWell.map((w) => w.wellName),
        axisLabel: { rotate: 30 },
      },
      yAxis: { type: 'value', name: 'bbl/day' },
      series: [
        {
          name: 'Oil Production',
          type: 'bar',
          data: this.trends.oilProductionByWell.map((w) => w.rate),
          itemStyle: { color: '#2E7D32' },
        },
      ],
    };
  }
}
