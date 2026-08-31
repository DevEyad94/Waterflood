import { Pipe, PipeTransform } from '@angular/core';
import { ZskReferenceData } from './zsk-reference.model';

@Pipe({
  name: 'zskWellTypeLabel',
  standalone: true,
  pure: true,
})
export class ZskWellTypeLabelPipe implements PipeTransform {
  transform(code: string, reference?: ZskReferenceData): string {
    if (!reference) return code;
    return reference.wellTypes.find((t) => t.code === code)?.name ?? code;
  }
}
