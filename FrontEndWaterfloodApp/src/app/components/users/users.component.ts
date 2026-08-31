import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../shared/services/auth.service';
import { ToastService } from '../../shared/services/toast.service';
import { RoleListItem, User, UserRegisterDto, UserUpdateDto } from '../../models/user.model';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.scss'],
})
export class UsersComponent implements OnInit {
  users: User[] = [];
  roles: RoleListItem[] = [];
  userForm!: FormGroup;
  isFormVisible = false;
  isEditMode = false;
  isSubmitting = false;

  constructor(
    private authService: AuthService,
    private fb: FormBuilder,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.userForm = this.fb.group({
      userID: [null],
      username: ['', Validators.required],
      fullName: ['', Validators.required],
      passwordHash: [''],
      zRoleId: [null, Validators.required],
    });
    this.loadUsers();
    this.authService.getRoles().subscribe((res) => {
      this.roles = res.data ?? [];
    });
  }

  loadUsers(): void {
    this.authService.getUsers().subscribe({
      next: (res) => (this.users = res.data ?? []),
      error: () => this.toastService.error('Failed to load users'),
    });
  }

  openCreateForm(): void {
    this.isEditMode = false;
    this.isFormVisible = true;
    this.userForm.reset();
    this.userForm.get('username')?.enable();
    this.userForm.get('passwordHash')?.setValidators([Validators.required, Validators.minLength(6)]);
    this.userForm.get('passwordHash')?.updateValueAndValidity();
  }

  openEditForm(user: User): void {
    this.isEditMode = true;
    this.isFormVisible = true;
    const roleName = user.role?.[0];
    const role = this.roles.find((r) => r.name === roleName);
    this.userForm.patchValue({
      userID: user.userID,
      username: user.username,
      fullName: user.fullName,
      passwordHash: '',
      zRoleId: role?.zRoleId ?? null,
    });
    this.userForm.get('username')?.disable();
    this.userForm.get('passwordHash')?.clearValidators();
    this.userForm.get('passwordHash')?.updateValueAndValidity();
  }

  closeForm(): void {
    this.isFormVisible = false;
    this.userForm.get('username')?.enable();
  }

  submitForm(): void {
    if (this.userForm.invalid) return;
    this.isSubmitting = true;
    const raw = this.userForm.getRawValue();

    if (this.isEditMode) {
      const dto: UserUpdateDto = {
        userID: raw.userID,
        fullName: raw.fullName,
        zRoleId: raw.zRoleId,
      };
      if (raw.passwordHash) dto.passwordHash = raw.passwordHash;
      this.authService.modifyUser(dto).subscribe({
        next: () => {
          this.toastService.success('User updated');
          this.isSubmitting = false;
          this.closeForm();
          this.loadUsers();
        },
        error: () => {
          this.isSubmitting = false;
          this.toastService.error('Failed to update user');
        },
      });
      return;
    }

    const createDto: UserRegisterDto = {
      username: raw.username,
      passwordHash: raw.passwordHash,
      fullName: raw.fullName,
      zRoleId: raw.zRoleId,
    };
    this.authService.register(createDto).subscribe({
      next: () => {
        this.toastService.success('User created');
        this.isSubmitting = false;
        this.closeForm();
        this.loadUsers();
      },
      error: () => {
        this.isSubmitting = false;
        this.toastService.error('Failed to create user');
      },
    });
  }

  roleLabel(user: User): string {
    return user.role?.join(', ') || '—';
  }
}
