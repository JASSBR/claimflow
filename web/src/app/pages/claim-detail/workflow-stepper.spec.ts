import { TestBed } from '@angular/core/testing';
import { WorkflowStepper } from './workflow-stepper';

describe('WorkflowStepper', () => {
  async function states(status: string) {
    const fixture = TestBed.createComponent(WorkflowStepper);
    fixture.componentRef.setInput('status', status);
    await fixture.whenStable();
    return Array.from((fixture.nativeElement as HTMLElement).querySelectorAll('li')).map(
      (li) => `${li.dataset['state']}:${li.textContent?.replace(/\s+/g, ' ').trim()}`,
    );
  }

  it('marks progress along the happy path', async () => {
    expect(await states('Approved')).toEqual([
      'done:Déclaré',
      'done:En instruction',
      'current:3 Accepté',
      'todo:4 Indemnisé',
    ]);
  });

  it('shows a pending information request as the review step', async () => {
    expect((await states('InformationRequested'))[1]).toBe('current:2 Pièces demandées');
  });

  it('ends the path on a rejection', async () => {
    expect(await states('Rejected')).toEqual([
      'done:Déclaré',
      'done:En instruction',
      'rejected:Refusé',
    ]);
  });
});
