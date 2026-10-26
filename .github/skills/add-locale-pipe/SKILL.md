---
name: add-locale-pipe
description: "Implement an Angular standalone formatting pipe that leverages injected LOCALE_ID for localized rendering."
---

> Evidence: value gate verified the facts below against this repository.

Implement an Angular standalone formatting pipe that leverages injected LOCALE_ID for localized rendering.

Applies in: `claimflow-web`.

## What is specific to this repository

- The repository defines RelativeTimePipe which initializes an Intl.RelativeTimeFormat instance directly with inject(LOCALE_ID) and numeric set to 'auto'. [web/src/app/shared/relative-time.pipe.ts:L14-L14]

## Files you will probably also touch

| File | Why |
|---|---|
| `web/src/app/pages/dashboard/dashboard.ts` | referenced by or referencing this skill's files |
| `web/src/app/shared/relative-time.pipe.spec.ts` | referenced by or referencing this skill's files |

## Start here

`web/src/app/shared/relative-time.pipe.ts` [web/src/app/shared/relative-time.pipe.ts:L9-L25] is the worked example for this change, as it stands in the repository:

```typescript
];

/** "il y a 5 minutes" / "5 minutes ago", in the build's locale. */
@Pipe({ name: 'relativeTime' })
export class RelativeTimePipe implements PipeTransform {
  private readonly formatter = new Intl.RelativeTimeFormat(inject(LOCALE_ID), { numeric: 'auto' });

  transform(value: string | Date | null | undefined, now: Date = new Date()): string {
    if (!value) return '';
    let delta = (new Date(value).getTime() - now.getTime()) / 1000;
    for (const [unit, size] of STEPS) {
      if (Math.abs(delta) < size) return this.formatter.format(Math.round(delta), unit);
      delta /= size;
    }
    return this.formatter.format(Math.round(delta), 'year');
  }
}
```

## Patterns

Required:
- `@Pipe`
- `PipeTransform`
- `inject(LOCALE_ID)`
Forbidden:
- Avoid `constructor(private locale` in this change.

## Workflow

1. Define an Angular pipe implementing PipeTransform with a concise pipe name. [web/src/app/shared/relative-time.pipe.ts:L1-L1]
2. Inject LOCALE_ID using inject(LOCALE_ID). [web/src/app/shared/relative-time.pipe.ts:L1-L1]
3. Initialize the corresponding Intl formatter or localization service with the injected locale. [web/src/app/shared/relative-time.pipe.ts:L3-L3]

## Checklist

- [ ] Verify LOCALE_ID is injected with inject(LOCALE_ID). [web/src/app/shared/relative-time.pipe.ts:L1-L1]
- [ ] Verify internationalization formats according to current locale (e.g. using Intl). [web/src/app/shared/relative-time.pipe.ts:L3-L3]
