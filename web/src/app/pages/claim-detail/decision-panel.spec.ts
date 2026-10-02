import { TestBed } from '@angular/core/testing';
import { claimDetails } from '../../testing';
import { DecisionPanel, DecisionRequest } from './decision-panel';

describe('DecisionPanel', () => {
  function render(inputs: Record<string, unknown>) {
    const fixture = TestBed.createComponent(DecisionPanel);
    fixture.componentRef.setInput('userName', 'Karim Benali');
    for (const [key, value] of Object.entries(inputs)) fixture.componentRef.setInput(key, value);
    const emitted: DecisionRequest[] = [];
    fixture.componentInstance.decide.subscribe((decision) => emitted.push(decision));
    fixture.detectChanges();
    const element = fixture.nativeElement as HTMLElement;
    const button = (label: string) =>
      Array.from(element.querySelectorAll('button')).find((b) => b.textContent?.includes(label))!;
    return { fixture, element, emitted, button };
  }

  it('renders exactly the actions the server allows', () => {
    const { element } = render({ claim: claimDetails() });
    expect(
      Array.from(element.querySelectorAll('.actions button')).map((b) => b.textContent?.trim()),
    ).toEqual(['Demander des pièces', 'Accepter', 'Refuser']);
  });

  it('emits a one-click decision immediately', () => {
    const { emitted, button } = render({
      claim: claimDetails({ status: 'Declared', allowedActions: ['StartReview'] }),
    });
    button('Prendre en charge').click();
    expect(emitted).toEqual([{ action: 'StartReview' }]);
  });

  it('asks for the amount before approving and warns above the delegated limit', () => {
    const { fixture, element, emitted, button } = render({
      claim: claimDetails({ claimedAmount: 15_000 }),
      approvalLimit: 10_000,
    });
    button('Accepter').click();
    fixture.detectChanges();

    expect(element.querySelector('.limit.over')?.textContent).toContain(
      'un responsable doit accepter',
    );
    button('Accepter').click();
    expect(emitted).toEqual([{ action: 'Approve', reason: undefined, approvedAmount: 15_000 }]);
  });

  it('explains the four-eyes rule to the approver instead of silently hiding the payment', () => {
    const claim = claimDetails({
      status: 'Approved',
      allowedActions: [],
      history: [
        {
          from: 'UnderReview',
          to: 'Approved',
          action: 'Approve',
          actorName: 'Karim Benali',
          reason: null,
          occurredAt: '2026-10-01T10:00:00Z',
        },
      ],
    });
    const { element } = render({ claim, canSettle: true });
    expect(element.querySelector('.four-eyes')?.textContent).toContain('Principe des quatre yeux');
  });

  it('tells a handler that payments are a manager’s job, even on a claim she approved', () => {
    const claim = claimDetails({
      status: 'Approved',
      allowedActions: [],
      history: [
        {
          from: 'UnderReview',
          to: 'Approved',
          action: 'Approve',
          actorName: 'Karim Benali',
          reason: null,
          occurredAt: '2026-10-01T10:00:00Z',
        },
      ],
    });
    const { element } = render({ claim, canSettle: false });
    expect(element.querySelector('.four-eyes')).toBeNull();
    expect(element.textContent).toContain('réservé aux responsables');
  });

  it('shows a read-only notice to auditors', () => {
    const { element } = render({ claim: claimDetails(), readOnly: true });
    expect(element.querySelector('.actions')).toBeNull();
    expect(element.textContent).toContain('lecture seule');
  });
});
