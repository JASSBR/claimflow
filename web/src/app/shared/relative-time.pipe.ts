import { Pipe, PipeTransform } from '@angular/core';

const formatter = new Intl.RelativeTimeFormat('fr', { numeric: 'auto' });
const STEPS: readonly [Intl.RelativeTimeFormatUnit, number][] = [
  ['second', 60],
  ['minute', 60],
  ['hour', 24],
  ['day', 30],
  ['month', 12],
];

/** "il y a 5 minutes". Pure: re-evaluated when its input changes, which realtime refreshes do often enough. */
@Pipe({ name: 'relativeTime' })
export class RelativeTimePipe implements PipeTransform {
  transform(value: string | Date | null | undefined, now: Date = new Date()): string {
    if (!value) return '';
    let delta = (new Date(value).getTime() - now.getTime()) / 1000;
    for (const [unit, size] of STEPS) {
      if (Math.abs(delta) < size) return formatter.format(Math.round(delta), unit);
      delta /= size;
    }
    return formatter.format(Math.round(delta), 'year');
  }
}
