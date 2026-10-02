import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

export interface BarItem {
  readonly key: string;
  readonly label: string;
  readonly value: number;
  readonly display: string;
}

@Component({
  selector: 'app-bar-list',
  template: `
    <ul class="bars">
      @for (bar of bars(); track bar.key) {
        <li>
          <div class="row">
            <span>{{ bar.label }}</span
            ><strong>{{ bar.display }}</strong>
          </div>
          <div class="track"><div class="fill" [style.width.%]="bar.percent"></div></div>
        </li>
      }
    </ul>
  `,
  styles: `
    .bars {
      list-style: none;
      margin: 0;
      padding: 0;
      display: grid;
      gap: 0.9rem;
    }
    .row {
      display: flex;
      justify-content: space-between;
      font-size: 0.85rem;
      margin-bottom: 0.35rem;
    }
    .row strong {
      font-variant-numeric: tabular-nums;
    }
    .track {
      height: 0.5rem;
      border-radius: 999px;
      background: var(--surface-2);
      overflow: hidden;
    }
    .fill {
      height: 100%;
      border-radius: inherit;
      background: linear-gradient(90deg, var(--accent), var(--accent-2));
      transition: width 0.6s ease;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BarList {
  readonly items = input.required<readonly BarItem[]>();
  protected readonly bars = computed(() => {
    const max = Math.max(...this.items().map((item) => item.value), 1);
    return this.items().map((item) => ({ ...item, percent: (item.value / max) * 100 }));
  });
}
