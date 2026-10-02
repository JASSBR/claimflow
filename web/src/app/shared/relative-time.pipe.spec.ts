import { RelativeTimePipe } from './relative-time.pipe';

describe('RelativeTimePipe', () => {
  const now = new Date('2026-10-02T12:00:00Z');
  const pipe = new RelativeTimePipe();

  it('formats recent moments in French', () => {
    expect(pipe.transform('2026-10-02T11:55:00Z', now)).toBe('il y a 5 minutes');
    expect(pipe.transform('2026-10-01T12:00:00Z', now)).toBe('hier');
    expect(pipe.transform(null, now)).toBe('');
  });
});
