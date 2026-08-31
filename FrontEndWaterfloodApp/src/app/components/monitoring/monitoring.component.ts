import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { WaterfloodService } from '../../shared/services/waterflood.service';
import { WaterfloodThresholdService } from '../../shared/services/threshold.service';
import { ZskReferenceService } from '../../core/services/zsk/zsk-reference.service';
import { WaterfloodRecord } from '../../models/waterflood.model';
import { AlertThreshold } from '../../models/threshold.model';
import { ZskMonitoringRule } from '../../core/services/zsk/zsk-reference.model';
import { HasRoleDirective } from '../../shared/directives/hasRole.directive';
import { Roles } from '../../core/enum/roles.enum';
import { ToastService } from '../../shared/services/toast.service';

@Component({
  selector: 'app-monitoring',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, HasRoleDirective],
  templateUrl: './monitoring.component.html',
  styleUrls: ['./monitoring.component.scss'],
})
export class MonitoringComponent implements OnInit {
  role = Roles;
  alerts: WaterfloodRecord[] = [];
  threshold?: AlertThreshold;
  monitoringRules: ZskMonitoringRule[] = [];
  thresholdForm!: FormGroup;
  isSavingThreshold = false;

  constructor(
    private waterfloodService: WaterfloodService,
    private waterfloodThresholdService: WaterfloodThresholdService,
    private zskReferenceService: ZskReferenceService,
    private fb: FormBuilder,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.thresholdForm = this.fb.group({
      maxWaterCutPercent: [80],
      minOilProductionRate: [500],
      minInjectionRate: [1000],
      maxInjectionPressure: [2500],
      productionDeclinePercent: [20],
    });

    this.loadAlerts();
    this.loadThresholds();
    this.zskReferenceService.getMonitoringRules().subscribe((res) => {
      this.monitoringRules = res.data ?? [];
    });
  }

  loadAlerts(): void {
    this.waterfloodService.getAlerts().subscribe((res) => {
      this.alerts = res.data ?? [];
    });
  }

  loadThresholds(): void {
    this.waterfloodThresholdService.get().subscribe((res) => {
      if (res.data) {
        this.threshold = res.data;
        this.thresholdForm.patchValue(res.data);
      }
    });
  }

  saveThresholds(): void {
    this.isSavingThreshold = true;
    this.waterfloodThresholdService.update(this.thresholdForm.value).subscribe({
      next: (res) => {
        this.threshold = res.data;
        this.isSavingThreshold = false;
        this.toastService.success('Thresholds updated');
        this.loadAlerts();
      },
      error: () => {
        this.isSavingThreshold = false;
        this.toastService.error('Failed to update thresholds');
      },
    });
  }
}
