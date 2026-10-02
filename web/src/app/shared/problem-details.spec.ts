import { HttpErrorResponse } from '@angular/common/http';
import { isConflict, problemMessages } from './problem-details';

describe('problemMessages', () => {
  it('lists every violated business rule of a validation problem', () => {
    const error = new HttpErrorResponse({
      status: 400,
      error: { title: 'Invalid', errors: { a: ['Rule A'], b: ['Rule B1', 'Rule B2'] } },
    });

    expect(problemMessages(error)).toEqual(['Rule A', 'Rule B1', 'Rule B2']);
  });

  it('falls back to the problem title', () => {
    const error = new HttpErrorResponse({
      status: 409,
      error: { title: 'Modified by someone else.' },
    });

    expect(problemMessages(error)).toEqual(['Modified by someone else.']);
    expect(isConflict(error)).toBe(true);
  });

  it('explains a network failure instead of showing status 0', () => {
    expect(problemMessages(new HttpErrorResponse({ status: 0 }))[0]).toContain('injoignable');
  });

  it('handles non-HTTP errors', () => {
    expect(problemMessages(new Error('boom'))).toEqual(['Erreur inattendue.']);
    expect(isConflict(new Error('boom'))).toBe(false);
  });
});
