---
name: add-chart-component
description: "Add a dependency-free SVG or CSS visualization chart component in shared/charts with signal inputs and OnPush change detection."
---

> Evidence: value gate verified the facts below against this repository.

Add a dependency-free SVG or CSS visualization chart component in shared/charts with signal inputs and OnPush change detection.

Applies in: `claimflow-web`.

## What is specific to this repository

- DonutSlice defines the slice data model with readonly key, label, value, and color properties. [web/src/app/shared/charts/donut-chart.ts:L3-L8]
- BarItem defines the item model with readonly key, label, value, and display properties. [web/src/app/shared/charts/bar-list.ts:L3-L8]

## Rules this change touches

- [Invariant] The rendered stroke length of an SVG donut arc slice is always at least 0.01 units. (approved, conviction 1.00, web/src/app/shared/charts/donut-chart.ts:L93-L93)
- [Invariant] The divisor max in the percentage calculation is always at least 1, preventing division by zero even if all item values are 0 or empty. (approved, conviction 1.00, web/src/app/shared/charts/bar-list.ts:L53-L53)
- [BusinessRule] The donut chart total is calculated as the sum of all slice values using this.slices().reduce((sum, slice) => sum + slice.value, 0). (approved, conviction 1.00, web/src/app/shared/charts/donut-chart.ts:L78-L80)
- [BusinessRule] The width percentage of each bar is calculated as (item.value / max) * 100, where max is the highest value among items, with a minimum floor of 1. (approved, conviction 1.00, web/src/app/shared/charts/bar-list.ts:L52-L54)
- [BusinessRule] Slices with a value of 0 or less are filtered out from arc rendering. For each positive slice, proportional arc length is calculated based on slice.value / total * CIRCUMFERENCE (falling back to total = 1 if the sum is 0), with visible length set to Math.max(length - GAP, 0.01) and offset set to the negative cumulative consumed length. (approved, conviction 1.00, web/src/app/shared/charts/donut-chart.ts:L87-L101)
- [InformalDecision] BarList uses ChangeDetectionStrategy.OnPush for fine-grained reactivity and rendering optimization. (approved, conviction 1.00, web/src/app/shared/charts/bar-list.ts:L49-L49)
- [InformalDecision] Donut chart arcs are rendered using native SVG circles with stroke-dasharray and stroke-dashoffset transitions rather than an external charting library. (approved, conviction 1.00, web/src/app/shared/charts/donut-chart.ts:L45-L49)

## Context

- DonutSlice is an interface defining a single data slice for the donut chart, with readonly properties: key (string), label (string), value (number), and color (string). [web/src/app/shared/charts/donut-chart.ts:L3-L8]
- DonutChart defines two Angular component inputs: a required 'slices' input of type readonly DonutSlice[], and an optional 'caption' input defaulting to 'total'. [web/src/app/shared/charts/donut-chart.ts:L74-L75]
- DonutChart is an Angular standalone component with selector 'app-donut-chart' and ChangeDetectionStrategy.OnPush, rendering a dependency-free SVG donut chart where slices are represented as dashed circle strokes rotated -90 degrees. [web/src/app/shared/charts/donut-chart.ts:L72-L73]

## Files you will probably also touch

| File | Why |
|---|---|
| `web/src/app/pages/dashboard/dashboard.ts` | referenced by or referencing this skill's files |
| `web/src/app/shared/charts/donut-chart.spec.ts` | referenced by or referencing this skill's files |

## Start here

`web/src/app/shared/charts/bar-list.ts` [web/src/app/shared/charts/bar-list.ts:L52-L63] is the worked example for this change, as it stands in the repository:

```typescript
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
```

## The same change elsewhere in this repository

- `web/src/app/shared/charts/donut-chart.ts` [web/src/app/shared/charts/donut-chart.ts:L3-L3]: `export interface DonutSlice {`
- `web/src/app/shared/charts/donut-chart.ts` [web/src/app/shared/charts/donut-chart.ts:L87-L87]: `changeDetection: ChangeDetectionStrategy.OnPush,`
- `web/src/app/shared/charts/donut-chart.ts` [web/src/app/shared/charts/donut-chart.ts:L89-L89]: `export class DonutChart {`
- `web/src/app/shared/charts/donut-chart.ts` [web/src/app/shared/charts/donut-chart.ts:L90-L90]: `  readonly slices = input.required<readonly DonutSlice[]>();`

## Patterns

Required:
- `ChangeDetectionStrategy.OnPush`
- `computed`
- `input.required`
Forbidden:
- Avoid `Chart.js` in this change.
- Avoid `d3` in this change.
- Avoid `ngOnInit` in this change.

## Workflow

1. Define a readonly item interface in web/src/app/shared/charts/ specifying key, label, and value fields [web/src/app/shared/charts/donut-chart.ts:L3-L3]
2. Create a standalone component with an app- prefixed selector and ChangeDetectionStrategy.OnPush [web/src/app/shared/charts/bar-list.ts:L1-L1]
3. Provide inline template and styles for dependency-free SVG or HTML/CSS rendering [web/src/app/shared/charts/donut-chart.ts:L15-L15]
4. Declare the input dataset using input.required<readonly ...[]>() [web/src/app/shared/charts/bar-list.ts:L58-L58]
5. Declare component inputs using input() or input.required(). [web/src/app/shared/charts/bar-list.ts:L58-L58]
6. Compute chart dimensions, percentages, or SVG dash geometries with computed(). [web/src/app/shared/charts/donut-chart.ts:L15-L15]

## Checklist

- [ ] Set changeDetection to ChangeDetectionStrategy.OnPush [web/src/app/shared/charts/bar-list.ts:L1-L1]
- [ ] Use input.required<readonly ...[]>() for the data input [web/src/app/shared/charts/bar-list.ts:L58-L58]
- [ ] Implement inline dependency-free SVG or CSS in template and styles [web/src/app/shared/charts/donut-chart.ts:L15-L15]
- [ ] Verify that ChangeDetectionStrategy.OnPush is specified in the @Component decorator. [web/src/app/shared/charts/bar-list.ts:L1-L1]
- [ ] Verify inputs are declared with Angular signal input/input.required. [web/src/app/shared/charts/bar-list.ts:L58-L58]
- [ ] Verify that SVG attributes or element styling scale responsively without third-party chart dependencies. [web/src/app/shared/charts/donut-chart.ts:L15-L15]
