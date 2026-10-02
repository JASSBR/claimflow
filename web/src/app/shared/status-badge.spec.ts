import { TestBed } from '@angular/core/testing';
import { StatusBadge } from './status-badge';

describe('StatusBadge', () => {
  it('renders the French label and exposes the status for styling', async () => {
    const fixture = TestBed.createComponent(StatusBadge);
    fixture.componentRef.setInput('status', 'InformationRequested');
    await fixture.whenStable();

    const badge = fixture.nativeElement.querySelector('.badge') as HTMLElement;
    expect(badge.textContent?.trim()).toBe('Pièces demandées');
    expect(badge.dataset['status']).toBe('InformationRequested');
  });
});
