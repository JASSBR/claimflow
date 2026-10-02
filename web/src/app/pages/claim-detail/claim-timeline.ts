import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { ACTION_META, STATUS_LABELS } from '../../claims/claim-labels';
import { ClaimDetails } from '../../claims/claim.models';
import { Avatar } from '../../shared/avatar';

@Component({
  selector: 'app-claim-timeline',
  imports: [DatePipe, Avatar],
  template: `
    <ol class="timeline">
      @for (entry of claim().history.slice().reverse(); track entry.occurredAt + entry.action) {
        <li>
          <app-avatar [name]="entry.actorName" [size]="30" />
          <div>
            <p>
              <strong>{{ entry.actorName }}</strong> · {{ actions[entry.action].label }} →
              <span class="to">{{ statuses[entry.to] }}</span>
            </p>
            @if (entry.reason) {
              <q>{{ entry.reason }}</q>
            }
            <time [attr.datetime]="entry.occurredAt">{{ entry.occurredAt | date: 'short' }}</time>
          </div>
        </li>
      }
      <li>
        <app-avatar [name]="claim().declaredBy" [size]="30" />
        <div>
          <p>
            <strong>{{ claim().declaredBy }}</strong> ·
            <span i18n="@@timeline.declared">Déclaration du sinistre</span>
          </p>
          <time [attr.datetime]="claim().declaredAt">{{ claim().declaredAt | date: 'short' }}</time>
        </div>
      </li>
    </ol>
  `,
  styles: `
    .timeline {
      list-style: none;
      margin: 0;
      padding: 0;
      display: grid;
      gap: 1rem;
      position: relative;
    }
    .timeline::before {
      content: '';
      position: absolute;
      left: 14px;
      top: 8px;
      bottom: 8px;
      width: 2px;
      background: var(--border);
    }
    li {
      display: flex;
      gap: 0.75rem;
      position: relative;
    }
    li app-avatar {
      box-shadow: 0 0 0 4px var(--surface);
      border-radius: 50%;
    }
    p {
      margin: 0;
      font-size: 0.88rem;
    }
    .to {
      font-weight: 600;
      color: var(--accent);
    }
    q {
      display: block;
      margin: 0.25rem 0;
      padding: 0.4rem 0.65rem;
      border-left: 3px solid var(--border);
      color: var(--muted);
      font-style: italic;
      font-size: 0.85rem;
    }
    time {
      font-size: 0.75rem;
      color: var(--muted);
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClaimTimeline {
  readonly claim = input.required<ClaimDetails>();
  protected readonly actions = ACTION_META;
  protected readonly statuses = STATUS_LABELS;
}
