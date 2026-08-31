import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { WaterfloodService } from '../../shared/services/waterflood.service';
import { ZskReferenceService } from '../../core/services/zsk/zsk-reference.service';
import {
  WaterfloodRecord,
  CreateWaterfloodRecordDto,
  UpdateWaterfloodRecordDto,
  WaterfloodFilter,
  WATERFLOOD_FIELD_NAMES,
  WATERFLOOD_WELL_TYPE_CODES,
} from '../../models/waterflood.model';
import { ZskReferenceData } from '../../core/services/zsk/zsk-reference.model';
import { Pagination } from '../../models/pagination.model';
import { ToastService } from '../../shared/services/toast.service';
import { HasRoleDirective } from '../../shared/directives/hasRole.directive';
import { ZskStatusBadgeDirective } from '../../core/services/zsk/zsk-status-badge.directive';
import { Roles } from '../../core/enum/roles.enum';
import { saveAs } from 'file-saver';

@Component({
  selector: 'app-wells',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, HasRoleDirective, ZskStatusBadgeDirective],
  templateUrl: './wells.component.html',
  styleUrls: ['./wells.component.scss'],
})
export class WellsComponent implements OnInit {
  Math = Math;
  role = Roles;
  records: WaterfloodRecord[] = [];
  zskReference?: ZskReferenceData;
  pagination: Pagination = { currentPage: 1, itemsPerPage: 10, totalItems: 0, totalPages: 0 };
  recordForm!: FormGroup;
  filterForm!: FormGroup;
  isEditMode = false;
  isFormVisible = false;
  isLoading = false;
  isSubmitting = false;
  sortColumn = 'measurementDate';
  sortDirection = 'desc';

  fieldOptions = WATERFLOOD_FIELD_NAMES.map((f) => ({ value: f, label: f }));
  wellTypeOptions: { value: string; label: string }[] = [];
  wellStatusOptions: { value: string; label: string }[] = [];

  constructor(
    private waterfloodService: WaterfloodService,
    private zskReferenceService: ZskReferenceService,
    private fb: FormBuilder,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.zskReferenceService.getReferenceData().subscribe((res) => {
      this.zskReference = res.data;
      this.wellTypeOptions =
        res.data?.wellTypes.map((t) => ({ value: t.code, label: t.name })) ?? [];
      this.wellStatusOptions =
        res.data?.wellStatuses.map((s) => ({ value: s.code, label: s.name })) ?? [];
    });

    this.initForms();
    this.loadRecords();
  }

