import { parseAnalysis, parseInlines } from './analysis-parser';

describe('parseAnalysis', () => {
  it('turns the model output into typed blocks, never HTML', () => {
    const blocks = parseAnalysis(
      '## Synthèse\nDossier cohérent[[1]].\n\n## Incohérences relevées\n- Choc **avant droit**[[2]] vs arrière[[3]]\n* Aucune',
    );

    expect(blocks.map((block) => block.kind)).toEqual([
      'heading',
      'paragraph',
      'heading',
      'bullet',
      'bullet',
    ]);
    expect(blocks[0]).toEqual({ kind: 'heading', text: 'Synthèse' });
    expect(blocks[3]).toEqual({
      kind: 'bullet',
      inlines: [
        { kind: 'text', text: 'Choc ', bold: false },
        { kind: 'text', text: 'avant droit', bold: true },
        { kind: 'cite', number: 2 },
        { kind: 'text', text: ' vs arrière', bold: false },
        { kind: 'cite', number: 3 },
      ],
    });
  });

  it('keeps markup-looking text as plain text', () => {
    expect(parseInlines('<img src=x onerror=alert(1)>')).toEqual([
      { kind: 'text', text: '<img src=x onerror=alert(1)>', bold: false },
    ]);
  });

  it('drops citation markers from headings', () => {
    expect(parseAnalysis('### Recommandation[[4]]')).toEqual([
      { kind: 'heading', text: 'Recommandation' },
    ]);
  });
});
