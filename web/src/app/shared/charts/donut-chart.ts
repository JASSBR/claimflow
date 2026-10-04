import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

export interface DonutSlice {
  readonly key: string;
  readonly label: string;
  readonly value: number;
  readonly color: string;
}

const RADIUS = 42;
const CIRCUMFERENCE = 2 * Math.PI * RADIUS;
// Round caps reach half a stroke past each end: the gap must exceed one stroke width to stay visible.
const GAP = 12;

/** Dependency-free SVG donut: each slice is a dashed circle stroke, so it scales crisply and animates in CSS. */
@Component({
  selector: 'app-donut-chart',
  template: `
    <figure class="donut">
      <svg viewBox="0 0 100 100" role="img" [attr.aria-label]="ariaLabel()">
        <circle class="track" cx="50" cy="50" [attr.r]="radius" />
        @for (arc of arcs(); track arc.key) {
          <circle
            class="arc"
            cx="50"
            cy="50"
            [attr.r]="radius"
            [attr.stroke]="arc.color"
            [attr.stroke-dasharray]="arc.dash"
            [attr.stroke-dashoffset]="arc.offset"
          />
        }
      </svg>
      <figcaption class="center">
        <strong>{{ total() }}</strong>
        <span>{{ caption() }}</span>
      </figcaption>
    </figure>
  `,
  styles: `
    .donut {
      position: relative;
      margin: 0;
      width: 100%;
      max-width: 200px;
      aspect-ratio: 1;
    }
    svg {
      width: 100%;
      height: 100%;
      transform: rotate(-90deg);
    }
    circle {
      fill: none;
      stroke-width: 9;
    }
    .arc {
      stroke-linecap: round;
    }
    .track {
      stroke: var(--surface-2);
    }
    .arc {
      transition:
        stroke-dasharray 0.6s ease,
        stroke-dashoffset 0.6s ease;
    }
    .center {
      position: absolute;
      inset: 0;
      display: grid;
      place-content: center;
      text-align: center;
    }
    .center strong {
      font-size: 2.1rem;
      font-weight: 800;
      letter-spacing: -0.04em;
      line-height: 1;
    }
    .center span {
      font-size: 0.75rem;
      color: var(--muted);
      margin-top: 0.2rem;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DonutChart {
  readonly slices = input.required<readonly DonutSlice[]>();
  readonly caption = input('total');

  protected readonly radius = RADIUS;
  protected readonly total = computed(() =>
    this.slices().reduce((sum, slice) => sum + slice.value, 0),
  );
  protected readonly ariaLabel = computed(() =>
    this.slices()
      .map((s) => `${s.label} : ${s.value}`)
      .join(', '),
  );

  protected readonly arcs = computed(() => {
    const total = this.total() || 1;
    let consumed = 0;
    return this.slices()
      .filter((slice) => slice.value > 0)
      .map((slice) => {
        const length = (slice.value / total) * CIRCUMFERENCE;
        const visible = Math.max(length - GAP, 0.01);
        const arc = {
          key: slice.key,
          color: slice.color,
          dash: `${visible} ${CIRCUMFERENCE}`,
          offset: -consumed,
        };
        consumed += length;
        return arc;
      });
  });
}
