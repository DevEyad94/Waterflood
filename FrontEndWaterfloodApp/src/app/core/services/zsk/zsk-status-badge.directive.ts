import { Directive, ElementRef, Input, OnChanges, Renderer2 } from '@angular/core';
import { ZskReferenceData } from './zsk-reference.model';

@Directive({
  selector: '[zskStatusBadge]',
  standalone: true,
})
export class ZskStatusBadgeDirective implements OnChanges {
  @Input('zskStatusBadge') statusCode = '';
  @Input() zskReference?: ZskReferenceData;

  constructor(
    private el: ElementRef<HTMLElement>,
    private renderer: Renderer2
  ) {}

  ngOnChanges(): void {
    const status = this.zskReference?.wellStatuses.find(
      (s) => s.code === this.statusCode
    );
    const label = status?.name ?? this.statusCode;
    const color = status?.colorCode ?? '#6c757d';

    this.renderer.setProperty(this.el.nativeElement, 'textContent', label);
    this.renderer.setStyle(this.el.nativeElement, 'backgroundColor', color);
    this.renderer.setStyle(this.el.nativeElement, 'color', '#fff');
    this.renderer.setStyle(this.el.nativeElement, 'padding', '2px 8px');
    this.renderer.setStyle(this.el.nativeElement, 'borderRadius', '4px');
    this.renderer.setStyle(this.el.nativeElement, 'fontSize', '12px');
    this.renderer.setStyle(this.el.nativeElement, 'display', 'inline-block');
  }
}
