import { DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, input, output, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ToastService } from '../../core/toast';
import { DocumentsApi } from '../../documents/documents-api';
import { ClaimDocument, DocumentSettings } from '../../documents/documents.models';
import { Icon } from '../../shared/icon';
import { problemMessages } from '../../shared/problem-details';

@Component({
  selector: 'app-documents-panel',
  imports: [DatePipe, DecimalPipe, Icon],
  templateUrl: './documents-panel.html',
  styleUrl: './documents-panel.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DocumentsPanel {
  private readonly api = inject(DocumentsApi);
  private readonly toasts = inject(ToastService);

  readonly claimId = input.required<string>();
  readonly documents = input.required<readonly ClaimDocument[]>();
  readonly settings = input<DocumentSettings | undefined>();
  readonly canUpload = input(false);
  readonly uploaded = output<void>();

  protected readonly dragging = signal(false);
  protected readonly uploading = signal(0);

  protected onDrop(event: DragEvent): void {
    event.preventDefault();
    this.dragging.set(false);
    void this.upload(event.dataTransfer?.files);
  }

  protected onPick(event: Event): void {
    const input = event.target as HTMLInputElement;
    void this.upload(input.files).finally(() => (input.value = ''));
  }

  protected open(document: ClaimDocument): void {
    void this.api.open(this.claimId(), document.id);
  }

  private async upload(files: FileList | null | undefined): Promise<void> {
    if (!files?.length || !this.canUpload()) return;
    for (const file of Array.from(files)) {
      this.uploading.update((count) => count + 1);
      try {
        await firstValueFrom(this.api.upload(this.claimId(), file));
        this.toasts.show({
          tone: 'success',
          title: $localize`:@@toast.uploaded:${file.name}:file: ajouté au dossier`,
        });
      } catch (error) {
        this.toasts.show({
          tone: 'error',
          title: $localize`:@@toast.uploadRejected:${file.name}:file: refusé`,
          message: problemMessages(error).join(' '),
        });
      } finally {
        this.uploading.update((count) => count - 1);
      }
    }
    this.uploaded.emit();
  }
}
