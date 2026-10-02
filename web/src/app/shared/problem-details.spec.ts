import { HttpErrorResponse } from '@angular/common/http';
import { isConflict, problemMessages } from './problem-details';

describe('problemMessages', () => {
  it('lists every violated business rule of a validation problem', () => {
    const error = new HttpErrorResponse({
      status: 400,
      error: { errors: { a: ['Rule A'], b: ['Rule B1', 'Rule B2'] } },
    });
    expect(problemMessages(error)).toEqual(['Rule A', 'Rule B1', 'Rule B2']);
  });

  it('falls back to the problem title and recognises conflicts', () => {
    const error = new HttpErrorResponse({
      status: 409,
      error: { title: 'Modified by someone else.' },
    });
    expect(problemMessages(error)).toEqual(['Modified by someone else.']);
    expect(isConflict(error)).toBe(true);
  });

  it('translates network failures and rate limiting into actionable messages', () => {
    expect(problemMessages(new HttpErrorResponse({ status: 0 }))[0]).toContain('injoignable');
    expect(problemMessages(new HttpErrorResponse({ status: 429 }))[0]).toContain(
      'Trop de demandes',
    );
  });

  it('handles non-HTTP errors', () => {
    expect(problemMessages(new Error('boom'))).toEqual(['Erreur inattendue.']);
    expect(isConflict(new Error('boom'))).toBe(false);
  });
});
