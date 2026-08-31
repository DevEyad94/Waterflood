import {
  Directive,
  Input,
  ViewContainerRef,
  TemplateRef,
  OnInit,
  OnDestroy
} from "@angular/core";
import { take, takeUntil } from "rxjs/operators";
import { Subject } from "rxjs";
import { AuthService } from "../services/auth.service";
import { User } from "../../models/user.model";

@Directive({
  selector: "[appHasRole]",
  standalone: true
})
export class HasRoleDirective implements OnInit, OnDestroy {
  @Input() appHasRole: string[] = [];
  isVisible = false;
  private destroy$ = new Subject<void>();

  constructor(
    private viewContainerRef: ViewContainerRef,
    private templateRef: TemplateRef<any>,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    // Initial check with user state
    console.log(this.appHasRole);
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(this.updateView.bind(this));
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private updateView(user: User | null | undefined): void {
    // Clear view initially
    this.viewContainerRef.clear();
    this.isVisible = false;

    if (user?.token && this.appHasRole?.length) {
      const hasRequiredRole = this.appHasRole.some(role =>
        user.role?.some((userRole: string) => userRole === role)
      );

      if (hasRequiredRole) {
        this.isVisible = true;
        this.viewContainerRef.createEmbeddedView(this.templateRef);
      }
    }
  }
}
