import { DatePipe, DecimalPipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  OnDestroy,
  computed,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { DocumentsApi } from '../../documents/documents-api';
import {
  AnalysisCitation,
  ClaimAnalysis,
  DocumentSettings,
} from '../../documents/documents.models';
import { parseAnalysis } from '../../documents/analysis-parser';
import { Icon } from '../../shared/icon';
import { problemMessages } from '../../shared/problem-details';

const STEPS = [
  $localize`:@@analysis.step1:Lecture des pièces`,
  $localize`:@@analysis.step2:Comparaison avec la déclaration`,
  $localize`:@@analysis.step3:Recherche des incohérences`,
  $localize`:@@analysis.step4:Rédaction de la synthèse`,
];

@Component({
  selector: 'app-analysis-panel',
  imports: [DatePipe, DecimalPipe, Icon],
  templateUrl: './analysis-panel.html',
  styleUrl: './analysis-panel.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AnalysisPanel implements OnDestroy {
  private readonly api = inject(DocumentsApi);

  readonly claimId = input.required<string>();
  readonly analysis = input<ClaimAnalysis | null | undefined>();
  readonly settings = input<DocumentSettings | undefined>();
  readonly documentCount = input(0);
  readonly canAnalyze = input(false);
  readonly analyzed = output<ClaimAnalysis>();

  protected readonly running = signal(false);
  protected readonly step = signal(0);
  protected readonly error = signal<string | null>(null);
  protected readonly selected = signal<AnalysisCitation | null>(null);
  protected readonly steps = STEPS;
  protected readonly sourceLabel = $localize`:@@analysis.source:Source`;

  protected readonly blocks = computed(() => {
    const analysis = this.analysis();
    return analysis && !analysis.refused ? parseAnalysis(analysis.content) : [];
  });

  private ticker: ReturnType<typeof setInterval> | undefined;

  protected citation(number: number): AnalysisCitation | undefined {
    return this.analysis()?.citations.find((citation) => citation.number === number);
  }

  protected toggle(number: number): void {
    const citation = this.citation(number) ?? null;
    this.selected.update((current) => (current?.number === number ? null : citation));
  }

  protected openSource(citation: AnalysisCitation): void {
    void this.api.open(this.claimId(), citation.documentId, citation.startPage);
  }

  protected async run(): Promise<void> {
    this.running.set(true);
    this.error.set(null);
    this.selected.set(null);
    this.step.set(0);
    // The model answers in one go; the steps only tell the user what is happening during the wait.
    this.ticker = setInterval(
      () => this.step.update((step) => Math.min(step + 1, STEPS.length - 1)),
      4000,
    );
    try {
      this.analyzed.emit(await firstValueFrom(this.api.analyze(this.claimId())));
    } catch (error) {
      this.error.set(problemMessages(error).join(' '));
    } finally {
      clearInterval(this.ticker);
      this.running.set(false);
    }
  }

  ngOnDestroy(): void {
    clearInterval(this.ticker);
  }
}
