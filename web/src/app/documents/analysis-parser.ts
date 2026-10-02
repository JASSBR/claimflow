/** A review rendered from the model's Markdown subset: headings, bullets, paragraphs, **bold** and [[n]] citations. */
export type Inline =
  | { readonly kind: 'text'; readonly text: string; readonly bold: boolean }
  | { readonly kind: 'cite'; readonly number: number };

export type Block =
  | { readonly kind: 'heading'; readonly text: string }
  | { readonly kind: 'bullet'; readonly inlines: readonly Inline[] }
  | { readonly kind: 'paragraph'; readonly inlines: readonly Inline[] };

const TOKEN = /\[\[(\d+)\]\]|\*\*(.+?)\*\*/g;

/** Pure and unit-tested: the model's text is never injected as HTML, only rendered through these typed nodes. */
export function parseAnalysis(content: string): Block[] {
  return content
    .split('\n')
    .map((line) => line.trim())
    .filter((line) => line.length > 0)
    .map((line): Block => {
      if (line.startsWith('#'))
        return { kind: 'heading', text: line.replace(/^#+\s*/, '').replace(TOKEN, '') };
      if (/^[-*•]\s/.test(line))
        return { kind: 'bullet', inlines: parseInlines(line.replace(/^[-*•]\s+/, '')) };
      return { kind: 'paragraph', inlines: parseInlines(line) };
    });
}

export function parseInlines(text: string): Inline[] {
  const inlines: Inline[] = [];
  let cursor = 0;
  for (const match of text.matchAll(TOKEN)) {
    if (match.index > cursor)
      inlines.push({ kind: 'text', text: text.slice(cursor, match.index), bold: false });
    if (match[1]) inlines.push({ kind: 'cite', number: Number(match[1]) });
    else inlines.push({ kind: 'text', text: match[2]!, bold: true });
    cursor = match.index + match[0].length;
  }
  if (cursor < text.length) inlines.push({ kind: 'text', text: text.slice(cursor), bold: false });
  return inlines;
}
