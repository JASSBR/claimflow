import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ClaimAnalysis } from '../../documents/documents.models';
import { settle } from '../../testing';
import { AnalysisPanel } from './analysis-panel';

const ANALYSIS: ClaimAnalysis = {
  id: 'a1',
  createdAt: '2026-10-02T10:00:00Z',
  requestedBy: 'Léa Martin',
  model: 'claude-opus-5-5',
  refused: false,
  content: '## Incohérences relevées\n- Le constat indique un choc avant droit[[1]].',
  citations: [
    {
      number: 1,
      documentId: 'd1',
      documentTitle: 'Constat amiable.pdf',
      citedText: 'Point de choc initial (A) : Avant droit',
      startPage: 1,
      endPage: 1,
    },
  ],
  documentCount: 3,
  inputTokens: 9_000,
  outputTokens: 700,
};

describe('AnalysisPanel', () => {
  let http: HttpTestingController;

  function render(inputs: Record<string, unknown>) {
    const fixture = TestBed.createComponent(AnalysisPanel);
    fixture.componentRef.setInput('claimId', 'c1');
    for (const [key, value] of Object.entries(inputs)) fixture.componentRef.setInput(key, value);
    fixture.detectChanges();
    return { fixture, element: fixture.nativeElement as HTMLElement };
  }

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('renders the review with clickable citations that reveal the quoted source', () => {
    const { fixture, element } = render({ analysis: ANALYSIS, settings: { aiEnabled: true } });

    expect(element.querySelector('h3')?.textContent).toBe('Incohérences relevées');
    (element.querySelector('button.cite') as HTMLButtonElement).click();
    fixture.detectChanges();

    expect(element.querySelector('.source blockquote')?.textContent).toContain('Avant droit');
    expect(element.querySelector('.source')?.textContent).toContain('page 1');
  });

  it('runs a review and emits the stored result', async () => {
    const { fixture, element } = render({
      settings: { aiEnabled: true },
      canAnalyze: true,
      documentCount: 2,
    });
    const emitted: ClaimAnalysis[] = [];
    fixture.componentInstance.analyzed.subscribe((analysis) => emitted.push(analysis));

    (element.querySelector('button.run') as HTMLButtonElement).click();
    fixture.detectChanges();
    expect(element.querySelector('.progress')).not.toBeNull();
    http.expectOne({ method: 'POST', url: '/api/claims/c1/analysis' }).flush(ANALYSIS);
    await settle();

    expect(emitted).toEqual([ANALYSIS]);
  });

  it('explains why the assistant is unavailable instead of showing a dead button', () => {
    const { element } = render({
      settings: { aiEnabled: false },
      canAnalyze: true,
      documentCount: 2,
    });
    expect(element.textContent).toContain("n'est pas activé");
    expect(element.querySelector('button.run')).toBeNull();
  });
});
