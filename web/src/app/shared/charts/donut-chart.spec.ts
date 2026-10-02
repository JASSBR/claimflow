import { TestBed } from '@angular/core/testing';
import { DonutChart } from './donut-chart';

describe('DonutChart', () => {
  it('draws one arc per non-empty slice, proportional to its value', async () => {
    const fixture = TestBed.createComponent(DonutChart);
    fixture.componentRef.setInput('slices', [
      { key: 'a', label: 'A', value: 3, color: 'red' },
      { key: 'b', label: 'B', value: 0, color: 'blue' },
      { key: 'c', label: 'C', value: 1, color: 'green' },
    ]);
    await fixture.whenStable();

    const arcs = Array.from(
      fixture.nativeElement.querySelectorAll('circle.arc'),
    ) as SVGCircleElement[];
    const lengths = arcs.map((arc) => Number(arc.getAttribute('stroke-dasharray')!.split(' ')[0]));
    expect(arcs).toHaveLength(2);
    expect(lengths[0]! / lengths[1]!).toBeCloseTo(3, 0);
    expect(fixture.nativeElement.querySelector('.center strong').textContent).toBe('4');
    expect(fixture.nativeElement.querySelector('svg').getAttribute('aria-label')).toBe(
      'A : 3, B : 0, C : 1',
    );
  });
});