  initForms(): void {
    this.recordForm = this.fb.group({
      id: [''],
      wellName: ['', Validators.required],
      wellTypeCode: [WATERFLOOD_WELL_TYPE_CODES.INJECTOR, Validators.required],
      fieldName: ['', Validators.required],
      latitude: [0, [Validators.required, Validators.min(-90), Validators.max(90)]],
      longitude: [0, [Validators.required, Validators.min(-180), Validators.max(180)]],
      injectionRate: [null],
      oilProductionRate: [null],
      waterProductionRate: [null],
      waterCut: [null],
      injectionPressure: [null],
      wellStatusCode: ['ACT', Validators.required],
      measurementDate: ['', Validators.required],
    });

    this.filterForm = this.fb.group({
      search: [''],
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

    this.recordForm.get('wellTypeCode')?.valueChanges.subscribe((code: string) => {
      this.applyWellTypeValidation(code);
    });
  }

  applyWellTypeValidation(wellTypeCode: string): void {
    const injRate = this.recordForm.get('injectionRate');
    const injPress = this.recordForm.get('injectionPressure');
    const oilRate = this.recordForm.get('oilProductionRate');
    const waterRate = this.recordForm.get('waterProductionRate');
    const waterCut = this.recordForm.get('waterCut');

    if (wellTypeCode === WATERFLOOD_WELL_TYPE_CODES.INJECTOR) {
      injRate?.setValidators([Validators.required, Validators.min(0)]);
      injPress?.setValidators([Validators.required, Validators.min(0)]);
      oilRate?.setValidators([Validators.min(0)]);
      waterRate?.setValidators([Validators.min(0)]);
      waterCut?.setValidators([Validators.min(0), Validators.max(100)]);
    } else {
      oilRate?.setValidators([Validators.required, Validators.min(0)]);
      waterRate?.setValidators([Validators.required, Validators.min(0)]);
      waterCut?.setValidators([Validators.required, Validators.min(0), Validators.max(100)]);
      injRate?.setValidators([Validators.min(0)]);
      injPress?.setValidators([Validators.min(0)]);
    }

    [injRate, injPress, oilRate, waterRate, waterCut].forEach((c) => c?.updateValueAndValidity());
  }

  loadRecords(): void {
    this.isLoading = true;
    const filter: WaterfloodFilter = this.filterForm.value;
    this.waterfloodService
      .getRecords(
        this.pagination.currentPage,
        this.pagination.itemsPerPage,
        this.sortColumn,
        this.sortDirection,
        filter
      )
      .subscribe({
        next: (result) => {
          this.records = result.result?.data ?? [];
          if (result.pagination) {
            this.pagination = {
              currentPage: result.pagination.currentPage,
              itemsPerPage: result.pagination.itemsPerPage,
              totalItems: result.pagination.totalItems,
              totalPages: result.pagination.totalPages,
            };
          }
          this.isLoading = false;
        },
        error: () => {
          this.isLoading = false;
          this.toastService.error('Failed to load waterflood records');
        },
      });
  }

  openCreateForm(): void {
    this.isEditMode = false;
    this.isFormVisible = true;
    this.recordForm.reset({
      wellTypeCode: WATERFLOOD_WELL_TYPE_CODES.INJECTOR,
      wellStatusCode: 'ACT',
      measurementDate: new Date().toISOString().split('T')[0],
    });
    this.applyWellTypeValidation(WATERFLOOD_WELL_TYPE_CODES.INJECTOR);
  }

  openEditForm(record: WaterfloodRecord): void {
    this.isEditMode = true;
    this.isFormVisible = true;
    this.recordForm.patchValue({
      ...record,
      measurementDate: record.measurementDate.split('T')[0],
    });
    this.applyWellTypeValidation(record.wellTypeCode);
  }

  closeForm(): void {
    this.isFormVisible = false;
  }

  submitForm(): void {
    if (this.recordForm.invalid) return;
    this.isSubmitting = true;
    const value = this.recordForm.value;

    const request = this.isEditMode
      ? this.waterfloodService.updateRecord(value as UpdateWaterfloodRecordDto)
      : this.waterfloodService.createRecord(value as CreateWaterfloodRecordDto);

    request.subscribe({
      next: () => {
        this.toastService.success(
          this.isEditMode ? 'Waterflood record updated' : 'Waterflood record created'
        );
        this.isSubmitting = false;
        this.closeForm();
        this.loadRecords();
      },
      error: () => {
        this.isSubmitting = false;
        this.toastService.error('Waterflood operation failed');
      },
    });
  }

  deleteRecord(id: string): void {
    if (!confirm('Delete this waterflood well record?')) return;
    this.waterfloodService.deleteRecord(id).subscribe({
      next: () => {
        this.toastService.success('Waterflood record deleted');
        this.loadRecords();
      },
      error: () => this.toastService.error('Delete failed'),
    });
  }

  exportData(format: 'csv' | 'excel'): void {
    this.waterfloodService.exportData(this.filterForm.value, format).subscribe({
      next: (blob) => {
        saveAs(blob, `waterflood-export.${format === 'excel' ? 'xlsx' : 'csv'}`);
      },
    });
  }

  onPageChange(page: number): void {
    this.pagination.currentPage = page;
    this.loadRecords();
  }

  onSort(column: string): void {
    if (this.sortColumn === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }
    this.loadRecords();
  }

  applyFilter(): void {
    this.pagination.currentPage = 1;
    this.loadRecords();
  }
}
